using System.Collections.Generic;
using System.Linq;
using ShoppingCart.EventFeed;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCart
    {
        private readonly HashSet<ShoppingCartItem> items = new HashSet<ShoppingCartItem>();

        public int UserId { get; }
        
        public IEnumerable<ShoppingCartItem> Items => items;

        public ShoppingCart()
        {
        }

        public ShoppingCart(int userId)
        {
            this.UserId = userId;
        }

        public void AddItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
            {
                if (this.items.Add(item))
                    eventStore.Raise("ShoppingCartItemAdded", new { UserId, item });
            }
        }

        public void RemoveItems(int[] productCatalogueIds, IEventStore eventStore)
        {
            items.RemoveWhere(i => productCatalogueIds.Contains(i.ProductCatalogueId));
        }
    }
}
