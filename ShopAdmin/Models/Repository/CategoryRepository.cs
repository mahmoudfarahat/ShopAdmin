

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ShopAdmin.Models.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ShopDbContext _shopDbContext;
        private readonly IMemoryCache memoryCache;

        private const string AllCategoriesCacheName = "AllCategories";
        public CategoryRepository(ShopDbContext shopDbContext, IMemoryCache memoryCache )
        {
            _shopDbContext = shopDbContext;
            this.memoryCache = memoryCache;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _shopDbContext.Categories.OrderBy(p => p.CategoryId);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            //throw new Exception("Database Down");
            List<Category> allCategories = null;
            if(!memoryCache.TryGetValue(AllCategoriesCacheName, out allCategories))
            {
                allCategories =await _shopDbContext.Categories.AsNoTracking().OrderBy(c => c.CategoryId).ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(60));

                memoryCache.Set(AllCategoriesCacheName,allCategories,cacheEntryOptions);
            }
            return allCategories;
            //return await _shopDbContext.Categories.AsNoTracking().OrderBy(c => c.CategoryId).ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
             return await  _shopDbContext.Categories.Include(p => p.Pies).AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId ==id);
        }

        public async Task<int> AddCategoryAsync(Category category)
        {
            bool categoryWithSameNameExist = await _shopDbContext.Categories.AnyAsync(c => c.Name == category.Name);

            if (categoryWithSameNameExist)
            {
                throw new Exception("A category with the same name already exists");
            }

            _shopDbContext.Categories.Add(category);

            int result = await _shopDbContext.SaveChangesAsync();
            memoryCache.Remove(AllCategoriesCacheName);

            return result;
        }

        public async Task<int> UpdateCategoryAsync(Category category)
        {
            bool categoryWithSameNameExist = await _shopDbContext.Categories.AnyAsync(c => c.Name == category.Name && c.CategoryId != category.CategoryId);

            if (categoryWithSameNameExist)
            {
                throw new Exception("A category with the same name already exists");
            }

            var categoryToUpdate = await _shopDbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

            if (categoryToUpdate != null)
            {

                categoryToUpdate.Name = category.Name;
                categoryToUpdate.Description = category.Description;

                _shopDbContext.Categories.Update(categoryToUpdate);
                return await _shopDbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"The category to update can't be found.");
            }
        }

        public async Task<int> DeleteCategoryAsync(int id)
        {
            var categoryToDelete = await _shopDbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);

            var piesInCategory = _shopDbContext.Pies.Any(p => p.CategoryId == id);

            if(piesInCategory)
            {
                throw new Exception("Pies exist in the this category. Delete all pies in this category before deleting the category.");
            }

            if (categoryToDelete != null)
            {
                _shopDbContext.Categories.Remove(categoryToDelete);
                return await _shopDbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"The category to delete can't be found.");
            }
        }
        public async Task<int> UpdateCategoryNamesAsync(List<Category> categories)
        {
            foreach (var category in categories)
            {
                var categoryToUpdate = await _shopDbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

                if (categoryToUpdate != null)
                {
                    categoryToUpdate.Name = category.Name;

                    _shopDbContext.Categories.Update(categoryToUpdate);
                }
            }

            int result = await _shopDbContext.SaveChangesAsync();

             memoryCache.Remove(AllCategoriesCacheName);

            return result;
        }
    }
}
