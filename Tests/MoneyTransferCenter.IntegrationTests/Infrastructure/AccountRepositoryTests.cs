using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Infrastructure.Data;
using MoneyTransferCenter.Infrastructure.Repositories;
using NSubstitute;

namespace MoneyTransferCenter.IntegrationTests.Infrastructure;

public class AccountRepositoryTests
{

    
    [Fact]
    public async Task AddAsync_ShouldSaveAccountToDatabase()
    {

        // Arrange
        //InMemoryDatabase kullanarak geçici bir veritabanı oluşturur ve bu veritabanını AppDbContext ile kullanır. Bu sayede test sırasında gerçek bir veritabanına ihtiyaç duymadan veri ekleme, güncelleme ve sorgulama işlemleri yapılabilir.
        var options = new DbContextOptionsBuilder<AppDbContext>()
           .UseInMemoryDatabase(Guid.CreateVersion7().ToString())
           .Options;

      //gelen kullanıcnın bilgisini almak için oluşturdum
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.CreateVersion7());

        await using var context = new AppDbContext(options, currentUserService);

        var repo = new AccountRepository(context);

        var account = new Account(Guid.CreateVersion7(), "TR123456789012345678901234");

        //act
        await repo.AddAsync(account);
        await context.SaveChangesAsync();
      


        //assert
        var result = await context.Accounts
            .FirstOrDefaultAsync(a => a.IBAN == "TR123456789012345678901234");

       
        result.Should().NotBeNull();
        result!.IBAN.Should().Be("TR123456789012345678901234");
    }
}
