using System;
using System.Collections.Generic;
using EasyEventSourcing.Domain.Orders;
using EasyEventSourcing.EventSourcing.Domain;
using EasyEventSourcing.Messages.Orders;
using EasyEventSourcing.Messages.Store;
using System.Linq;

namespace EasyEventSourcing.Domain.Store
{
    public class ShoppingCart : Aggregate
    {
        public ShoppingCart() {}

        protected override void RegisterAppliers()
        {
            this.RegisterApplier<CartCreated>(this.Apply);
            this.RegisterApplier<ProductAddedToCart>(this.Apply);
            this.RegisterApplier<ProductRemovedFromCart>(this.Apply);
            this.RegisterApplier<CartEmptied>(this.Apply);
            this.RegisterApplier<CartCheckedOut>(this.Apply);
        }

        private readonly Dictionary<Guid, Decimal> products = new Dictionary<Guid, decimal>();
        private bool checkedOut;
        private Guid clientId;

        private ShoppingCart(Guid cartId, Guid customerId)
        {
            this.ApplyChanges(new CartCreated(cartId, customerId));
        }

        public static ShoppingCart Create(Guid cartId, Guid customerId)
        {
            return new ShoppingCart(cartId, customerId);
        }

        private void Apply(CartCreated evt)
        {
            this.Id = evt.CartId;
            this.clientId = evt.ClientId;
        }

        public void AddProduct(Guid productId, decimal price)
        {
            if(checkedOut)
            {
                throw new CartAlreadyCheckedOutException();
            }
            if (!products.ContainsKey(productId))
            {
                this.ApplyChanges(new ProductAddedToCart(this.Id, productId, price));
            }
        }

        private void Apply(ProductAddedToCart evt)
        {
            products.Add(evt.ProductId, evt.Price);
        }

        public void RemoveProduct(Guid productId)
        {
            if (checkedOut)
            {
                throw new CartAlreadyCheckedOutException();
            }
            if(products.ContainsKey(productId))
            {
                this.ApplyChanges(new ProductRemovedFromCart(this.Id, productId));
            }
        }

        private void Apply(ProductRemovedFromCart evt)
        {
            products.Remove(evt.ProductId);
        }

        public void Empty()
        {
            if (checkedOut)
            {
                throw new CartAlreadyCheckedOutException();
            }
            this.ApplyChanges(new CartEmptied(this.Id));
        }

        private void Apply(CartEmptied evt)
        {
            products.Clear();
        }

        public EventStream Checkout()
        {
            if(this.products.Count == 0)
            {
                throw new CannotCheckoutEmptyCartException();
            }
            this.ApplyChanges(new CartCheckedOut(this.Id));
            return Order.Create(this.Id, this.clientId, this.products.Select(x => new OrderItem(x.Key, x.Value)));
        }

        private void Apply(CartCheckedOut evt)
        {
            this.checkedOut = true;
        }
    }
}
