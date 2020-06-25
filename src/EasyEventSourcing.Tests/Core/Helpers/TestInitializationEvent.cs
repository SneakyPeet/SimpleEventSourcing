using EasyEventSourcing.Messages;

namespace EasyEventSourcing.Tests.Core.Helpers 
{
    public class TestInitializationEvent: IEvent 
    {
        public TestInitializationEvent(int aValue)
        {
            InitialValue = aValue;
        }

        public int InitialValue { get; private set; }
    }
}