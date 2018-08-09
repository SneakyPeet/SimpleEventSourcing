using System;
using System.Collections.Generic;
using System.Linq;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.Messages;

namespace EasyEventSourcing.EventSourcing.Persistence {
    public class EventStreamCreator 
    {
        private readonly Dictionary<Type, Func<IEvent, object>> initializers = new Dictionary<Type, Func<IEvent, object>>();

        public T CreateNew<T>(IEnumerable<IEvent> history) where T : EventStream, new()
        {
            var first = history.First();
            T streamItem;
            if (initializers.ContainsKey(first.GetType()))
            {
                streamItem = (T) initializers[first.GetType()](first);
                streamItem.LoadFromHistory(history.Skip(1));
            }
            else {
                streamItem = new T();
                streamItem.LoadFromHistory(history);
            }

            return streamItem;
        }

        public void RegisterInitializer<TEvent, TItem>(IEventStreamInitializer<TEvent, TItem> initializer) where TEvent : IEvent
            where TItem : EventStream 
        {
            initializers.Add(typeof(TEvent), (e) => initializer.Create((TEvent)e));
        }
    }
}