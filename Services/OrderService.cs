using Microsoft.EntityFrameworkCore;
using System.Linq;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Models.Enums;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IOrderEmailService _orderEmailService;
        private readonly INotificationService _notificationService;

        public OrderService(
            ApplicationDbContext context, 
            ICartService cartService,
            IOrderEmailService orderEmailService,
            INotificationService notificationService)
        {
            _context = context;
            _cartService = cartService;
            _orderEmailService = orderEmailService;
            _notificationService = notificationService;
        }

        // GET: Order/OrderDetail/{id}
        public async Task<Order?> GetOrderByIdAsync(int orderId, string userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
        }

        // GET: Order/MyOrders
        // GET: Order/MyOrders
        public async Task<List<Order>> GetUserOrdersAsync(string userId, int pageIndex = 1, int pageSize = 10)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUserOrdersCountAsync(string userId)
        {
            return await _context.Orders.Where(o => o.UserId == userId).CountAsync();
        }

        // GET: Order/Detail (Admin)
        public async Task<Order?> GetOrderForAdminAsync(int orderId)
        {
             return await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Include(o => o.StatusHistories.OrderByDescending(h => h.CreatedAt))
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Include(o => o.StatusHistories.OrderByDescending(h => h.CreatedAt))
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        // GET: Order/Index (Admin)
        public async Task<List<Order>> GetAllOrdersAsync(string status, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All" && Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                 query = query.Where(o => o.OrderStatus == orderStatus);
            }

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetAllOrdersCountAsync(string status)
        {
             var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All" && Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                 query = query.Where(o => o.OrderStatus == orderStatus);
            }
            return await query.CountAsync();
        }

        public async Task<OrderStatsViewModel> GetOrderStatsAsync()
        {
             var stats = new OrderStatsViewModel
            {
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Pending),
                ConfirmedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Confirmed),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Processing),
                ShippedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Shipped),
                DeliveredOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Delivered),
                CancelledOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Cancelled)
            };
            return stats;
        }

        // POST: Order/UpdateStatus (Admin)
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, string? trackingNumber = null, string? courierService = null, string? notes = null, string? changedBy = null)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Item).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return false;

            // Basic validation: Cannot move from terminal states
            if (order.OrderStatus == OrderStatus.Cancelled || order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.Returned) 
                return false;

            // Enforce valid transitions (Forward only, unless Cancelling)
            if (status != OrderStatus.Cancelled && status != OrderStatus.Returned && (int)status < (int)order.OrderStatus)
            {
                return false;
            }

            // Logic for Cancelled status
            if (status == OrderStatus.Cancelled)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    if (orderItem.Item != null)
                    {
                        orderItem.Item.Quantity += orderItem.Quantity;
                    }
                }
            }

            order.OrderStatus = status;
            order.StatusUpdatedAt = DateTime.UtcNow;
            order.ModifiedDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(trackingNumber)) order.TrackingNumber = trackingNumber;
            if (!string.IsNullOrEmpty(courierService)) order.CourierService = courierService;

            // Add to History
            var history = new OrderStatusHistory
            {
                OrderId = orderId,
                Status = status,
                Notes = notes ?? $"Status updated to {status}",
                ChangedBy = changedBy ?? "System",
                CreatedAt = DateTime.UtcNow
            };
            _context.StatusHistories.Add(history);

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // Trigger Notification
            await _notificationService.CreateNotificationAsync(
                "Order Status Updated",
                $"Order #{order.OrderNumber} status changed to {status}",
                NotificationType.OrderStatusUpdate,
                order.OrderNumber,
                changedBy);

            return true;
        }

        public List<OrderTimelineStep> GetOrderTimeline(Order order)
        {
            var steps = new List<OrderTimelineStep>
            {
                new OrderTimelineStep { Status = OrderStatus.Pending, Label = "Order Placed" },
                new OrderTimelineStep { Status = OrderStatus.Confirmed, Label = "Confirmed" },
                new OrderTimelineStep { Status = OrderStatus.Processing, Label = "Processing" },
                new OrderTimelineStep { Status = OrderStatus.Shipped, Label = "Shipped" },
                new OrderTimelineStep { Status = OrderStatus.Delivered, Label = "Delivered" }
            };

            var histories = order.StatusHistories.OrderBy(h => h.CreatedAt).ToList();

            // Map timestamps based on history
            foreach (var step in steps)
            {
                var history = histories.LastOrDefault(h => h.Status == step.Status); // Use LastOrDefault to get latest occurrence
                if (history != null)
                {
                    step.IsCompleted = true;
                    step.Timestamp = history.CreatedAt;
                }
            }

            // Ensure "Order Placed" always has a timestamp
            if (steps[0].Timestamp == null)
            {
                steps[0].IsCompleted = true;
                steps[0].Timestamp = order.OrderDate;
            }

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                // If cancelled, just mark the specific Cancelled step if we explicitly want to show it,
                // or just leave the timeline as "frozen" at the last state.
                // The requirement is "Timeline stops and visually reflects cancellation state".
                // Adding an explicit Cancelled step helps visualize it.
                steps.Add(new OrderTimelineStep 
                { 
                    Status = OrderStatus.Cancelled, 
                    Label = "Cancelled", 
                    IsCurrent = true, 
                    IsCompleted = true,
                    Timestamp = histories.LastOrDefault(h => h.Status == OrderStatus.Cancelled)?.CreatedAt ?? order.StatusUpdatedAt,
                    Description = "Order has been cancelled"
                });
            }
            else if (order.OrderStatus == OrderStatus.Returned)
            {
                steps.Add(new OrderTimelineStep
                {
                    Status = OrderStatus.Returned,
                    Label = "Returned",
                    IsCurrent = true,
                    IsCompleted = true,
                    Timestamp = histories.LastOrDefault(h => h.Status == OrderStatus.Returned)?.CreatedAt ?? order.StatusUpdatedAt
                });
            }
            else
            {
                // Happy path: Fill gaps
                int currentLevel = (int)order.OrderStatus;

                foreach (var step in steps)
                {
                    if ((int)step.Status <= currentLevel)
                    {
                        step.IsCompleted = true;
                    }
                    if (step.Status == order.OrderStatus)
                    {
                        step.IsCurrent = true;
                    }
                }
            }

            return steps;
        }

        // POST: Order/Cancel (Admin)
        public async Task<bool> CancelOrderAsync(int orderId, string adminNotes)
        {
             return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled, notes: adminNotes, changedBy: "Admin");
        }


        // POST: Order/PlaceOrder
        public async Task<Order?> PlaceOrderAsync(OrderPlacementViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Load Cart Items
                var cartItems = await _context.CartItems
                    .Include(c => c.Item)
                    .ThenInclude(i => i.GoldKarat)
                    .Include(c => c.Item)
                    .ThenInclude(i => i.Brand)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (!cartItems.Any()) return null;

                // Validate Stock
                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Item.Quantity < cartItem.Quantity)
                    {
                        throw new Exception($"Insufficient stock for {cartItem.Item.ItemName}. Available: {cartItem.Item.Quantity}");
                    }
                }

                // Validate Payment (Server-side)
                if (model.PaymentMethod == "Credit Card")
                {
                    if (string.IsNullOrWhiteSpace(model.CardNumber) || !ValidateLuhn(model.CardNumber))
                    {
                        throw new Exception("Invalid Credit Card Number.");
                    }
                    if (string.IsNullOrWhiteSpace(model.ExpiryDate) || !ValidateExpiry(model.ExpiryDate))
                    {
                        throw new Exception("Invalid or Expired Card.");
                    }
                    if (string.IsNullOrWhiteSpace(model.CVV) || model.CVV.Length < 3)
                    {
                        throw new Exception("Invalid CVV.");
                    }
                    if (string.IsNullOrWhiteSpace(model.CardHolderName))
                    {
                        throw new Exception("Card Holder Name is required.");
                    }
                }

                // Create Order Header
                var order = new Order
                {
                    OrderNumber = Order.GenerateOrderNumber(),
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    CustomerName = $"{model.FirstName} {model.LastName}",
                    Email = model.EmailOrPhone,
                    PhoneNumber = model.Phone,
                    ShippingAddress = model.Address,
                    ShippingCity = model.City,
                    ShippingState = model.State ?? string.Empty,
                    ShippingPostalCode = model.ZipCode,
                    ShippingCountry = model.Country,
                    BillingAddressSameAsShipping = model.UseShippingAsBilling,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = model.PaymentMethod == "Cash on Delivery" ? "Pending" : "Completed", // Mocking immediate success for electronic
                    OrderStatus = OrderStatus.Pending,
                    StatusUpdatedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    // Secure metadata storage
                    CardHolderName = model.PaymentMethod == "Credit Card" ? model.CardHolderName : null,
                    CardNumberLastFour = model.PaymentMethod == "Credit Card" ? model.CardNumber?.Substring(model.CardNumber.Length - 4) : null,
                    TransactionId = model.PaymentMethod == "Credit Card" ? Guid.NewGuid().ToString("N").ToUpper() : null, // Mock transaction ID
                    
                    // Gift Options
                    IsGift = model.IsGift,
                    GiftMessage = model.IsGift ? model.GiftMessage : null
                };

                if (model.UseShippingAsBilling)
                {
                    order.BillingAddress = model.Address;
                    order.BillingCity = model.City;
                    order.BillingState = model.State;
                    order.BillingPostalCode = model.ZipCode;
                    order.BillingCountry = model.Country;
                }
                else
                {
                    order.BillingAddress = model.BillingAddress;
                    order.BillingCity = model.BillingCity;
                    order.BillingState = model.BillingState;
                    order.BillingPostalCode = model.BillingZipCode;
                    order.BillingCountry = model.BillingCountry;
                }

                // Calculate Item Totals and Headers
                order.Subtotal = cartItems.Sum(c => c.Subtotal);
                _context.Orders.Add(order);
                
                // Add Initial History
                _context.StatusHistories.Add(new OrderStatusHistory
                {
                    Order = order,
                    Status = OrderStatus.Pending,
                    Notes = "Order placed successfully",
                    ChangedBy = "Customer",
                    CreatedAt = DateTime.UtcNow
                });
                
                // Create Order Items & Update Stock
                decimal totalDiscount = 0;
                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        Order = order,
                        StyleCode = cartItem.StyleCode,
                        ProductName = cartItem.Item.ItemName,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Item.SellingPrice, // Current DB price
                        DiscountPercentage = cartItem.Item.DiscountPercentage,
                        ProductImageUrl = cartItem.Item.PrimaryImageUrl,
                        BrandName = cartItem.Item.Brand?.BrandType,
                        CategoryName = cartItem.Item.Category?.CategoryName,
                        ProductTypeName = cartItem.Item.ProductType?.ProductTypeName,
                        GoldCarat = cartItem.Item.GoldKarat?.GoldCarat,
                        GoldWeight = cartItem.Item.GoldWeight,
                        CreatedDate = DateTime.UtcNow
                    };
                    orderItem.CalculateSubtotal();
                    totalDiscount += orderItem.DiscountAmount;
                    _context.OrderItems.Add(orderItem);

                    // Decrement Stock
                    cartItem.Item.Quantity -= cartItem.Quantity;
                }

                order.DiscountAmount = totalDiscount;
                order.TaxAmount = 3.00m; // Static tax amount as per requirement
                order.ShippingCharges = model.ShippingMethod == "Express" ? 10.00m : 0.00m; // Example fixed rates
                order.CalculateTotalAmount();

                await _context.SaveChangesAsync();
                
                // Clear Cart
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Trigger Notification
                await _notificationService.CreateNotificationAsync(
                    "New Order Received",
                    $"New order #{order.OrderNumber} placed by {order.CustomerName} for {order.TotalAmount:C}",
                    NotificationType.NewOrder,
                    order.OrderNumber,
                    "Customer");

                // Trigger Emails (Background-ish)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _orderEmailService.SendOrderConfirmationEmailAsync(order);
                        await _orderEmailService.SendNewOrderAdminNotificationAsync(order);
                    }
                    catch (Exception)
                    {
                        // Background failure shouldn't affect user experience
                    }
                });

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool ValidateLuhn(string number)
        {
            number = number.Replace(" ", "").Replace("-", "");
            if (string.IsNullOrEmpty(number) || !number.All(char.IsDigit)) return false;

            int sum = 0;
            bool isSecond = false;
            for (int i = number.Length - 1; i >= 0; i--)
            {
                int d = number[i] - '0';
                if (isSecond)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
                isSecond = !isSecond;
            }
            return (sum % 10 == 0);
        }

        private bool ValidateExpiry(string expiryDate)
        {
            if (string.IsNullOrEmpty(expiryDate)) return false;
            var parts = expiryDate.Split('/');
            if (parts.Length != 2) return false;

            if (int.TryParse(parts[0], out int month) && int.TryParse(parts[1], out int year))
            {
                if (month < 1 || month > 12) return false;
                
                var currentYear = DateTime.UtcNow.Year % 100;
                var currentMonth = DateTime.UtcNow.Month;
                var fullYear = 2000 + year;

                if (year < currentYear || (year == currentYear && month < currentMonth))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
