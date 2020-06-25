using System.Security.Policy;
using EasyEventSourcing.EventSourcing;
using EasyEventSourcing.EventSourcing.Domain;
using NUnit.Framework.Internal;

namespace EasyEventSourcing.Tests.Core.Helpers
{
    class TestAggregate : Aggregate
    {
        public TestAggregate(TestInitializationEvent initEvent)
        {
            this.ApplyChanges(initEvent);
        }
        
        public static TestAggregate Create(int initialValue)
        {
            return new TestAggregate(new TestInitializationEvent(initialValue));
        }

        private void Apply(TestInitializationEvent evt) {
            this.Validation = false;
            this.StateUpdated = false;
            this.InitialState = evt.InitialValue;
        }
        
        protected override void RegisterAppliers()
        {
            this.RegisterApplier<TestInitializationEvent>(this.Apply);
            this.RegisterApplier<TestEvent>((e) => this.Apply(e));
        }

        public bool StateUpdated { get; internal set; }
        public bool Validation { get; internal set; }
        public int InitialState { get; internal set; }

        public void ExecuteTest()
        {
            this.Validation = true;
            this.ApplyChanges(new TestEvent());
        }

        private void Apply(TestEvent evt)
        {
            this.StateUpdated = true;
        }
    }
}
