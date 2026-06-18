using System.ComponentModel.DataAnnotations;

namespace SalesWebMvc.Models.Enums
{
    public enum ItemStatus
    {
        Stock,

        [Display(Name ="Out of Stock")]
        OutOfStock
    }
}
