using Microsoft.AspNetCore.Mvc.Rendering;
using ShopAdmin.Models;

namespace ShopAdmin.ViewModels
{
    public class PieSearchViewModel
    {
        public IEnumerable<Pie>? Pies { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; } = default!;
        public string? SearchQuery { get; set; }
        public int? SearchCategory
        {
            get; set;
        }
    }
}
