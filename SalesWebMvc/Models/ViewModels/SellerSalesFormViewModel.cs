using System.Collections.Generic;

namespace SalesWebMvc.Models.ViewModels
{
    public class SellerSalesFormViewModel
    {
        public SalesRecord SalesRecord { get; set; }
        public int SellerId { get; set; }
        public Seller Seller { get; set; }
        public ICollection<Seller> Sellers { get; set; }
        public List<Item> Items { get; set; }
        public int[] ItemIds { get; set; }
        public int[] Quantity { get; set; }
    }
}
