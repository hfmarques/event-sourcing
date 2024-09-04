using System.Text.Json;
using EventStore.Client;

namespace WebApi;

public class AccountService(EventStoreClient client)
{
    public async Task<Account> OpenAccount(Guid id, CancellationToken cancellationToken)
    {
        var @event = new AccountOpened(id, DateTime.Now);
        
        var account = new Account
        {
            Id = id,
            Events = [@event]
        };
        
        var eventData = new EventData(
            Uuid.NewUuid(),
            "AccountOpened",
            JsonSerializer.SerializeToUtf8Bytes(@event)
        );
        
        await client.AppendToStreamAsync(
            "AccountOpened",
            StreamState.Any,
            [eventData],
            cancellationToken: cancellationToken
        );
        
        return account;
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

    // private void ApplyEvent(object @event)
    // {
    //     _events.Add(@event);
    //
    //     switch (@event)
    //     {
    //         case AccountOpened e:
    //             Id = e.AccountId;
    //             Balance = 0;
    //             break;
    //         case MoneyDeposited e:
    //             Balance += e.Amount;
    //             break;
    //         case MoneyWithdrawn e:
    //             Balance -= e.Amount;
    //             break;
    //     }
    // }
}