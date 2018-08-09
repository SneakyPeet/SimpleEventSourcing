namespace EasyEventSourcing.EventSourcing.Persistence 
{
    public interface IEventStreamInitializer<in TEvent, out TItem>
    {
        TItem Create(TEvent evt);    
    }
}