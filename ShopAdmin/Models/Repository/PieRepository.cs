
using Microsoft.EntityFrameworkCore;

namespace ShopAdmin.Models.Repository
{
    public class PieRepository : IPieRepository
    {
        private readonly ShopDbContext shopDbContext;

        public PieRepository(ShopDbContext shopDbContext)
        {
            this.shopDbContext = shopDbContext;
        }

        public async Task<int> AddPieAsync(Pie pie)
        {
          shopDbContext.Pies.Add(pie);
            return await shopDbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Pie>> GetAllPiesAsync()
        {
            return await shopDbContext.Pies.OrderBy(c => c.PieId).ToListAsync(); 
        }

        public async Task<Pie?> GetPieByIdAsync(int id)
        {
            return await shopDbContext.Pies.Include(p => p.Ingredients).Include(p => p.Category).FirstOrDefaultAsync(p => p.PieId == id);
        }
        public async Task<int> UpdatePieAsync(Pie pie)
        {

            var pieToUpdate = await shopDbContext.Pies.FirstOrDefaultAsync(c => c.PieId == pie.PieId);
            if (pieToUpdate != null)
            {
                shopDbContext.Entry(pieToUpdate).Property("RowVersion").OriginalValue = pie.RowVersion;
                pieToUpdate.CategoryId = pie.CategoryId;
                pieToUpdate.ShortDescription = pie.ShortDescription;
                pieToUpdate.LongDescription = pie.LongDescription;
                pieToUpdate.Price = pie.Price;
                pieToUpdate.AllergyInformation = pie.AllergyInformation;
                pieToUpdate.ImageThumbnailUrl = pie.ImageThumbnailUrl;
                pieToUpdate.ImageUrl = pie.ImageUrl;
                pieToUpdate.InStock = pie.InStock;
                pieToUpdate.IsPieOfTheWeek = pie.IsPieOfTheWeek;
                pieToUpdate.Name = pie.Name;

                shopDbContext.Pies.Update(pieToUpdate);
                return await shopDbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"The pie to update can't be found.");
            }

        }
        public async Task<int> DeletePieAsync(int id)
        {
            var pieToDelete = await shopDbContext.Pies.FirstOrDefaultAsync(c => c.PieId == id);

            if (pieToDelete != null)
            {
                shopDbContext.Pies.Remove(pieToDelete);
                return await shopDbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"The pie to delete can't be found.");
            }
        }

        public async Task<int> GetAllPiesCountAsync()
        {
            var count = await shopDbContext.Pies.CountAsync();
            return count;

        }

        public async Task<IEnumerable<Pie>> GetPiesPagedAsync(int? pageNumber, int pageSize)
        {
            IQueryable<Pie> pies = from p in shopDbContext.Pies
                                   select p;
            pageNumber ??= 1;

            pies = pies.Skip((pageNumber.Value-1)*pageSize).Take(pageSize);
            return await pies.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Pie>> GetPiesSortedAndPagedAsync(string sortBy, int? pageNumber, int pageSize)
        {
            IQueryable<Pie> pies = from p in shopDbContext.Pies
                                   select p;
            switch (sortBy)
            {
                case "name_desc":
                    pies = pies.OrderByDescending(p => p.Name);
                    break;
                case "name":
                    pies = pies.OrderBy(p => p.Name);
                    break;
                case "id_desc":
                    pies = pies.OrderByDescending(p => p.PieId);
                    break;
                case "id":
                    pies = pies.OrderBy(p => p.PieId);
                    break;
                case "price_desc":
                    pies = pies.OrderByDescending(p => p.Price);
                    break;
                case "price":
                    pies = pies.OrderBy(p => p.Price);
                    break;
                default:
                    pies = pies.OrderBy(p => p.PieId);
                    break;
            }

            pageNumber ??= 1;

            pies = pies.Skip((pageNumber.Value - 1) * pageSize).Take(pageSize);

            return await pies.AsNoTracking().ToListAsync(); ;
        }
        public async Task<IEnumerable<Pie>> SearchPies(string searchQuery, int? categoryId)
        {
            var pies = from p in  shopDbContext.Pies
                       select p;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                pies = pies.Where(s => s.Name.Contains(searchQuery) || s.ShortDescription.Contains(searchQuery) || s.LongDescription.Contains(searchQuery));
            }

            if (categoryId != null)
            {
                pies = pies.Where(s => s.CategoryId == categoryId);
            }

            return await pies.ToListAsync();
        }




    }
}
