using BookSpot.Models;
using BookSpot.Models.DTO;

namespace BookSpot.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> GetUserOrders(bool getAll = false);
        Task ChangeOrderStatus(UpdateOrderStatusModel data);
        Task TogglePaymentStatus(int orderId);
        Task<Order?> GetOrderById(int id);
        Task<IEnumerable<OrderStatus>> GetOrderStatuses();
    }
}