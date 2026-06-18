using SalesWebMvc.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SalesWebMvc.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Display(Name= "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "{0} required")]
        public string Name { get; set; }

        [Required]
        [Range(1.0, 999999.0, ErrorMessage = "{0} must be from {1} to {2}")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Price { get; set; }

        public ItemStatus Status { get; set; } = ItemStatus.Stock;

        public Item()
        {
        }

        public Item(int id, int departmentId, string name, double price, ItemStatus status)
        {
            Id = id;
            DepartmentId = departmentId;
            Name = name;
            Price = price;
            Status = status;
        }
    }
}
