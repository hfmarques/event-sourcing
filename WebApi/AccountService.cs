namespace WebApi;

public class AccountService(EventStoreRepository repository)
{
    public async Task<List<Account>> GetAll(CancellationToken cancellationToken)
    {
        return await repository.LoadListAsync<Account>(cancellationToken);
    }
    
    public async Task<Account?> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        return await repository.LoadAsync<Account>(GetStreamName(id), cancellationToken);
    }
    
    public async Task<Account> OpenAccount(Guid id, CancellationToken cancellationToken)
    {
        var @event = new AccountOpened(id, DateTime.Now);
        
        var account = new Account
        {
            Id = id,
            Events = [@event]
        };
        
        await repository.AppendEvents(GetStreamName(account.Id), @event, cancellationToken);
        
        return account;
    }

    public async Task Deposit(Guid id, decimal amount, CancellationToken cancellationToken)
    {
        var account = await GetAccount(id, cancellationToken);
        
        ArgumentNullException.ThrowIfNull(account);
        
        var @event = new MoneyDeposited(amount, DateTime.Now);
        await repository.AppendEvents(GetStreamName(id), @event, cancellationToken);
    }
    
    public async Task Withdraw(Guid id, decimal amount, CancellationToken cancellationToken)
    {
        var account = await GetAccount(id, cancellationToken);
        
        ArgumentNullException.ThrowIfNull(account);

        if (account.Balance >= amount)
        {
            var @event = new MoneyWithdrawn(amount, DateTime.Now);
        
            await repository.AppendEvents(GetStreamName(id), @event, cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("Insufficient funds");
        }
    }
    
    private string GetStreamName(Guid? id = null)
    {
        var aggregateName = nameof(Account).ToLowerInvariant();

        return id is not null ? $"{aggregateName}_{id}" : aggregateName;
    }
}