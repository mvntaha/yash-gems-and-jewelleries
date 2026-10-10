using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class OrderEmailService : IOrderEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderEmailService> _logger;

        public OrderEmailService(
            IEmailSender emailSender, 
            IConfiguration configuration, 
            ILogger<OrderEmailService> logger)
        {
            _emailSender = emailSender;
            _configuration = configuration;
            _logger = logger;
        }

        // Send Order Confirmation Email To Customer
        public async Task SendOrderConfirmationEmailAsync(Order order)
        {
            try
            {
                var subject = $"Order Confirmation - {order.OrderNumber}";
                var customerName = order.CustomerName ?? "Valued Customer";
                var body = BuildOrderEmailBody(order, "Order Confirmation", 
                    $"Thank you for your order, {customerName}! Your order has been received and is being processed.");

                var email = order.Email ?? string.Empty;
                if (!string.IsNullOrEmpty(email))
                {
                    await _emailSender.SendEmailAsync(email, subject, body);
                    _logger.LogInformation("Order confirmation email sent for {OrderNumber}", order.OrderNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation email for {OrderNumber}", order.OrderNumber);
            }
        }

        // Send New Order Admin Notification To Admin
        public async Task SendNewOrderAdminNotificationAsync(Order order)
        {
            try
            {
                var adminEmail = _configuration["ApplicationSettings:CompanyEmail"] ?? "admin@yash.com";
                var subject = $"New Order Received - {order.OrderNumber}";
                var body = BuildOrderEmailBody(order, "New Order Notification", 
                    $"A new order has been placed by {order.CustomerName}. Please process it at your earliest convenience.");

                await _emailSender.SendEmailAsync(adminEmail, subject, body);
                _logger.LogInformation("Admin notification email sent for {OrderNumber}", order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin notification email for {OrderNumber}", order.OrderNumber);
            }
        }

        // Build Order Email Body
        private string BuildOrderEmailBody(Order order, string title, string message)
        {
            var companyName = _configuration["ApplicationSettings:CompanyName"] ?? "Yash Gems & Jewelleries";
            var sb = new StringBuilder();

            sb.Append($@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>
                <div style='background-color: #f8f8f8; padding: 20px; text-align: center; border-bottom: 2px solid #b08d43;'>
                    <h1 style='color: #b08d43; margin: 0;'>{companyName}</h1>
                    <p style='margin: 10px 0 0;'>{title}</p>
                </div>
                
                <div style='padding: 20px;'>
                    <p>{message}</p>
                    
                    <div style='background-color: #fafafa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='margin-top: 0; color: #b08d43; border-bottom: 1px solid #eee; padding-bottom: 10px;'>Order Summary</h3>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 5px 0; font-weight: bold;'>Order Number:</td>
                                <td style='padding: 5px 0; text-align: right;'>{order.OrderNumber}</td>
                            </tr>
                            <tr>
                                <td style='padding: 5px 0; font-weight: bold;'>Order Date:</td>
                                <td style='padding: 5px 0; text-align: right;'>{order.OrderDate:MMM dd, yyyy HH:mm}</td>
                            </tr>
                            <tr>
                                <td style='padding: 5px 0; font-weight: bold;'>Payment Method:</td>
                                <td style='padding: 5px 0; text-align: right;'>{order.PaymentMethod}</td>
                            </tr>
                            <tr>
                                <td style='padding: 5px 0; font-weight: bold;'>Order Status:</td>
                                <td style='padding: 5px 0; text-align: right;'>{order.OrderStatus.ToString()}</td>
                            </tr>
                        </table>
                    </div>

                    <h3 style='color: #b08d43;'>Order Items</h3>
                    <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                        <thead>
                            <tr style='border-bottom: 2px solid #eee; text-align: left;'>
                                <th style='padding: 10px 0;'>Product</th>
                                <th style='padding: 10px 0; text-align: center;'>Qty</th>
                                <th style='padding: 10px 0; text-align: right;'>Price</th>
                            </tr>
                        </thead>
                        <tbody>");

            foreach (var item in order.OrderItems)
            {
                sb.Append($@"
                            <tr style='border-bottom: 1px solid #eee;'>
                                <td style='padding: 10px 0;'>
                                    <div style='font-weight: bold;'>{item.ProductName}</div>
                                    <div style='font-size: 12px; color: #777;'>{item.StyleCode}</div>
                                </td>
                                <td style='padding: 10px 0; text-align: center;'>{item.Quantity}</td>
                                <td style='padding: 10px 0; text-align: right;'>{item.UnitPrice:C}</td>
                            </tr>");
            }

            sb.Append($@"
                        </tbody>
                    </table>

                    <div style='width: 250px; margin-left: auto;'>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 5px 0;'>Subtotal:</td>
                                <td style='padding: 5px 0; text-align: right;'>{order.Subtotal:C}</td>
                            </tr>
                            <tr>
                                <td style='padding: 5px 0;'>Tax:</td>
                                <td style='padding: 5px 0; text-align: right;'>{order.TaxAmount:C}</td>
                            </tr>
                            <tr>
                                <td style='padding: 5px 0;'>Shipping:</td>
                                <td style='padding: 5px 0; text-align: right;'>{order.ShippingCharges:C}</td>
                            </tr>");

            if (order.DiscountAmount > 0)
            {
                sb.Append($@"
                            <tr>
                                <td style='padding: 5px 0; color: #d9534f;'>Discount:</td>
                                <td style='padding: 5px 0; text-align: right; color: #d9534f;'>-{order.DiscountAmount:C}</td>
                            </tr>");
            }

            sb.Append($@"
                            <tr style='border-top: 2px solid #b08d43; font-weight: bold; font-size: 18px;'>
                                <td style='padding: 10px 0;'>Total:</td>
                                <td style='padding: 10px 0; text-align: right; color: #b08d43;'>{order.TotalAmount:C}</td>
                            </tr>
                        </table>
                    </div>

                    <div style='margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px;'>
                        <h3 style='color: #b08d43;'>Shipping Address</h3>
                        <p style='margin: 5px 0;'>{order.CustomerName}</p>
                        <p style='margin: 5px 0;'>{order.ShippingAddress}</p>
                        <p style='margin: 5px 0;'>{order.ShippingCity}, {order.ShippingState} {order.ShippingPostalCode}</p>
                        <p style='margin: 5px 0;'>{order.ShippingCountry}</p>
                        <p style='margin: 5px 0;'>Ph: {order.PhoneNumber}</p>
                    </div>

                    <div style='text-align: center; margin-top: 40px; color: #777; font-size: 12px;'>
                        <p>&copy; {DateTime.UtcNow.Year} {companyName}. All rights reserved.</p>
                        <p>This is an automated email, please do not reply.</p>
                    </div>
                </div>
            </div>");

            return sb.ToString();
        }
    }
}
