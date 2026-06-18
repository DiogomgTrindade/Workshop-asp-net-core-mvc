using SalesWebMvc.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Schema;

namespace SalesWebMvc.Models
{
    public class SalesRecord
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Date { get; set; }

        [Required]
        [Range(1.0, 999999.0, ErrorMessage = "{0} must be from {1} to {2}")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Amount { get; set; }

        public SaleStatus Status { get; set; }
        public Seller Seller { get; set; }
        public List<ItemCart> Items { get; set; } = new List<ItemCart>();

        public SalesRecord()
        {
        }

        public SalesRecord(int id, DateTime date, SaleStatus status, Seller seller)
        {
            Id = id;
            Date = date;
            Amount = TotalAmount();
            Status = status;
            Seller = seller;
        }

        public double TotalAmount ()
        {
            double total = 0;
            foreach (var item in Items)
            {
                total += item.TotalValue();
            }

            return total;
        }
    }
}
