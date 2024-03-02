namespace ShopAdmin.Models.Repository
{
    public interface IOrderRepository
    {
        Task<Order?> GetOrderDetailAsync(int? orderId);

        Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync();
    }
}
