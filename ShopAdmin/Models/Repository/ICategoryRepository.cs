namespace ShopAdmin.Models.Repository
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAllCategories();

        Task<IEnumerable<Category>> GetAllCategoriesAsync();
    }
}
