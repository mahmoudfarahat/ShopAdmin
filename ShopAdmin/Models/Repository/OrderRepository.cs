
using Microsoft.EntityFrameworkCore;

namespace ShopAdmin.Models.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ShopDbContext shopDbContext;

        public OrderRepository(ShopDbContext shopDbContext)
        {
            this.shopDbContext = shopDbContext;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync()
        {
            return await shopDbContext.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Pie).OrderBy(o => o.OrderId).ToListAsync();
        }

        public async Task<Order?> GetOrderDetailAsync(int? orderId)
        {
            if(orderId != null)
            {
                var order = await shopDbContext.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Pie)
                    .OrderBy(o => o.OrderId).Where(o => o.OrderId == orderId.Value).FirstOrDefaultAsync();

                return order;
            }
            return null;
        }
    }
}
