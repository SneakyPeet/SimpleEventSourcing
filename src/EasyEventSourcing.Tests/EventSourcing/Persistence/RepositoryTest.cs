using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.EventSourcing.Persistence;
using EasyEventSourcing.Messages;
using EasyEventSourcing.Tests.Core.Helpers;
using EasyEventSourcing.Tests.Domain.Helpers;
using NUnit.Framework;

namespace EasyEventSourcing.Tests.EventSourcing.Persistence
{
    [TestFixture]
    public class RepositoryTest 
    {
        [Test]
        public void LoadFromHistory() {
            IEventStore store = new TestEventStore();
            Guid id = Guid.NewGuid();
            store.Save(new List<EventStoreStream> 
            {
                new EventStoreStream(new StreamIdentifier(typeof(TestAggregate).Name, id), new []{ new TestEvent()})
            });
            IRepository repo = new Repository(store, new EventStreamCreator());
            
            TestAggregate aggregate = repo.GetById<TestAggregate>(id);
            Assert.That(aggregate.StateUpdated, Is.True);
        }
        
        [Test]
        public void LoadFromHistoryWithInitializerEvent() 
        {
            IEventStore store = new TestEventStore();
            Guid id = Guid.NewGuid();
            const int anIntValue = 19;
            store.Save(new List<EventStoreStream> 
            {
                new EventStoreStream(new StreamIdentifier(typeof(TestAggregate).Name, id), new IEvent []{ new TestInitializationEvent(anIntValue), new TestEvent() })
            });
            EventStreamCreator creator = new EventStreamCreator();
            creator.RegisterInitializer(new TestAggregateInitializer());
            IRepository repo = new Repository(store, creator);
            
            TestAggregate aggregate = repo.GetById<TestAggregate>(id);
            Assert.That(aggregate.InitialState, Is.EqualTo(anIntValue));
            Assert.That(aggregate.StateUpdated, Is.True);
        }
    }

    class TestAggregateInitializer : IEventStreamInitializer<TestInitializationEvent, TestAggregate> {
        public TestAggregate Create(TestInitializationEvent evt) {
            return new TestAggregate(evt.InitialValue);
        }
    }
}