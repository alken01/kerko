using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Kerko.Infrastructure;
using Kerko.Services;
using Kerko.Models;

namespace Kerko.Tests;

public class PaginationRankingTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<SearchService>> _loggerMock;
    private readonly SearchService _searchService;

    public PaginationRankingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<SearchService>>();
        _searchService = new SearchService(_context, _loggerMock.Object);

        SeedTestDataForRanking();
    }

    private void SeedTestDataForRanking()
    {
        // Add test data specifically for testing ranking and pagination
        _context.Person.AddRange(
            // Exact matches should come first
            new Person { Emer = "John", Mbiemer = "Smith", Adresa = "1 Main St", Qyteti = "City1" },
            new Person { Emer = "John", Mbiemer = "Smith", Adresa = "2 Main St", Qyteti = "City2" },

            // Starts with matches should come second
            new Person { Emer = "John", Mbiemer = "Smithson", Adresa = "3 Oak St", Qyteti = "City3" },
            new Person { Emer = "Johnny", Mbiemer = "Smith", Adresa = "4 Oak St", Qyteti = "City4" },

            // Contains matches should come last
            new Person { Emer = "Johnson", Mbiemer = "Goldsmith", Adresa = "5 Pine St", Qyteti = "City5" },
            new Person { Emer = "Johnathan", Mbiemer = "Smithfield", Adresa = "6 Pine St", Qyteti = "City6" },

            // Add more than 10 results to test pagination
            new Person { Emer = "John", Mbiemer = "Smithers", Adresa = "7 Elm St", Qyteti = "City7" },
            new Person { Emer = "John", Mbiemer = "Smithwick", Adresa = "8 Elm St", Qyteti = "City8" },
            new Person { Emer = "Johnson", Mbiemer = "Smith", Adresa = "9 Elm St", Qyteti = "City9" },
            new Person { Emer = "Johns", Mbiemer = "Smith", Adresa = "10 Elm St", Qyteti = "City10" },
            new Person { Emer = "Johnnie", Mbiemer = "Smith", Adresa = "11 Elm St", Qyteti = "City11" },
            new Person { Emer = "John", Mbiemer = "Smithson", Adresa = "12 Elm St", Qyteti = "City12" }
        );

        // Add targat data for license plate ranking tests
        _context.Targat.AddRange(
            // Exact match
            new Targat { NumriTarges = "ABC123", Marka = "BMW", Emri = "John", Mbiemri = "Doe" },

            // Starts with
            new Targat { NumriTarges = "ABC123XYZ", Marka = "Mercedes", Emri = "Jane", Mbiemri = "Smith" },
            new Targat { NumriTarges = "ABC12345", Marka = "Audi", Emri = "Bob", Mbiemri = "Johnson" },

            // Contains
            new Targat { NumriTarges = "XYZABC123", Marka = "Toyota", Emri = "Alice", Mbiemri = "Brown" },
            new Targat { NumriTarges = "DEF123ABC", Marka = "Honda", Emri = "Charlie", Mbiemri = "Wilson" }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task KerkoAsync_ShouldReturnMax10Results()
    {
        // Arrange - search for "John" and "Smith" which should match many records
        var emri = "John";
        var mbiemri = "Smith";

        // Act
        var result = await _searchService.KerkoAsync(mbiemri, emri);

        // Assert
        result.Should().NotBeNull();
        result.Person.Should().NotBeNull();
        result.Person!.Count.Should().BeLessOrEqualTo(10, "pagination should limit results to 10 per page");
    }

    [Fact]
    public async Task KerkoAsync_ShouldRankExactMatchesFirst()
    {
        // Arrange - search for exact match "John" and "Smith"
        var emri = "John";
        var mbiemri = "Smith";

        // Act
        var result = await _searchService.KerkoAsync(mbiemri, emri);

        // Assert
        result.Should().NotBeNull();
        result.Person.Should().NotBeEmpty();

        // First results should be exact matches
        var firstResult = result.Person!.First();
        firstResult.Emri.Should().Be("John");
        firstResult.Mbiemri.Should().Be("Smith");

        // Check that exact matches come before partial matches
        var exactMatches = result.Person!.Where(p => p.Emri == "John" && p.Mbiemri == "Smith").ToList();
        var partialMatches = result.Person!.Where(p => p.Emri != "John" || p.Mbiemri != "Smith").ToList();

        if (partialMatches.Any())
        {
            var lastExactMatchIndex = result.Person!.IndexOf(exactMatches.Last());
            var firstPartialMatchIndex = result.Person!.IndexOf(partialMatches.First());

            lastExactMatchIndex.Should().BeLessThan(firstPartialMatchIndex,
                "exact matches should appear before partial matches");
        }
    }

    [Fact]
    public async Task TargatAsync_ShouldRankExactMatchesFirst()
    {
        // Arrange
        var numriTarges = "ABC123";

        // Act
        var result = await _searchService.TargatAsync(numriTarges);

        // Assert
        result.Should().NotBeNull();
        result.Targat.Should().NotBeEmpty();

        // First result should be exact match
        var firstResult = result.Targat!.First();
        firstResult.NumriTarges.Should().Be("ABC123");

        // Check ranking order
        var resultList = result.Targat!.ToList();
        for (int i = 0; i < resultList.Count - 1; i++)
        {
            var current = resultList[i];
            var next = resultList[i + 1];

            // Exact matches should come before starts-with matches
            var currentIsExact = current.NumriTarges!.Equals(numriTarges, StringComparison.OrdinalIgnoreCase);
            var currentStartsWith = current.NumriTarges!.StartsWith(numriTarges, StringComparison.OrdinalIgnoreCase);
            var nextIsExact = next.NumriTarges!.Equals(numriTarges, StringComparison.OrdinalIgnoreCase);
            var nextStartsWith = next.NumriTarges!.StartsWith(numriTarges, StringComparison.OrdinalIgnoreCase);

            if (currentIsExact && !nextIsExact)
            {
                // This is correct - exact before non-exact
                continue;
            }
            else if (currentStartsWith && !nextStartsWith && !nextIsExact)
            {
                // This is correct - starts with before contains
                continue;
            }
            else if (!currentIsExact && nextIsExact)
            {
                Assert.Fail($"Exact match '{next.NumriTarges}' should come before non-exact match '{current.NumriTarges}'");
            }
        }
    }

    [Fact]
    public async Task TargatAsync_ShouldReturnMax10Results()
    {
        // Act - search broadly to potentially match many results
        var result = await _searchService.TargatAsync("ABC123");

        // Assert
        result.Should().NotBeNull();
        result.Targat.Should().NotBeNull();
        result.Targat!.Count.Should().BeLessOrEqualTo(10, "pagination should limit results to 10 per page");
    }

    [Fact]
    public async Task TelefonAsync_ShouldReturnMax10Results()
    {
        // Arrange - Add phone data
        _context.Patronazhist.AddRange(
            Enumerable.Range(1, 15).Select(i => new Patronazhist
            {
                NumriPersonal = $"K{i:D8}A",
                Emri = $"Person{i}",
                Mbiemri = $"Surname{i}",
                Tel = $"+35569123456{i:D2}",
                QV = $"{i:D3}"
            })
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _searchService.TelefonAsync("+355691234567");

        // Assert
        result.Should().NotBeNull();
        result.Patronazhist.Should().NotBeNull();
        result.Patronazhist!.Count.Should().BeLessOrEqualTo(10, "pagination should limit results to 10 per page");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}