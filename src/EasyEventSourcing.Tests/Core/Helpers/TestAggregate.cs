using EasyEventSourcing.EventSourcing;
using EasyEventSourcing.EventSourcing.Domain;

namespace EasyEventSourcing.Tests.Core.Helpers
{
    class TestAggregate : Aggregate
    {
        public TestAggregate()
        {
            this.Validation = false;
            this.StateUpdated = false;
        }

        protected override void RegisterAppliers()
        {
            this.RegisterApplier<TestEvent>((e) => this.Apply(e));
        }

        public bool StateUpdated { get; internal set; }
        public bool Validation { get; internal set; }

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
