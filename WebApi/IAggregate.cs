namespace WebApi;

public interface IAggregate
{
    void ApplyEvent(object @event);
}