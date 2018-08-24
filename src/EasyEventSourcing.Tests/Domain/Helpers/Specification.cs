using System;
using System.Collections.Generic;
using EasyEventSourcing.Application;
using EasyEventSourcing.EventSourcing;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.EventSourcing.Handlers;
using EasyEventSourcing.EventSourcing.Persistence;
using EasyEventSourcing.Messages;
using NUnit.Framework;
using SharpTestsEx;

namespace EasyEventSourcing.Tests.Domain.Helpers
{
    class Specification
    {
        protected readonly ICommandDispatcher app;
        protected readonly TestEventStore eventStore;

        protected Specification()
        {
            this.eventStore = new TestEventStore();
            var commandHandlerFactory = new CommandHandlerFactory(this.eventStore);
            this.app = new CommandDispatcher(commandHandlerFactory);
        }

        protected void And<TEventStream, TEvent>(Guid id, TEvent evt)
            where TEvent : IEvent
            where TEventStream : EventStream
        {
            this.Given<TEventStream, TEvent>(id, evt);
        }

        protected void Given<TEventStream, TEvent>(Guid id, TEvent evt) 
                                                    where TEvent : IEvent
                                                    where TEventStream : EventStream
        {
            var streamIdentifier = new StreamIdentifier(typeof(TEventStream).Name, id);
            var eventStoreStream = new EventStoreStream(streamIdentifier, new List<IEvent>{evt});
            this.eventStore.Save(new List<EventStoreStream>{eventStoreStream});
            this.eventStore.CommitInitialEvents();
        }

        protected void When<T>(T command) where T : ICommand
        {
            this.app.Send<T>(command);
        }

        protected void Then(params IEvent[] expectedEvents)
        {
            this.eventStore.NewEvents().Should()
                      .Not.Be.Empty()
                      .And.Have.SameSequenceAs(expectedEvents)
                      .And.Have.UniqueValues();
        }

        protected void ThenNoEvents()
        {
            this.eventStore.NewEvents().Should()
                .Be.Empty();
        }

        protected void ThrowsWhen<TException,T>(T command) where TException : Exception where T :ICommand
        {
            Assert.Throws<TException>(
                () => this.When(command)
            );
        }

        [TearDown]
        public void TearDown()
        {
            this.eventStore.Complete();
        }
    }
}
