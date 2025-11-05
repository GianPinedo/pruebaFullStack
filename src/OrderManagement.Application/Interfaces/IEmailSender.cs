namespace OrderManagement.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendOrderConfirmationAsync(string customerEmail, string customerName, string orderNumber, decimal totalAmount);
    Task SendOrderCancellationAsync(string customerEmail, string customerName, string orderNumber);
}