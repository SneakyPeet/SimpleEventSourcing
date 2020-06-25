using System;
using System.Collections.Generic;
using System.Linq;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.Messages;

namespace EasyEventSourcing.EventSourcing.Persistence {
    public class HistoryProcessor 
    {
        public T RebuildAggregate<T>(IEnumerable<IEvent> history) where T : EventStream
        {
            var first = history.First();
            T streamItem;
            var constructorByEvent = typeof(T).GetConstructor(new [] { first.GetType() });
            if (constructorByEvent != null)
            {
                streamItem = (T) constructorByEvent.Invoke(new object[] {first});
                streamItem.Commit();
                streamItem.LoadFromHistory(history.Skip(1));
            }
            else
            {
                throw new Exception($"Unable to find {typeof(T).Name} constructor that takes event type {first.GetType().Name}");
            }
            return streamItem;
        }
    }
}