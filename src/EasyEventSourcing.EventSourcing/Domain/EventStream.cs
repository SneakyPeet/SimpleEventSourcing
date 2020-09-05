using System.Collections.Generic;
using System;
using EasyEventSourcing.EventSourcing.Exceptions;
using EasyEventSourcing.Messages;

namespace EasyEventSourcing.EventSourcing.Domain
{
    public abstract class EventStream
    {
        private readonly List<IEvent> changes;
        protected Dictionary<Type, Action<IEvent>> eventAppliers;

        protected EventStream()
        {
            this.changes = new List<IEvent>();
            this.eventAppliers = new Dictionary<Type, Action<IEvent>>();
            this.RegisterAppliers();
        }

        protected abstract void RegisterAppliers();

        protected void RegisterApplier<TEvent>(Action<TEvent> applier) where TEvent : IEvent
        {
            this.eventAppliers.Add(typeof(TEvent), (x) => applier((TEvent)x));
        }

        protected Guid Id { get; set; }

        public string Name => this.GetType().Name;

        protected void ApplyChanges(IEvent evt)
        {
            this.Apply(evt);
            this.changes.Add(evt);
        }

        public StreamIdentifier StreamIdentifier => new StreamIdentifier(this.Name, this.Id);

        private void Apply(IEvent evt)
        {
            var evtType = evt.GetType();
            if (!this.eventAppliers.ContainsKey(evtType))
            {
                throw new NoEventApplyMethodRegisteredException(evt, this);
            }
            this.eventAppliers[evtType](evt);
        }

        public void LoadFromHistory(IEnumerable<IEvent> history)
        {
            foreach(var evt in history)
            {
                Apply(evt);
            }
        }

        public IEnumerable<IEvent> GetUncommitedChanges()
        {
            return this.changes.AsReadOnly();
        }

        public void Commit()
        {
            this.changes.Clear();
        }

        protected void NoStateChange<T>(T evt) where T : IEvent { }
    }
}