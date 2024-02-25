

using Microsoft.EntityFrameworkCore;

namespace ShopAdmin.Models.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ShopDbContext _shopDbContext;

        public CategoryRepository(ShopDbContext shopDbContext)
        {
            _shopDbContext = shopDbContext;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _shopDbContext.Categories.OrderBy(p => p.CategoryId);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _shopDbContext.Categories.OrderBy(c => c.CategoryId).ToListAsync();
        }
    }
}
