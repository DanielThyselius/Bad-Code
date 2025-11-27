namespace InvoiceSender.Test;

using Moq;
using System.Reflection;

public class InvoiceHelperTests
{
    [Theory]
    [InlineData("daniel@mindfulstack.se", 1)]
    [InlineData("ruben@mindfulstack.se", 0)]
    public void SendsCorrectNumberOfEmails(string email, int expected)
    {
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(x => x.GetUser("daniel@mindfulstack.se")).Returns(
            new User
            {
                Id = 1,
                Name = "Daniel",
                Email = email
            }
        );
        mockDb.Setup(x => x.GetInvoice(1)).Returns(
            new Invoice
            {
                Amount = 10,
                DueDate = DateTime.Now.AddDays(10)
            });


        mockDb.Setup(x => x.GetUser("ruben@mindfulstack.se")).Returns(
            new User
            {
                Id = 2,
                Name = "Ruben",
                Email = email
            }
        );

        //mockDb.Setup(x => x.GetUser("emmanuel@mindfulstack.se")).Returns(
        //    new User
        //    {
        //        Id = 3,
        //        Name = "Emmanuel",
        //        Email = email
        //    }
        //);
        //mockDb.Setup(x => x.GetInvoice(3)).Returns(
        //    new Invoice
        //    {
        //        Amount = 10,
        //        DueDate = DateTime.Now.AddDays(10)
        //    });


        var mockEmail = new Mock<IEmailSender>();
  
        var sut = new InvoiceHelper(mockDb.Object, mockEmail.Object);
        // Act
        var actual = sut.Run(email);

        // Assert
        Assert.Equal(expected, actual.Count);
    }

    // TODO: Ajust for multiple messages
    [Fact]
    public void SendsCorrectMessage()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(x => x.GetUser("daniel@mindfulstack.se")).Returns(
            new User
            {
                Id = 1,
                Name = "Daniel",
                Email = "daniel@mindfulstack.se"
            });
        mockDb.Setup(x => x.GetInvoice(1)).Returns(
          new Invoice
          {
              Amount = 10,
              DueDate = DateTime.Now.AddDays(10)
          });

        var mockEmail = new Mock<IEmailSender>();

        var expected = "Dear Daniel,\n\nYour invoice of 10 is " +
                       "due in 10 days.\n\nBest regards,\nThe Invoice Team";

        var sut = new InvoiceHelper(mockDb.Object, mockEmail.Object);
        // Act
        var actual = sut.Run("daniel@mindfulstack.se");

        // Assert
        Assert.Equal(expected, actual[0]);
    }


    [Fact]
    public void VerifyEmailIsSent()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(x => x.GetUser("daniel@mindfulstack.se")).Returns(
            new User
            {
                Id = 1,
                Name = "Daniel",
                Email = "daniel@mindfulstack.se"
            });
        mockDb.Setup(x => x.GetInvoice(1)).Returns(
          new Invoice
          {
              Amount = 10,
              DueDate = DateTime.Now.AddDays(10)
          });

        var mockEmail = new Mock<IEmailSender>();

        var sut = new InvoiceHelper(mockDb.Object, mockEmail.Object);

        // Act
        var actual = sut.Run("daniel@mindfulstack.se");

        // Assert
        mockEmail.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public void CalculatesDueDateProperly()
    {
        // TODO: Test different due dates (especially around 0)
    }

    [Fact]
    public void PrivateMethodTest()
    {
        // Arrange
        var db = new Database();
        var emailSender = new EmailSender();

        var helper = new InvoiceHelper(db, emailSender);

        var expected = "Dear Daniel,\n\nYour invoice of 10 is " +
                       "due in 10 days.\n\nBest regards,\nThe Invoice Team";
        // Act
        var actual = helper.TestGetInvoice("Daniel", 10, 10);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PrivateMethodTestWithReflection()
    {
        // Arrange
        var db = new Database();
        var emailSender = new EmailSender();

        var helper = new InvoiceHelper(db, emailSender);

        var expected = "Dear Daniel,\n\nYour invoice of 10 is " +
                       "due in 10 days.\n\nBest regards,\nThe Invoice Team";
        var args = new object[] { "Daniel", 10m, 10 };

        // Act
        var privat = typeof(InvoiceHelper).GetMethod("GetInvoice", BindingFlags.NonPublic | BindingFlags.Instance);
        var actual = privat.Invoke(helper, args);

        // Assert
        Assert.Equal(expected, actual);
    }
}

internal class TestDatabase : IDatabase
{
    public User GetUser(string email)
    {
        switch (email)
        {
            case "daniel@mindfulstack.se":
                return new User
                {
                    Id = 1,
                    Name = "Daniel",
                    Email = email
                };
            case "ruben@mindfulstack.se":
                return new User
                {
                    Id = 2,
                    Name = "Ruben",
                    Email = email
                };
            default:
                return new User
                {
                    Id = 0,
                    Name = "Unknown",
                    Email = email
                };
        }
    }

    public Invoice? GetInvoice(int userId)
    {
        switch (userId)
        {
            case 1:
                return new Invoice
                {
                    Amount = 10,
                    DueDate = DateTime.Now.AddDays(10)
                };
            default:
                return null;
        }

    }
}

internal class TestEmailSender : IEmailSender
{
    public void SendEmail(string email, string subject, string messsage)
    {
        // Do nothing
    }
}