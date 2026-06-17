using System.Collections.Generic;

namespace SalesWebMvc.Models.ViewModels
{
    public class SellerSalesViewModel
    {
        public Seller Seller { get; set; }
        public ICollection<SalesRecord> SalesRecords { get; set; }
        public SalesRecord SaleRecord { get; set; } = new SalesRecord();
    }
}
