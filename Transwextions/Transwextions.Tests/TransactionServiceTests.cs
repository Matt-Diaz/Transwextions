using FluentAssertions;
using Transwextions.App.Services;
using Transwextions.Data.Models;
using Transwextions.Tests.Fakes;
using Transwextions.Tests.TestHelpers;

namespace Transwextions.Tests;

[TestFixture]
public class TransactionServiceTests
{
    [Test]
    public async Task AddAsync_Error_GuidAlreadyExists()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Arrange
            var uid1 = Guid.Parse("00000001-4496-4B99-A0C3-111F9AB55149");

            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);

            await svc.AddAsync(new TransactionModel
            {
                Description = new string('x', 10),
                UniqueIdentifier = uid1,
                IsDeleted = false,
                AmountTotalCents = 222
            });

            var serviceResult = await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid1,
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 1000
            });

            // Assert
            serviceResult.IsSuccess.Should().BeFalse();
            serviceResult.ErrorMessage.Should().Contain("UniqueIdentifier already exists");
            events.Added.Should().ContainSingle();
        }
    }

    [Test]
    public async Task AddAsync_Error_DescriptionCharacterLengthOverFifty()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Arrange
            var tooLong = new string('x', 51);

            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);

            var serviceResult = await svc.AddAsync(new TransactionModel
            {
                Description = tooLong,
                AmountTotalCents = 100
            });

            // Assert
            serviceResult.IsSuccess.Should().BeFalse();
            serviceResult.ErrorMessage.Should().Contain("character limit is 50");
            events.Added.Should().BeEmpty();
        }
    }

    [Test]
    public async Task AddAsync_Error_DescriptionNull()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);
            var serviceResult = await svc.AddAsync(new TransactionModel
            {
                Description = null,
                AmountTotalCents = 100,
            });

            // Assert
            serviceResult.IsSuccess.Should().BeFalse();
            serviceResult.ErrorMessage.Should().Contain("description is null");
            events.Added.Should().BeEmpty();
        }
    }

    [Test]
    public async Task AddAsync_Error_NullModel()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);
            var serviceResult = await svc.AddAsync(null);

            // Assert
            serviceResult.IsSuccess.Should().BeFalse();
            serviceResult.ErrorMessage.Should().Contain("Model is null");
            events.Added.Should().BeEmpty();
        }
    }

    [Test]
    public async Task AddAsync_Success_ApplicationEventShouldBeTriggered()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Arrange
            var description = "Valid Description";
            ulong amountTotalCents = 1000;

            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);

            var serviceResult = await svc.AddAsync(new TransactionModel
            {
                Description = description,
                AmountTotalCents = amountTotalCents
            });

            // Assert
            serviceResult.IsSuccess.Should().BeTrue();
            serviceResult.ErrorMessage.Should().BeNullOrWhiteSpace();
            events.Added.Should().ContainSingle();
            events.Added.First().Description.Should().Be(description);
            events.Added.First().AmountTotalCents.Should().Be(amountTotalCents);
        }
    }

    [Test]
    public async Task GetTransactionsTotalCentsAsync_Success_TotalShouldBeCorrect()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Arrange
            ulong correctTotal = 444;

            // Mock
            var events = new FakeApplicationEventsService();
            var svc = new TransactionsService(db, events);

            await svc.AddAsync(new TransactionModel
            {
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 222
            });

            await svc.AddAsync(new TransactionModel
            {
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 222
            });

            await svc.AddAsync(new TransactionModel
            {
                Description = new string('x', 10),
                IsDeleted = true,
                AmountTotalCents = 1111
            });

            //Act
            var serviceResult = await svc.GetTransactionsTotalCentsAsync();

            // Assert
            serviceResult.IsSuccess.Should().BeTrue();
            serviceResult.ErrorMessage.Should().BeNullOrWhiteSpace();
            events.Added.Should().HaveCount(3);
            serviceResult.Object.Should().Be(correctTotal);
        }
    }

    [Test]
    public async Task GetAllAsync_Success_ConfirmAllGuidsPresent()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Arrange
            var uid1 = Guid.Parse("00000001-4496-4B99-A0C3-111F9AB55149");
            var uid2 = Guid.Parse("00000002-4496-4B99-A0C3-111F9AB55149");
            var uid3 = Guid.Parse("00000003-4496-4B99-A0C3-111F9AB55149");

            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);

            await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid1,
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 222
            });

            await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid2,
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 222
            });

            await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid3,
                Description = new string('x', 10),
                IsDeleted = true,
                AmountTotalCents = 1111
            });

            var serviceResult = await svc.GetAllAsync();

            // Assert
            serviceResult.IsSuccess.Should().BeTrue();
            serviceResult.ErrorMessage.Should().BeNullOrWhiteSpace();
            events.Added.Count().Should().Be(3);
            serviceResult.Object.Should().HaveCount(2);
            serviceResult.Object.Should().Contain(p => p.UniqueIdentifier == uid1);
            serviceResult.Object.Should().Contain(p => p.UniqueIdentifier == uid2);
        }
    }

    [Test]
    public async Task GetByGuidAsync_Success_ConfirmGuid()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Arrange
            var uid1 = Guid.Parse("00000001-4496-4B99-A0C3-111F9AB55149");
            var uid2 = Guid.Parse("00000002-4496-4B99-A0C3-111F9AB55149");
            var uid3 = Guid.Parse("00000003-4496-4B99-A0C3-111F9AB55149");

            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);

            var r1 = await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid1,
                Description = new string('x', 10),
                IsDeleted = true,
                AmountTotalCents = 222
            });

            var r2 = await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid2,
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 222
            });

            var r3 = await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid3,
                Description = new string('x', 10),
                IsDeleted = true,
                AmountTotalCents = 1111
            });

            var serviceResult = await svc.GetByGuidAsync(uid2);

            // Assert
            serviceResult.IsSuccess.Should().BeTrue();
            serviceResult.ErrorMessage.Should().BeNullOrWhiteSpace();
            serviceResult.Object.Should().NotBeNull();
            serviceResult.Object.UniqueIdentifier.Should().Be(uid2);
            events.Added.Count().Should().Be(3);
            events.Deleted.Count().Should().Be(0);
        }
    }

    [Test]
    public async Task DeleteAsync_Success_ConfirmGuidDoesNotExist()
    {
        var (connection, db) = SqliteTestHelpers.CreateContext();
        await using (connection)
        await using (db)
        {
            // Arrange
            var uid1 = Guid.Parse("00000001-4496-4B99-A0C3-111F9AB55149");
            var uid2 = Guid.Parse("00000002-4496-4B99-A0C3-111F9AB55149");
            var uid3 = Guid.Parse("00000003-4496-4B99-A0C3-111F9AB55149");

            // Mock
            var events = new FakeApplicationEventsService();

            // Act
            var svc = new TransactionsService(db, events);

            var r1 = await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid1,
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 222
            });

            var r2 = await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid2,
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 222
            });

            var r3 = await svc.AddAsync(new TransactionModel
            {
                UniqueIdentifier = uid3,
                Description = new string('x', 10),
                IsDeleted = false,
                AmountTotalCents = 1111
            });

            var deleteServiceResult = await svc.DeleteAsync(uid2);
            var serviceResult = await svc.GetByGuidAsync(uid2);

            // Assert
            deleteServiceResult.IsSuccess.Should().BeTrue();
            deleteServiceResult.ErrorMessage.Should().BeNullOrWhiteSpace();
            serviceResult.IsSuccess.Should().BeFalse();
            serviceResult.ErrorMessage.Should().Contain("Model does not exist");
            serviceResult.Object.Should().BeNull();
            events.Added.Count().Should().Be(3);
            events.Deleted.Count().Should().Be(1);
        }
    }
}