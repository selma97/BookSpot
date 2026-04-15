using BookSpot.Data;
using BookSpot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using System.ComponentModel;

namespace BookSpot.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _context.Database.BeginTransaction();

            var cart = await GetCart(userId);
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in!");

                if (cart == null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                    };
                    _context.ShoppingCarts.Add(cart);
                }
                _context.SaveChanges();

                var cartDetail = await _context.CartDetails.FirstOrDefaultAsync(cd => cd.ShoppingCartId == cart.Id && cd.BookId == bookId);

                if (cartDetail != null)
                {
                    cartDetail.Quantity += qty;
                }
                else
                {
                    var book = _context.Books.Find(bookId);
                    cartDetail = new CartDetail
                    {
                        BookId = bookId,
                        Quantity = qty,
                        ShoppingCartId = cart.Id,
                        UnitPrice= book.Price
                    };
                    _context.CartDetails.Add(cartDetail);
                }
                _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }


        public async Task<int> RemoveItem(int bookId)
        {
            string userId = GetUserId();

            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in!");
                var cart = await GetCart(userId);

                if (cart == null)
                {
                    throw new InvalidOperationException("Invalid cart");
                }
           
                var cartDetail = await _context.CartDetails.FirstOrDefaultAsync(cd => cd.ShoppingCartId == cart.Id && cd.BookId == bookId);

                if (cartDetail == null)
                {
                    throw new InvalidOperationException("Not items in cart");
                }
                else if (cartDetail.Quantity == 1)
                {
                    _context.CartDetails.Remove(cartDetail);
                    _context.SaveChanges();
                }
                else
                {
                    cartDetail.Quantity = cartDetail.Quantity - 1;
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }

            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == userId);
            return cart;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            var data = await (from cart in _context.ShoppingCarts
                              join cartDetail in _context.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId == userId
                              select new { cartDetail.Id }
                        ).ToListAsync();

            return data.Count;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new InvalidOperationException("Invalid userid");
            var shoppingCart = await _context.ShoppingCarts
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Book)
                                  .ThenInclude(a => a.Genre)
                                  .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;

        }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new InvalidOperationException("User is not logged in!");
                var cart = await GetCart(userId);
                if(cart==null)
                {
                    throw new InvalidOperationException("Invalid cart");
                }
                var cartDetails = await _context.CartDetails.Where(cd => cd.ShoppingCartId == cart.Id).ToListAsync();
                if(cartDetails.Count==0)
                {
                  throw new InvalidOperationException("No items in cart");
                }

                var order= new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId=1
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
                foreach(var item in cartDetails) 
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        OrderId = order.Id,
                        UnitPrice =item.UnitPrice
                    };
                    _context.OrderDetails.Add(orderDetail);
                }
                _context.SaveChanges();
                _context.CartDetails.RemoveRange(cartDetails);
                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}