using System.Text;
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

    public async Task<Account> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        // var events = Enumerable.Range(0, 20)
        //     .Select(
        //         r => new EventData(
        //             Uuid.NewUuid(),
        //             "some-event",
        //             Encoding.UTF8.GetBytes($"{{\"id\": \"{r}\" \"value\": \"some value\"}}")
        //         )
        //     );
        //
        // await client.AppendToStreamAsync(
        //     "some-stream",
        //     StreamState.Any,
        //     events
        // );
        //
        // var events = client.ReadStreamAsync(
        //     Direction.Forwards,
        //     "some-stream",
        //     StreamPosition.Start
        // );
        //
        // await foreach (var @event in events) Console.WriteLine(Encoding.UTF8.GetString(@event.Event.Data.ToArray()));
        //
        // Console.WriteLine(events.FirstStreamPosition);
        // Console.WriteLine(events.LastStreamPosition);
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