using System.Net;
using System.Text.Json;
using FluentAssertions;
using Kerko.Models;

namespace Kerko.Tests.Integration;

public class SearchControllerTests : IntegrationTestBase
{
    public SearchControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task KerkoAsync_WithValidNameAndSurname_ShouldReturnResults()
    {
        // Arrange
        var emri = "John";
        var mbiemri = "Doe";

        // Act
        var response = await Client.GetAsync($"/api/kerko?emri={emri}&mbiemri={mbiemri}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Person.Should().NotBeEmpty();
        searchResponse.Person!.Should().HaveCountGreaterThan(0);

        // Check that results contain the searched name
        searchResponse.Person.Should().Contain(p =>
            p.Emri!.Contains(emri, StringComparison.OrdinalIgnoreCase) &&
            p.Mbiemri!.Contains(mbiemri, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task KerkoAsync_WithPartialMatch_ShouldReturnMatchingResults()
    {
        // Arrange
        var emri = "Jo"; // Partial match for "John"
        var mbiemri = "Do"; // Partial match for "Doe"

        // Act
        var response = await Client.GetAsync($"/api/kerko?emri={emri}&mbiemri={mbiemri}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Person.Should().NotBeEmpty();

        // Check that results contain the searched partial name
        searchResponse.Person.Should().Contain(p =>
            p.Emri!.Contains(emri, StringComparison.OrdinalIgnoreCase) &&
            p.Mbiemri!.Contains(mbiemri, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task KerkoAsync_WithEmptyName_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("/api/kerko?emri=&mbiemri=Doe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task KerkoAsync_WithShortName_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("/api/kerko?emri=J&mbiemri=D");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task KerkoAsync_ShouldReturnResultsFromAllTables()
    {
        // Arrange
        var emri = "John";
        var mbiemri = "Doe";

        // Act
        var response = await Client.GetAsync($"/api/kerko?emri={emri}&mbiemri={mbiemri}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();

        // Should have results from Person table
        searchResponse!.Person.Should().NotBeEmpty();
        searchResponse.Person!.Should().Contain(p =>
            p.Emri == emri && p.Mbiemri == mbiemri);

        // Should have results from Rrogat table
        searchResponse.Rrogat.Should().NotBeEmpty();
        searchResponse.Rrogat!.Should().Contain(r =>
            r.Emri == emri && r.Mbiemri == mbiemri);

        // Should have results from Targat table
        searchResponse.Targat.Should().NotBeEmpty();
        searchResponse.Targat!.Should().Contain(t =>
            t.Emri == emri && t.Mbiemri == mbiemri);

        // Should have results from Patronazhist table
        searchResponse.Patronazhist.Should().NotBeEmpty();
        searchResponse.Patronazhist!.Should().Contain(p =>
            p.Emri == emri && p.Mbiemri == mbiemri);
    }

    [Fact]
    public async Task TargatAsync_WithValidLicensePlate_ShouldReturnResults()
    {
        // Arrange
        var numriTarges = "AA123BB";

        // Act
        var response = await Client.GetAsync($"/api/targat?numriTarges={numriTarges}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Targat.Should().NotBeEmpty();
        searchResponse.Targat!.Should().Contain(t => t.NumriTarges == numriTarges);
    }

    [Fact]
    public async Task TargatAsync_WithPartialLicensePlate_ShouldReturnMatchingResults()
    {
        // Arrange
        var partialNumriTarges = "AA123B"; // Partial match with 6+ characters

        // Act
        var response = await Client.GetAsync($"/api/targat?numriTarges={partialNumriTarges}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Targat.Should().NotBeEmpty();
        searchResponse.Targat!.Should().Contain(t =>
            t.NumriTarges!.Contains(partialNumriTarges, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task TargatAsync_WithShortLicensePlate_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("/api/targat?numriTarges=12345");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TelefonAsync_WithValidPhoneNumber_ShouldReturnResults()
    {
        // Arrange
        var numriTelefonit = "+355691234567";

        // Act
        var response = await Client.GetAsync($"/api/telefon?numriTelefonit={Uri.EscapeDataString(numriTelefonit)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Patronazhist.Should().NotBeEmpty();
        searchResponse.Patronazhist!.Should().Contain(p => p.Tel == numriTelefonit);
    }

    [Fact]
    public async Task TelefonAsync_WithPartialPhoneNumber_ShouldReturnMatchingResults()
    {
        // Arrange
        var partialTelefon = "3556912345"; // Partial match with 10+ digits

        // Act
        var response = await Client.GetAsync($"/api/telefon?numriTelefonit={partialTelefon}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Patronazhist.Should().NotBeEmpty();
        searchResponse.Patronazhist!.Should().Contain(p =>
            p.Tel!.Contains(partialTelefon));
    }

    [Fact]
    public async Task TelefonAsync_WithShortPhoneNumber_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("/api/telefon?numriTelefonit=123456789");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOK()
    {
        // Act
        var response = await Client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("OK");
    }

    [Theory]
    [InlineData("")]
    [InlineData("J")]
    [InlineData("a")]
    public async Task KerkoAsync_WithInvalidInput_ShouldReturnBadRequest(string invalidInput)
    {
        // Act
        var response = await Client.GetAsync($"/api/kerko?emri={invalidInput}&mbiemri={invalidInput}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("A1234")]
    public async Task TargatAsync_WithInvalidInput_ShouldReturnBadRequest(string invalidInput)
    {
        // Act
        var response = await Client.GetAsync($"/api/targat?numriTarges={invalidInput}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456789")]
    public async Task TelefonAsync_WithInvalidInput_ShouldReturnBadRequest(string invalidInput)
    {
        // Act
        var response = await Client.GetAsync($"/api/telefon?numriTelefonit={invalidInput}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}