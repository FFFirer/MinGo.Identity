// Copyright (c) MinGo. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using DataSeeder;
using SimpleIdServer.IdServer.Builders;
using SimpleIdServer.IdServer.Stores;
using System.Threading;
using System.Threading.Tasks;

namespace MinGo.IdServer.Conf.Migrations.AfterDeployment;

/// <summary>
/// 创建默认管理员用户（administrator / password）
/// 仅在首次启动、用户不存在时执行
/// </summary>
public class ConfigureAdminUserDataSeeder : BaseAfterDeploymentDataSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionBuilder _transactionBuilder;

    public ConfigureAdminUserDataSeeder(
        IUserRepository userRepository,
        ITransactionBuilder transactionBuilder,
        IDataSeederExecutionHistoryRepository dataSeederExecutionHistoryRepository)
        : base(dataSeederExecutionHistoryRepository)
    {
        _userRepository = userRepository;
        _transactionBuilder = transactionBuilder;
    }

    public override string Name => nameof(ConfigureAdminUserDataSeeder);

    protected override async Task Execute(CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetBySubject("administrator", SimpleIdServer.IdServer.Constants.DefaultRealm, cancellationToken);
        if (existingUser != null) return;

        var user = UserBuilder.Create("administrator", "password", "Administrator")
            .SetFirstname("Administrator")
            .SetEmail("admin@mingo.local")
            .Build();

        _userRepository.Add(user);
        using var transaction = _transactionBuilder.Build();
        await transaction.Commit(cancellationToken);
    }
}
