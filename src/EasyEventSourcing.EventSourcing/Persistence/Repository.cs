using System;
using System.Collections.Generic;
using System.Linq;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.Messages;

namespace EasyEventSourcing.EventSourcing.Persistence
{
    public class Repository : IRepository {
        private readonly EventStreamCreator creator;
        private readonly IEventStore eventStore;
        public Repository(IEventStore eventStore, EventStreamCreator creator) {
            this.eventStore = eventStore;
            this.creator = creator;
        }

        public T GetById<T>(Guid id) where T : EventStream, new()
        {
            var streamId = new StreamIdentifier(typeof(T).Name, id);
            var history = this.eventStore.GetByStreamId(streamId);
            T streamItem = creator.CreateNew<T>(history);
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