using System.Collections.Generic;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartStore : IShoppingCartStore
    {
        private static readonly Dictionary<int, ShoppingCart> Database = new Dictionary<int, ShoppingCart>();

        public ShoppingCart Get(int userId)
        {
            if (!Database.ContainsKey(userId))
                Database[userId] = new ShoppingCart(userId);
            return Database[userId];
        }

        public void Save(ShoppingCart shoppingCart)
        {
            // Nothing needed. Saving would be needed with a real DB
        }
    }
}