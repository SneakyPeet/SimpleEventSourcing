using System;
using System.Collections.Generic;
using System.Linq;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.Messages;

namespace EasyEventSourcing.EventSourcing.Persistence
{
    public class Repository : IRepository {
        private readonly HistoryProcessor processor;
        private readonly IEventStore eventStore;
        public Repository(IEventStore eventStore, HistoryProcessor processor) {
            this.eventStore = eventStore;
            this.processor = processor;
        }

        public T GetById<T>(Guid id) where T : EventStream
        {
            var streamId = new StreamIdentifier(typeof(T).Name, id);
            var history = this.eventStore.GetByStreamId(streamId);
            T streamItem = processor.Replay<T>(history);
            return streamItem;
        }

        public void Save(params EventStream[] streamItems)
        {
            var newEvents = new List<EventStoreStream>();
            foreach(var item in streamItems)
            {
                newEvents.Add(new EventStoreStream(item.StreamIdentifier, item.GetUncommitedChanges()));
            }

            this.eventStore.Save(newEvents);

            foreach (var item in streamItems)
            {
                item.Commit();
            }
        }
    }
}