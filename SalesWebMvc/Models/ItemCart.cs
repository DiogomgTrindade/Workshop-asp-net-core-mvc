using System.Collections.Generic;

namespace SalesWebMvc.Models
{
    public class ItemCart
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int SalesRecordId { get; set; }

        public ItemCart()
        {
        }

        public ItemCart(int id, int itemId, Item item, int quantity)
        {
            Id = id;
            ItemId = itemId;
            Item = item;
            Quantity = quantity;
        }

        public double TotalValue()
        {
            return Item.Price * Quantity;
        }
    }
}
