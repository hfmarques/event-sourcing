namespace WebApi;

public class Account : IAggregate
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; } = 0;
    public List<object> Events { get; set; } = [];
    public void ApplyEvent(object @event)
    {
        Events.Add(@event);
        
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