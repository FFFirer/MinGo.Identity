import http.server, json, os, pathlib, socket, subprocess, threading, time, urllib.request, urllib.error

root = pathlib.Path(__file__).resolve().parents[1]
(root/'.packages').mkdir(exist_ok=True)
requests = []
hang = False
class Provider(http.server.BaseHTTPRequestHandler):
    def log_message(self, *args): pass
    def do_GET(self):
        requests.append((self.path, self.headers.get('Host'), self.headers.get('X-Forwarded-Proto')))
        if self.path.endswith('openid-configuration'):
            payload = {'issuer':'https://auth.example.test/master', 'authorization_endpoint':'https://auth.example.test/master/authorization', 'token_endpoint':'https://auth.example.test/master/token', 'jwks_uri':'https://auth.example.test/master/jwks', 'response_types_supported':['code'], 'subject_types_supported':['public'], 'id_token_signing_alg_values_supported':['RS256']}
        elif self.path.endswith('jwks'): payload = {'keys':[]}
        else: self.send_error(404); return
        data = json.dumps(payload).encode()
        self.send_response(200); self.send_header('Content-Type','application/json'); self.send_header('Content-Length',str(len(data))); self.end_headers()
        if hang: time.sleep(4)
        try: self.wfile.write(data)
        except (BrokenPipeError, ConnectionResetError, ConnectionAbortedError): pass
class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, *args): return None
server = http.server.ThreadingHTTPServer(('127.0.0.1',0),Provider)
threading.Thread(target=server.serve_forever,daemon=True).start()
def run_case(body_hang):
    global hang
    hang = body_hang
    requests.clear()
    sock = socket.socket(); sock.bind(('127.0.0.1',0)); port=sock.getsockname()[1]; sock.close()
    env=os.environ.copy()
    env.update({'ASPNETCORE_URLS':f'http://127.0.0.1:{port}', 'ASPNETCORE_ENVIRONMENT':'Production', 'IdserverBaseUrl':'https://auth.example.test', 'BackchannelBaseUrl':f'http://127.0.0.1:{server.server_port}', 'BackchannelTimeoutSeconds':'1' if body_hang else '5', 'Serilog__WriteTo__0__Name':'Console', 'DataProtectionPath':str(root/'.packages'/'smoke-keys'), 'IsRealmEnabled':'true', 'IsForwardedEnabled':'true'})
    logfile=root/'.packages'/('oidc-smoke-hang.log' if body_hang else 'oidc-smoke-success.log')
    with logfile.open('w',encoding='utf-8') as log:
        process=subprocess.Popen(['dotnet',str(root/'src/MinGo.IdServer.Admin/bin/Release/net10.0/MinGo.IdServer.Admin.dll')],cwd=root/'src/MinGo.IdServer.Admin',env=env,stdout=log,stderr=subprocess.STDOUT,creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
        try:
            for i in range(80):
                if process.poll() is not None: raise AssertionError('Admin exited; inspect '+str(logfile))
                try:
                    with socket.create_connection(('127.0.0.1',port),timeout=.1): break
                except OSError: time.sleep(.1)
            start=time.monotonic()
            opener=urllib.request.build_opener(NoRedirect)
            try: response=opener.open(f'http://127.0.0.1:{port}/auth',timeout=12)
            except urllib.error.HTTPError as e: response=e
            elapsed=time.monotonic()-start
            body=response.read().decode()
            if body_hang:
                assert response.status==500, response.status
                assert elapsed < 6, elapsed
                assert '参考编号' in body, body
            else:
                assert response.status==302, (response.status,body)
                assert response.headers['Location'].startswith('https://auth.example.test/master/authorization?')
                assert len(requests)==2, requests
                assert all(host=='auth.example.test' and proto=='https' for _,host,proto in requests),requests
            print(json.dumps({'case':'body_timeout' if body_hang else 'realm_discovery', 'status':response.status, 'elapsed_seconds':round(elapsed,2), 'private_paths':[x[0] for x in requests]}))
        finally:
            process.terminate()
            try: process.wait(timeout=10)
            except subprocess.TimeoutExpired: process.kill(); process.wait()
    content=logfile.read_text(encoding='utf-8', errors='replace')
    if body_hang: assert 'response_body' in content and 'backchannel failed' in content, 'Missing body timeout diagnostics'
    else: assert 'backchannel completed' in content, 'Missing completion diagnostics'
run_case(False)
run_case(True)
server.shutdown()



