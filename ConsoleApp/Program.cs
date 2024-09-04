var account = new Account(Guid.NewGuid());
account.Deposit(100);
account.Withdraw(30);
account.Deposit(50);

Console.WriteLine($"Final balance: {account.Balance}"); // Output: Final balance: 120

//Records are a perfect fit for events, as they are immutable by design.
public record AccountOpened(Guid AccountId, DateTime OpenedAt);
public record MoneyDeposited(decimal Amount, DateTime DepositedAt);
public record MoneyWithdrawn(decimal Amount, DateTime WithdrawnAt);

public class Account
{
    public Guid Id { get; private set; }
    public decimal Balance { get; private set; }

    private List<object> _events = [];

    public Account(Guid id)
    {
        ApplyEvent(new AccountOpened(id, DateTime.Now));
    }

    public void Deposit(decimal amount)
    {
        ApplyEvent(new MoneyDeposited(amount, DateTime.Now));
    }

    public void Withdraw(decimal amount)
    {
        if (Balance >= amount)
        {
            ApplyEvent(new MoneyWithdrawn(amount, DateTime.Now));
        }
        else
        {
            throw new InvalidOperationException("Insufficient funds");
        }
    }

    private void ApplyEvent(object @event)
    {
        _events.Add(@event);

        switch (@event)
        {
            case AccountOpened e:
                Id = e.AccountId;
                Balance = 0;
                break;
            case MoneyDeposited e:
                Balance += e.Amount;
                break;
            case MoneyWithdrawn e:
                Balance -= e.Amount;
                break;
        }
    }
}