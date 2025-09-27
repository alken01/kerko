using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Kerko.Infrastructure;
using Kerko.Services;
using Kerko.Models;

namespace Kerko.Tests.Unit;

public class SearchServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<SearchService>> _loggerMock;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<SearchService>>();
        _searchService = new SearchService(_context, _loggerMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Person.AddRange(
            new Person
            {
                Emer = "John",
                Mbiemer = "Doe",
                Adresa = "123 Main St",
                Qyteti = "Tirana"
            },
            new Person
            {
                Emer = "Jane",
                Mbiemer = "Smith",
                Adresa = "456 Oak Ave",
                Qyteti = "Durres"
            },
            new Person
            {
                Emer = "Alice",
                Mbiemer = "Johnson",
                Adresa = "789 Pine Rd",
                Qyteti = "Vlore"
            }
        );

        _context.Rrogat.AddRange(
            new Rrogat
            {
                NumriPersonal = "K12345678A",
                Emri = "John",
                Mbiemri = "Doe",
                PagaBruto = 50000,
                Profesioni = "Engineer"
            },
            new Rrogat
            {
                NumriPersonal = "K87654321B",
                Emri = "Jane",
                Mbiemri = "Smith",
                PagaBruto = 60000,
                Profesioni = "Manager"
            }
        );

        _context.Targat.AddRange(
            new Targat
            {
                NumriTarges = "AA123BB",
                Marka = "BMW",
                Modeli = "X5",
                Emri = "John",
                Mbiemri = "Doe"
            },
            new Targat
            {
                NumriTarges = "CC456DD",
                Marka = "Mercedes",
                Modeli = "E-Class",
                Emri = "Jane",
                Mbiemri = "Smith"
            }
        );

        _context.Patronazhist.AddRange(
            new Patronazhist
            {
                NumriPersonal = "K12345678A",
                Emri = "John",
                Mbiemri = "Doe",
                Tel = "+355691234567",
                QV = "001"
            },
            new Patronazhist
            {
                NumriPersonal = "K87654321B",
                Emri = "Jane",
                Mbiemri = "Smith",
                Tel = "+355697654321",
                QV = "002"
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task KerkoAsync_WithValidParameters_ShouldReturnCorrectResults()
    {
        // Arrange
        var emri = "John";
        var mbiemri = "Doe";

        // Act
        var result = await _searchService.KerkoAsync(mbiemri, emri);

        // Assert
        result.Should().NotBeNull();
        result.Person.Should().NotBeEmpty();
        result.Person.Should().Contain(p => p.Emri == emri && p.Mbiemri == mbiemri);

        result.Rrogat.Should().NotBeEmpty();
        result.Rrogat.Should().Contain(r => r.Emri == emri && r.Mbiemri == mbiemri);

        result.Targat.Should().NotBeEmpty();
        result.Targat.Should().Contain(t => t.Emri == emri && t.Mbiemri == mbiemri);

        result.Patronazhist.Should().NotBeEmpty();
        result.Patronazhist.Should().Contain(p => p.Emri == emri && p.Mbiemri == mbiemri);
    }

    [Fact]
    public async Task KerkoAsync_WithNullParameters_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.KerkoAsync(null, "John"));
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.KerkoAsync("Doe", null));
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.KerkoAsync("", "John"));
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.KerkoAsync("Doe", ""));
    }

    [Fact]
    public async Task KerkoAsync_WithShortParameters_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.KerkoAsync("D", "J"));
    }

    [Fact]
    public async Task KerkoAsync_WithCaseInsensitiveSearch_ShouldReturnResults()
    {
        // Arrange
        var emri = "john"; // lowercase
        var mbiemri = "DOE"; // uppercase

        // Act
        var result = await _searchService.KerkoAsync(mbiemri, emri);

        // Assert
        result.Should().NotBeNull();
        result.Person.Should().NotBeEmpty();
        result.Person.Should().Contain(p =>
            p.Emri!.Equals("John", StringComparison.OrdinalIgnoreCase) &&
            p.Mbiemri!.Equals("Doe", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task TargatAsync_WithValidLicensePlate_ShouldReturnResults()
    {
        // Arrange
        var numriTarges = "AA123BB";

        // Act
        var result = await _searchService.TargatAsync(numriTarges);

        // Assert
        result.Should().NotBeNull();
        result.Targat.Should().NotBeEmpty();
        result.Targat.Should().Contain(t => t.NumriTarges == numriTarges);
    }

    [Fact]
    public async Task TargatAsync_WithPartialLicensePlate_ShouldReturnMatchingResults()
    {
        // Arrange
        var partialTarges = "AA123B"; // 6+ characters

        // Act
        var result = await _searchService.TargatAsync(partialTarges);

        // Assert
        result.Should().NotBeNull();
        result.Targat.Should().NotBeEmpty();
        result.Targat.Should().Contain(t => t.NumriTarges!.Contains(partialTarges));
    }

    [Fact]
    public async Task TargatAsync_WithNullOrEmptyParameter_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.TargatAsync(null));
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.TargatAsync(""));
    }

    [Fact]
    public async Task TargatAsync_WithShortParameter_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.TargatAsync("12345"));
    }

    [Fact]
    public async Task TelefonAsync_WithValidPhoneNumber_ShouldReturnResults()
    {
        // Arrange
        var telefon = "+355691234567";

        // Act
        var result = await _searchService.TelefonAsync(telefon);

        // Assert
        result.Should().NotBeNull();
        result.Patronazhist.Should().NotBeEmpty();
        result.Patronazhist.Should().Contain(p => p.Tel == telefon);
    }

    [Fact]
    public async Task TelefonAsync_WithPartialPhoneNumber_ShouldReturnMatchingResults()
    {
        // Arrange
        var partialTelefon = "3556912345"; // 10+ digits

        // Act
        var result = await _searchService.TelefonAsync(partialTelefon);

        // Assert
        result.Should().NotBeNull();
        result.Patronazhist.Should().NotBeEmpty();
        result.Patronazhist.Should().Contain(p => p.Tel!.Contains(partialTelefon));
    }

    [Fact]
    public async Task TelefonAsync_WithNullOrEmptyParameter_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.TelefonAsync(null));
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.TelefonAsync(""));
    }

    [Fact]
    public async Task TelefonAsync_WithShortParameter_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.TelefonAsync("123456789"));
    }

    [Fact]
    public async Task GetSearchLogsAsync_WithoutDateFilter_ShouldReturnAllLogs()
    {
        // Arrange
        _context.SearchLogs.Add(new SearchLog
        {
            IpAddress = "127.0.0.1",
            SearchType = "kerko",
            SearchParams = "test",
            Timestamp = DateTime.UtcNow,
            WasSuccessful = true
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _searchService.GetSearchLogsAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(log => log.IpAddress == "127.0.0.1");
    }

    [Fact]
    public async Task GetSearchLogsAsync_WithDateFilter_ShouldReturnFilteredLogs()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddDays(-10);
        var newDate = DateTime.UtcNow;

        _context.SearchLogs.AddRange(
            new SearchLog
            {
                IpAddress = "127.0.0.1",
                SearchType = "kerko",
                SearchParams = "old",
                Timestamp = oldDate,
                WasSuccessful = true
            },
            new SearchLog
            {
                IpAddress = "127.0.0.2",
                SearchType = "kerko",
                SearchParams = "new",
                Timestamp = newDate,
                WasSuccessful = true
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _searchService.GetSearchLogsAsync(DateTime.UtcNow.AddDays(-5));

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(log => log.SearchParams == "new");
        result.Should().NotContain(log => log.SearchParams == "old");
    }

    [Fact]
    public async Task DbStatusAsync_ShouldReturnCorrectCounts()
    {
        // Act
        var result = await _searchService.DbStatusAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(5);

        result.Should().Contain(dict => dict.ContainsKey("Person") && dict["Person"] >= 0);
        result.Should().Contain(dict => dict.ContainsKey("Rrogat") && dict["Rrogat"] >= 0);
        result.Should().Contain(dict => dict.ContainsKey("Targat") && dict["Targat"] >= 0);
        result.Should().Contain(dict => dict.ContainsKey("Patronazhist") && dict["Patronazhist"] >= 0);
        result.Should().Contain(dict => dict.ContainsKey("SearchLogs") && dict["SearchLogs"] >= 0);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}