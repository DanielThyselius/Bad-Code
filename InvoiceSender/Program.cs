// This is NOT how you should be coding!
// Well, now it looks a bit better, no?

namespace InvoiceSender;

internal class Program
{
    public static void Main(string[] args)
    {
        var db = new Database();
        var emailSender = new EmailSender();
        
        var helper = new InvoiceHelper(db, emailSender);

        var email = "john@example.com";
        
        // Should not be allowed!
        //var s = helper.GetInvoice("John", 100, 10);
        
        var invoicesSent = helper.Run(email);
        
        Console.WriteLine($"Sent {invoicesSent} invoices to {email}");

    }
}

public class InvoiceHelper
{
    private readonly IDatabase _db;
    private readonly IEmailSender _emailSender;

    public InvoiceHelper(IDatabase db, IEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    public List<string> Run(string email)
    {
        var messages = new List<string>();
        var user = _db.GetUser(email);
        // TODO: Enable several invoices per user
        // Could this be more than one?
        var invoice = _db.GetInvoice(user.Id);
        if (invoice is null)
            return messages;
        
        var dueDate = invoice.DueDate;
        var daysToDueDate = (dueDate.Date - DateTime.Now.Date).Days;

        var message = GetInvoice(user.Name, invoice.Amount, daysToDueDate);
        SendInvoice(message, user.Email);
        messages.Add(message);
        
        return messages;
    }
    
    private string GetInvoice(string name, decimal amount, int daysToDueDate)
    {
        // Note: This method has tests using reflection, but it's not the best way to test it
        // Update tests if you change this method
        return $"Dear {name},\n\nYour invoice of {amount} is " +
               $"due in {daysToDueDate} days.\n\nBest regards,\nThe Invoice Team";
    }

    private void SendInvoice(string message, string email)
    {
        _emailSender.SendEmail(email, "Invoice Reminder", message);
    }
    
    // TODO: how do we test this logic through the Run method?
    internal string TestGetInvoice(string name, decimal amount, int daysToDueDate)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            throw new InvalidOperationException("This method should not be called in production!");
        }
        
        return GetInvoice(name, amount, daysToDueDate);
    }
}