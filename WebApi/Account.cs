namespace WebApi;

public class Account
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; } = 0;
    public List<object> Events = [];
}