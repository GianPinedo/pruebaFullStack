using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "1025");
        var fromEmail = _configuration["Email:FromEmail"] ?? "no-reply@orders.local";
        var fromName = _configuration["Email:FromName"] ?? "Order Management";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendOrderConfirmationAsync(string customerEmail, string customerName, string orderNumber, decimal totalAmount)
    {
        var subject = $"Order Confirmation - {orderNumber}";
        var htmlBody = await GetOrderConfirmationTemplateAsync(customerName, orderNumber, totalAmount);
        await SendEmailAsync(customerEmail, subject, htmlBody);
    }

    public async Task SendOrderCancellationAsync(string customerEmail, string customerName, string orderNumber)
    {
        var subject = $"Order Cancelled - {orderNumber}";
        var htmlBody = await GetOrderCancellationTemplateAsync(customerName, orderNumber);
        await SendEmailAsync(customerEmail, subject, htmlBody);
    }

    private async Task<string> GetOrderConfirmationTemplateAsync(string customerName, string orderNumber, decimal totalAmount)
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "email_templates", "order-confirmation.html");
        
        if (File.Exists(templatePath))
        {
            var template = await File.ReadAllTextAsync(templatePath);
            return template
                .Replace("{{CustomerName}}", customerName)
                .Replace("{{OrderNumber}}", orderNumber)
                .Replace("{{TotalAmount}}", totalAmount.ToString("C"))
                .Replace("{{Date}}", DateTime.Now.ToString("F"));
        }

        // Fallback template
        return $@"
            <html>
            <body>
                <h2>Order Confirmation</h2>
                <p>Dear {customerName},</p>
                <p>Thank you for your order! Your order <strong>{orderNumber}</strong> has been confirmed.</p>
                <p>Total Amount: <strong>${totalAmount:F2}</strong></p>
                <p>We will process your order shortly.</p>
                <br>
                <p>Best regards,<br>Order Management Team</p>
            </body>
            </html>";
    }

    private async Task<string> GetOrderCancellationTemplateAsync(string customerName, string orderNumber)
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "email_templates", "order-cancelled.html");
        
        if (File.Exists(templatePath))
        {
            var template = await File.ReadAllTextAsync(templatePath);
            return template
                .Replace("{{CustomerName}}", customerName)
                .Replace("{{OrderNumber}}", orderNumber)
                .Replace("{{Date}}", DateTime.Now.ToString("F"));
        }

        // Fallback template
        return $@"
            <html>
            <body>
                <h2>Order Cancellation</h2>
                <p>Dear {customerName},</p>
                <p>Your order <strong>{orderNumber}</strong> has been cancelled.</p>
                <p>If you have any questions, please contact our support team.</p>
                <br>
                <p>Best regards,<br>Order Management Team</p>
            </body>
            </html>";
    }
}