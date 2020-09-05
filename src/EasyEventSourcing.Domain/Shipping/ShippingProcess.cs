using System;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.EventSourcing.Handlers;
using EasyEventSourcing.Messages.Orders;
using EasyEventSourcing.Messages.Shipping;

namespace EasyEventSourcing.Domain.Shipping
{
    public class ShippingProcess : ProcessManager
    {
        protected override void RegisterAppliers()
        {
            RegisterApplier<StartedShippingProcess>(Apply);
            RegisterApplier<PaymentConfirmed>(Apply);
            RegisterApplier<AddressConfirmed>(Apply);
            RegisterApplier<OrderDelivered>(Apply);
        }

        private enum Status
        {
            Started,
            PaymentReceived,
            AddressReceived,
            ReadyToComplete,
            Complete
        }

        private Status status = Status.Started;

        public ShippingProcess() { }

        public static ShippingProcess Create(Guid orderId)
        {
            return new ShippingProcess(orderId);
        }

        private ShippingProcess(Guid orderId)
        {
            ApplyChanges(new StartedShippingProcess(orderId));
        }

        private void Apply(StartedShippingProcess evt)
        {
            Id = evt.OrderId;
        }

        public void ConfirmPayment(ICommandDispatcher dispatcher)
        {
            if (AwaitingPayment())
            {
                ApplyChanges(new PaymentConfirmed(Id));
                CompleteIfPossible(dispatcher);
            }
        }

        private bool AwaitingPayment()
        {
            return status == Status.Started || status == Status.AddressReceived;
        }

        private void Apply(PaymentConfirmed evt)
        {
            status = status == Status.AddressReceived ? Status.ReadyToComplete : Status.PaymentReceived;
        }

        public void ConfirmAddress(ICommandDispatcher dispatcher)
        {
            if (AwaitingAddress())
            {
                ApplyChanges(new AddressConfirmed(Id));
                CompleteIfPossible(dispatcher);
            }
        }

        private bool AwaitingAddress()
        {
            return status == Status.Started || status == Status.PaymentReceived;
        }

        private void Apply(AddressConfirmed evt)
        {
            status = status == Status.PaymentReceived ? Status.ReadyToComplete : Status.AddressReceived;
        }

        private void CompleteIfPossible(ICommandDispatcher dispatcher)
        {
            if(status == Status.ReadyToComplete)
            {
                ApplyChanges(new OrderDelivered(Id));
                dispatcher.Send(new CompleteOrder(Id)); //todo this is wierd should do it after events have been persisted
            }
        }

        private void Apply(OrderDelivered obj)
        {
            status = Status.Complete;
        }
    }
}
