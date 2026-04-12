using Kerko.Infrastructure;
using Kerko.Models;
using Kerko.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Kerko.Tests;

[TestFixture]
public class SearchServiceTests
{
    private ApplicationDbContext _db = null!;
    private SearchService _service = null!;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _db = new ApplicationDbContext(options);
        await _db.Database.OpenConnectionAsync();
        await _db.Database.EnsureCreatedAsync();

        SeedData();
        _service = new SearchService(_db, NullLogger<SearchService>.Instance);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _db.Database.CloseConnectionAsync();
        await _db.DisposeAsync();
    }

    private void SeedData()
    {
        _db.Person.AddRange(
            MakePerson(1, "Andi", "Kuçi", "Tiranë"),
            MakePerson(2, "Ëngjëll", "Çela", "Elbasan"),
            MakePerson(3, "Erion", "Hoxha", "Durrës"),
            MakePerson(4, "Çlirim", "Bërdica", "Shkodër"),
            MakePerson(5, "Elena", "Koci", "Vlorë"),
            MakePerson(6, "Ece", "Ece", "Korçë"),
            MakePerson(7, "Alken", "Rrokaj", "Tiranë"));

        _db.Rrogat.AddRange(
            new Rrogat { Id = 1, Emri = "Andi", Mbiemri = "Kuçi", NumriPersonal = "A1234", PagaBruto = 50000 },
            new Rrogat { Id = 2, Emri = "Ëngjëll", Mbiemri = "Çela", NumriPersonal = "B5678", PagaBruto = 60000 });

        _db.Targat.AddRange(
            new Targat { Id = 1, Emri = "Andi", Mbiemri = "Kuçi", NumriTarges = "AA123BB" },
            new Targat { Id = 2, Emri = "Erion", Mbiemri = "Hoxha", NumriTarges = "TR456CD" });

        _db.Patronazhist.AddRange(
            new Patronazhist { Id = 1, Emri = "Andi", Mbiemri = "Kuçi", Tel = "0691234567" },
            new Patronazhist { Id = 2, Emri = "Ëngjëll", Mbiemri = "Çela", Tel = "0697654321" });

        _db.SaveChanges();
    }

    private static Person MakePerson(int id, string emer, string mbiemer, string qyteti) =>
        new() { Id = id, Emer = emer, Mbiemer = mbiemer, Qyteti = qyteti };

    // ─── Diacritic normalization ─────────────────────────────────────

    [TestCase("kuci", "andi", "Kuçi", Description = "ASCII input finds ç")]
    [TestCase("kuçi", "andi", "Kuçi", Description = "ç input finds ç")]
    [TestCase("koci", "elena", "Koci", Description = "ASCII stored name")]
    [TestCase("cela", "engjell", "Çela", Description = "ASCII finds Ç and Ë")]
    [TestCase("berdica", "clirim", "Bërdica", Description = "ASCII finds ë")]
    [TestCase("KUCI", "ANDI", "Kuçi", Description = "Uppercase ASCII finds ç")]
    [TestCase("ÇELA", "ËNGJËLL", "Çela", Description = "Uppercase diacritics")]
    [TestCase("Kuçi", "Andi", "Kuçi", Description = "Mixed case diacritics")]
    [TestCase("ece", "ece", "Ece", Description = "Repeated diacritic chars")]
    public async Task Search_DiacriticNormalization(string mbiemri, string emri, string expectedMbiemri)
    {
        var result = await _service.KerkoAsync(mbiemri, emri);
        Assert.That(result.Person.Items, Has.Some.Matches<PersonResponse>(p => p.Mbiemri == expectedMbiemri));
    }

    // ─── Variant explosion regression (the ececece crash) ────────────

    [TestCase("ececece", "ececece", Description = "3^7 = 2187 variants in old code")]
    [TestCase("ecececececececececece", "ecececececececececece", Description = "3^20 in old code")]
    [TestCase("cccccccccc", "eeeeeeeeee", Description = "All substitutable chars")]
    public async Task Search_ManyDiacriticChars_DoesNotExplode(string mbiemri, string emri)
    {
        var result = await _service.KerkoAsync(mbiemri, emri);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Person, Is.Not.Null);
    }

    // ─── Prefix matching on the normalized indexed column ────────────
    // The search is a B-tree range scan on (MbiemriNormalized, EmriNormalized),
    // so every prefix of the folded, lowercased name should hit the row.

    [TestCase("ku", "an", "Kuçi", Description = "2-char ASCII prefix folds ç")]
    [TestCase("kuç", "and", "Kuçi", Description = "Prefix containing ç still folds")]
    [TestCase("ho", "er", "Hoxha", Description = "Plain ASCII prefix")]
    [TestCase("be", "cl", "Bërdica", Description = "ASCII prefix matches ë-folded stored name")]
    [TestCase("bër", "çli", "Bërdica", Description = "Diacritic prefix matches diacritic stored name")]
    [TestCase("KU", "AN", "Kuçi", Description = "Uppercase prefix is lowercased before the range query")]
    [TestCase("ÇE", "ËNG", "Çela", Description = "Uppercase diacritic prefix")]
    public async Task Search_PrefixMatching(string mbiemriPrefix, string emriPrefix, string expectedMbiemri)
    {
        var result = await _service.KerkoAsync(mbiemriPrefix, emriPrefix);

        Assert.That(result.Person.Items,
            Has.Some.Matches<PersonResponse>(p => p.Mbiemri == expectedMbiemri));
    }

    // ─── Starts-with semantics (documents the change from contains) ──
    // Switching to a sargable range query means only prefix matches are
    // returned. A mid-string substring that used to match under LIKE
    // '%foo%' must no longer match.

    [TestCase("oxha", "erion", Description = "'oxha' is a suffix of 'Hoxha' but not a prefix")]
    [TestCase("uci", "andi", Description = "'uci' is inside 'Kuçi' but not a prefix")]
    [TestCase("rdica", "clirim", Description = "'rdica' is inside 'Bërdica' but not a prefix")]
    public async Task Search_NonPrefixSubstring_DoesNotMatch(string mbiemri, string emri)
    {
        var result = await _service.KerkoAsync(mbiemri, emri);

        Assert.That(result.Person.Items, Is.Empty);
    }

    // ─── Computed column populated by SQLite on insert ───────────────
    // The VIRTUAL generated column is what the index stores — if this
    // ever comes back null or unfolded, the whole prefix search breaks
    // and nothing else in the suite would catch it.

    [Test]
    public async Task Seed_NormalizedColumnsAreFoldedAndLowercased()
    {
        var row = await _db.Person.AsNoTracking().FirstAsync(p => p.Id == 1);
        Assert.That(row.MbiemerNormalized, Is.EqualTo("kuci"));
        Assert.That(row.EmerNormalized, Is.EqualTo("andi"));

        var celaRow = await _db.Person.AsNoTracking().FirstAsync(p => p.Id == 2);
        Assert.That(celaRow.MbiemerNormalized, Is.EqualTo("cela"));
        Assert.That(celaRow.EmerNormalized, Is.EqualTo("engjell"));
    }

    // ─── Cross-table search ──────────────────────────────────────────

    [Test]
    public async Task Search_FindsAcrossAllTables()
    {
        var result = await _service.KerkoAsync("kuci", "andi");

        Assert.That(result.Person.Items, Is.Not.Empty);
        Assert.That(result.Rrogat.Items, Is.Not.Empty);
        Assert.That(result.Targat.Items, Is.Not.Empty);
        Assert.That(result.Patronazhist.Items, Is.Not.Empty);
    }

    // ─── Relevance ordering ──────────────────────────────────────────

    [Test]
    public async Task Search_ExactMatchRankedFirst()
    {
        var result = await _service.KerkoAsync("hoxha", "erion");

        Assert.That(result.Person.Items, Is.Not.Empty);
        Assert.That(result.Person.Items[0].Mbiemri, Is.EqualTo("Hoxha"));
        Assert.That(result.Person.Items[0].Emri, Is.EqualTo("Erion"));
    }

    // ─── Pagination ──────────────────────────────────────────────────

    [TestCase(1, 1)]
    [TestCase(1, 5)]
    [TestCase(2, 1)]
    public async Task Search_PaginationReturnsCorrectInfo(int pageNumber, int pageSize)
    {
        var result = await _service.KerkoAsync("kuci", "andi", pageNumber, pageSize);

        Assert.That(result.Person.Pagination.CurrentPage, Is.EqualTo(pageNumber));
        Assert.That(result.Person.Pagination.PageSize, Is.EqualTo(pageSize));
    }

    // ─── Validation ──────────────────────────────────────────────────

    [TestCase("", "test", Description = "Empty mbiemri")]
    [TestCase("test", "", Description = "Empty emri")]
    [TestCase(null, "test", Description = "Null mbiemri")]
    [TestCase("test", null, Description = "Null emri")]
    public void Search_InvalidInput_ThrowsArgumentException(string? mbiemri, string? emri)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.KerkoAsync(mbiemri, emri));
    }

    [TestCase("a", "b", Description = "Both too short")]
    [TestCase("ab", "c", Description = "Emri too short")]
    [TestCase("a", "bc", Description = "Mbiemri too short")]
    public void Search_TooShortInput_ThrowsArgumentException(string mbiemri, string emri)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.KerkoAsync(mbiemri, emri));
    }

    // ─── Targa search ────────────────────────────────────────────────

    [TestCase("AA123BB", "Kuçi", Description = "Full plate")]
    [TestCase("AA123B", "Kuçi", Description = "Partial plate")]
    [TestCase("TR456CD", "Hoxha", Description = "Different plate")]
    public async Task Targat_FindsByPlateNumber(string plate, string expectedMbiemri)
    {
        var result = await _service.TargatAsync(plate);

        Assert.That(result.Items, Has.Some.Matches<TargatResponse>(t => t.Mbiemri == expectedMbiemri));
    }

    [TestCase("AB", Description = "Too short")]
    [TestCase("", Description = "Empty")]
    [TestCase(null, Description = "Null")]
    public void Targat_InvalidInput_ThrowsArgumentException(string? plate)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.TargatAsync(plate));
    }

    // ─── Telefon search ──────────────────────────────────────────────

    [TestCase("0691234567", "Kuçi", Description = "Exact phone")]
    [TestCase("0697654321", "Çela", Description = "Different phone")]
    public async Task Telefon_FindsByPhoneNumber(string phone, string expectedMbiemri)
    {
        var result = await _service.TelefonAsync(phone);

        Assert.That(result.Items, Has.Some.Matches<PatronazhistResponse>(p => p.Mbiemri == expectedMbiemri));
    }

    [TestCase("069", Description = "Too short")]
    [TestCase("", Description = "Empty")]
    [TestCase(null, Description = "Null")]
    public void Telefon_InvalidInput_ThrowsArgumentException(string? phone)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.TelefonAsync(phone));
    }

    // ─── Adversarial input sanitization ────────────────────────────

    [Test]
    public async Task Search_PercentInInput_StrippedAndNoMatch()
    {
        var result = await _service.KerkoAsync("%%", "%%");
        Assert.That(result.Person.Items, Is.Empty);
    }

    [Test]
    public async Task Search_UnderscoreInInput_StrippedAndNoMatch()
    {
        var result = await _service.KerkoAsync("_oxha", "erion");
        Assert.That(result.Person.Items, Is.Empty);
    }

    [Test]
    public async Task Search_BackslashInInput_DoesNotCrash()
    {
        var result = await _service.KerkoAsync("test\\", "test\\");
        Assert.That(result.Person.Items, Is.Empty);
    }

    // ─── Pagination boundary clamping ────────────────────────────────

    [Test]
    public async Task Search_PageZero_ClampedToOne()
    {
        var result = await _service.KerkoAsync("rrokaj", "alken", pageNumber: 0);
        Assert.That(result.Person.Pagination.CurrentPage, Is.EqualTo(1));
    }

    [Test]
    public async Task Search_NegativePage_ClampedToOne()
    {
        var result = await _service.KerkoAsync("rrokaj", "alken", pageNumber: -5);
        Assert.That(result.Person.Pagination.CurrentPage, Is.EqualTo(1));
    }

    [Test]
    public async Task Search_OversizedPageSize_ClampedToMax()
    {
        var result = await _service.KerkoAsync("rrokaj", "alken", pageSize: 500);
        Assert.That(result.Person.Pagination.PageSize, Is.EqualTo(100));
    }

    [Test]
    public async Task Search_ZeroPageSize_ClampedToOne()
    {
        var result = await _service.KerkoAsync("rrokaj", "alken", pageSize: 0);
        Assert.That(result.Person.Pagination.PageSize, Is.EqualTo(1));
    }

}
