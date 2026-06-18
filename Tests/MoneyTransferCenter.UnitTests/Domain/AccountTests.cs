using FluentAssertions;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Enums;
using MoneyTransferCenter.Domain.Exceptions;
using static MongoDB.Driver.WriteConcern;

namespace MoneyTransferCenter.UnitTests.Domain;

public class AccountTests
{

    //hesap oluşturma
    [Fact]
    public void Account_ShouldBePassive_WhenCreated()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");

        //assert
        account.Status.Should().Be(AccountStatus.Passive);
        account.Balance.Should().Be(0);
    }

    //pasif olan hesaba para yatırma
    [Fact]
    public void Deposit_ShouldThrowDomainException_WhenAccountIsPassive()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");
        //act
        Action act = () => account.Deposit(100);

        //assert
        act.Should().Throw<DomainException>().WithMessage("Aktif olmayan hesaba para yatırılamaz.", "ACCOUNT_NOT_ACTIVE");
    }

    //aktif olmaayan hesabı aktif yapma
    [Fact]
    public void CompleteProfile_ShouldActivateAccount_WhenProfileIsComplete()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");

        //act
        account.CompleteProfile("Test Mah.", "Istanbul", "34000", "05554443322", DateTimeOffset.UtcNow.AddYears(-20));

        //assert
        account.Status.Should().Be(AccountStatus.Active);
        account.IsProfileComplete().Should().BeTrue();


    }


    // negatif para yüklemeye çalışmak
    [Fact]
    public void Deposit_ShouldThrowException_WhenAmountIsNegative()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");
        account.Activate();
        //act
        Action act = () => account.Deposit(-100);
        //assert
        act.Should().Throw<DomainException>().WithMessage("Tutar sıfırdan büyük olmalıdır.", "INVALID_AMOUNT");
    }


    //pozitif para  yüklemek
    [Fact]
    public void Deposit_ShouldIncreaseBalance_WhenAmountIsPositive()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");
        account.Activate();
        //act
        account.Deposit(100);
        //assert
        account.Balance.Should().Be(100);
    }

    //aktif olan hesaba para yatırma
    [Fact]
    public void Deposit_ShouldIncreaseBalance_WhenAccountIsActive()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");
        account.CompleteProfile("Test Mah.", "Istanbul", "34000", "05554443322", DateTimeOffset.UtcNow.AddYears(-20));

        //act
        account.Deposit(100);

        //assert
        account.Balance.Should().Be(100);


    }


    //aktif olan hesaptan para çekme
    [Fact]
    public void Withdraw_ShouldDecreaseBalance_WhenAccountIsActiveAndSufficientBalance()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");
        account.CompleteProfile("Test Mah.", "Istanbul", "34000", "05554443322", DateTimeOffset.UtcNow.AddYears(-20));
        account.Deposit(500);

        //act
        account.Withdraw(100);

        //assert
        account.Balance.Should().Be(400);

    }

    //aktif olan hesaptan para çekmeye çalışmak ama yeterli bakiye yok
    [Fact]
    public void Withdraw_ShouldThrowDomainException_WhenInsufficientFunds()
    {
        //arrange
        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");
        account.CompleteProfile("Test Mah.", "Istanbul", "34000", "05554443322", DateTimeOffset.UtcNow.AddYears(-20));
        account.Deposit(50);

        //act
        Action act = () =>  account.Withdraw(100);

        //assert
        act.Should()
                .Throw<DomainException>()
                .WithMessage("Yetersiz bakiye*");



    }

    //bakiyesi olan hesabı kapatmaya çalışmak
    [Fact]
    public void Close_ShouldThrowDomainException_WhenBalanceIsNotZero()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), "TR123456789012345678901234");
        account.CompleteProfile("Test", "Test", "Test", "Test", DateTimeOffset.UtcNow.AddYears(-20));
        account.Deposit(100);
        // Act
        Action act = () => account.Close();
        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Bakiyesi olan hesap kapatılamaz.", "BALANCE_NOT_ZERO");
    }

    //kapalı hesap üzerinden adres güncelleme yapmaya çalışmak
    [Fact]
    public void CompleteProfile_ShouldThrowDomainException_WhenAccountIsClosed()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), "TR123456789012345678901234");
        account.CompleteProfile("Test", "Test", "Test", "Test", DateTimeOffset.UtcNow.AddYears(-20));
        account.Close();
        // Act
        Action act = () => account.CompleteProfile("New Address", "New City", "New Postal Code", "New Phone", DateTimeOffset.UtcNow.AddYears(-30));
        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Kapatılmış hesap güncellenemez.", "ACCOUNT_CLOSED");
    }



}
