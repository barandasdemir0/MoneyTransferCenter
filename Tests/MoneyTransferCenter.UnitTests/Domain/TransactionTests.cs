using FluentAssertions;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Exceptions;

namespace MoneyTransferCenter.UnitTests.Domain;

public class TransactionTests
{
    //eksi veya sıfır tutarında transferde hata
    [Fact]
    public void TransferAmount_ShouldThrowException_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var senderId = Guid.CreateVersion7();
        var receiverId = Guid.CreateVersion7();
        // Act
        Action act = () => Transaction.Create(senderId, receiverId, 0, "Sıfır TL Gönderme Testi");
        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Transfer tutarı sıfırdan büyük olmalıdır.", "INVALID_AMOUNT");
    }

    //kendi hesabına transferde hata
    [Fact]
    public void TransferAmount_ShouldThrowException_WhenSenderAndReceiverAreSame()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        // Act
        Action act = () => Transaction.Create(accountId, accountId, 100, "Kendi Hesabına Transfer Testi");
        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Kendi hesabınıza transfer yapamazsınız.", "SELF_TRANSFER");
    }


    //bekleme durumunda 
    [Fact]
    public void TransactionStatus_ShouldBePending_WhenTransactionIsCreated()
    {
        // Arrange
        var senderId = Guid.CreateVersion7();
        var receiverId = Guid.CreateVersion7();
        // Act
        var transaction = Transaction.Create(senderId, receiverId, 100, "Transfer Testi");
        // Assert
        transaction.Status.Should().Be(MoneyTransferCenter.Domain.Enums.TransactionStatus.Pending);
    }

    //tamamlanmış durumda
    [Fact]
    public void TransactionStatus_ShouldBeCompleted_WhenTransactionIsCompleted()
    {
        // Arrange
        var senderId = Guid.CreateVersion7();
        var receiverId = Guid.CreateVersion7();
        var transaction = Transaction.Create(senderId, receiverId, 100, "Transfer Testi");
        // Act
        transaction.MarkAsCompleted();
        // Assert
        transaction.Status.Should().Be(MoneyTransferCenter.Domain.Enums.TransactionStatus.Completed);
    }


}
