using Microsoft.AspNetCore.Mvc.Rendering;
using ShopAdmin.Models;

namespace ShopAdmin.ViewModels
{
    public class PieAddViewModel
    {
        public IEnumerable<SelectListItem>? Categories { get; set; } = default!; 
        public Pie? Pie {  get; set; } 
    }
}
