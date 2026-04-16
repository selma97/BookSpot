using BookSpot.Data;
using BookSpot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookSpot.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        public readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserOrderRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<IEnumerable<Order>> GetUserOrders()
        {
            var userId = GetUserId();
            if(string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not logged-in!");
            var orders = _context.Orders
                                 .Include(o => o.OrderStatus) 
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Book)
                                 .ThenInclude(x => x.Genre)
                                 .Where(o => o.UserId == userId)   
                                 .ToListAsync();
            return await orders;

        }
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
