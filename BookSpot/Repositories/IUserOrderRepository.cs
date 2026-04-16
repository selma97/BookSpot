using BookSpot.Models;

namespace BookSpot.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> GetUserOrders();
    }
}