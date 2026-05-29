using DataSeeder;
using SimpleIdServer.IdServer.Builders;
using SimpleIdServer.IdServer.Stores;
using System.Threading;
using System.Threading.Tasks;

namespace MinGo.IdServer.Seed;

public class AdminUserDataSeeder : BaseAfterDeploymentDataSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionBuilder _transactionBuilder;

    public AdminUserDataSeeder(
        IUserRepository userRepository,
        ITransactionBuilder transactionBuilder,
        IDataSeederExecutionHistoryRepository dataSeederExecutionHistoryRepository)
        : base(dataSeederExecutionHistoryRepository)
    {
        _userRepository = userRepository;
        _transactionBuilder = transactionBuilder;
    }

    public override string Name => nameof(AdminUserDataSeeder);

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
