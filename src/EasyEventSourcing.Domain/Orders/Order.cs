using System;
using System.Collections.Generic;
using System.Linq;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.Messages.Orders;
using EasyEventSourcing.Messages.Shipping;

namespace EasyEventSourcing.Domain.Orders
{
    public class Order : Aggregate
    {
        protected override void RegisterAppliers()
        {
            this.RegisterApplier<OrderCreated>(this.Apply);
            this.RegisterApplier<PaymentReceived>(this.Apply);
            this.RegisterApplier<ShippingAddressConfirmed>(this.Apply);
            this.RegisterApplier<OrderCompleted>(this.Apply);
        }

        private bool paidFor;
        private bool shippingAddressProvided;
        private bool completed;

        public static Order Create(Guid orderId, Guid clientId, IEnumerable<OrderItem> items)
        {
            return new Order(orderId, clientId, items);
        }

        private Order(Guid orderId, Guid clientId, IEnumerable<OrderItem> items)
        {
            this.ApplyChanges(new OrderCreated(orderId, clientId, items.ToArray()));
        }

        public Order()
        {
        }

        private void Apply(OrderCreated evt)
        {
            this.Id = evt.OrderId;
        }

        public void ProvideShippingAddress(string address)
        {
            if(!this.shippingAddressProvided && !this.completed)
            {
                this.ApplyChanges(new ShippingAddressConfirmed(this.Id, address));
            }
        }

        private void Apply(ShippingAddressConfirmed evt)
        {
            this.shippingAddressProvided = true;
        }

        public void Pay()
        {
            if (!this.paidFor && !this.completed)
            {
                this.ApplyChanges(new PaymentReceived(this.Id));
            }
        }

        public void Apply(PaymentReceived evt)
        {
            this.paidFor = true;
        }

        public void CompleteOrder()
        {
            if(!this.paidFor || !this.shippingAddressProvided)
            {
                throw new CannotCompleteOrderException();
            }
            this.ApplyChanges(new OrderCompleted(this.Id));
        }

        private void Apply(OrderCompleted evt)
        {
            this.completed = true;
        }
    }
}