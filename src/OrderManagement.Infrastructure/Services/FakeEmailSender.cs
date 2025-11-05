using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Services;

public class FakeEmailSender : IEmailSender
{
    private readonly ILogger<FakeEmailSender> _logger;

    public FakeEmailSender(ILogger<FakeEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("FAKE EMAIL SENDER: Simulating email send to '{To}' with subject '{Subject}'", to, subject);
        _logger.LogDebug("FAKE EMAIL SENDER: Email body: {Body}", htmlBody);
        
        return Task.CompletedTask;
    }

    public Task SendOrderConfirmationAsync(string customerEmail, string customerName, string orderNumber, decimal totalAmount)
    {
        _logger.LogInformation("FAKE EMAIL SENDER: Simulating order confirmation email to '{CustomerEmail}' for order '{OrderNumber}' with total ${TotalAmount}", 
            customerEmail, orderNumber, totalAmount);
        
        return Task.CompletedTask;
    }

    public Task SendOrderCancellationAsync(string customerEmail, string customerName, string orderNumber)
    {
        _logger.LogInformation("FAKE EMAIL SENDER: Simulating order cancellation email to '{CustomerEmail}' for order '{OrderNumber}'", 
            customerEmail, orderNumber);
        
        return Task.CompletedTask;
    }
}