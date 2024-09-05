namespace WebApi;

public class AccountService(EventStoreRepository repository)
{
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

    public async Task<Account?> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        return await repository.LoadAsync<Account>(GetStreamName(id), cancellationToken);
    }

    // public void Deposit(decimal amount)
    // {
    //     ApplyEvent(new MoneyDeposited(amount, DateTime.Now));
    // }
    //
    // public void Withdraw(decimal amount)
    // {
    //     if (Balance >= amount)
    //     {
    //         ApplyEvent(new MoneyWithdrawn(amount, DateTime.Now));
    //     }
    //     else
    //     {
    //         throw new InvalidOperationException("Insufficient funds");
    //     }
    // }
    
    private string GetStreamName(Guid? id = null)
    {
        var aggregateName = nameof(Account).ToLowerInvariant();

        return id is not null ? $"{aggregateName}_{id}" : aggregateName;
    }
}