using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Kerko.Infrastructure;
using Kerko.Models;

namespace Kerko.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            // Add an in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });

            // Build the service provider and create the database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Database.EnsureCreated();
            SeedTestData(context);
        });
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Clear existing data
        context.Person.RemoveRange(context.Person);
        context.Rrogat.RemoveRange(context.Rrogat);
        context.Targat.RemoveRange(context.Targat);
        context.Patronazhist.RemoveRange(context.Patronazhist);

        // Add test persons
        context.Person.AddRange(
            new Person
            {
                Emer = "John",
                Mbiemer = "Doe",
                Adresa = "123 Main St",
                NrBaneses = "001",
                Atesi = "Father Name",
                Amesi = "Mother Name",
                Datelindja = "1990-01-01",
                Vendlindja = "Tirana",
                Seksi = "M",
                LidhjaMeKryefamiljarin = "Head",
                Qyteti = "Tirana",
                GjendjeCivile = "Single",
                Kombesia = "Albanian"
            },
            new Person
            {
                Emer = "Jane",
                Mbiemer = "Smith",
                Adresa = "456 Oak Ave",
                NrBaneses = "002",
                Atesi = "Father2",
                Amesi = "Mother2",
                Datelindja = "1985-05-15",
                Vendlindja = "Durres",
                Seksi = "F",
                LidhjaMeKryefamiljarin = "Head",
                Qyteti = "Durres",
                GjendjeCivile = "Married",
                Kombesia = "Albanian"
            },
            new Person
            {
                Emer = "Alice",
                Mbiemer = "Johnson",
                Adresa = "789 Pine Rd",
                NrBaneses = "003",
                Atesi = "Father3",
                Amesi = "Mother3",
                Datelindja = "1992-12-30",
                Vendlindja = "Vlore",
                Seksi = "F",
                LidhjaMeKryefamiljarin = "Wife",
                Qyteti = "Vlore",
                GjendjeCivile = "Married",
                Kombesia = "Albanian"
            },
            // Add more test data for better ranking tests
            new Person
            {
                Emer = "John",
                Mbiemer = "Johnson", // Exact match should rank higher
                Adresa = "100 Test St",
                NrBaneses = "004",
                Atesi = "Father4",
                Amesi = "Mother4",
                Datelindja = "1988-03-15",
                Vendlindja = "Shkoder",
                Seksi = "M",
                LidhjaMeKryefamiljarin = "Head",
                Qyteti = "Shkoder",
                GjendjeCivile = "Single",
                Kombesia = "Albanian"
            }
        );

        // Add test rrogat
        context.Rrogat.AddRange(
            new Rrogat
            {
                NumriPersonal = "K12345678A",
                Emri = "John",
                Mbiemri = "Doe",
                NIPT = "K123456789",
                DRT = "001",
                PagaBruto = 50000,
                Profesioni = "Engineer",
                Kategoria = "Technical"
            },
            new Rrogat
            {
                NumriPersonal = "K87654321B",
                Emri = "Jane",
                Mbiemri = "Smith",
                NIPT = "K987654321",
                DRT = "002",
                PagaBruto = 60000,
                Profesioni = "Manager",
                Kategoria = "Management"
            }
        );

        // Add test targat
        context.Targat.AddRange(
            new Targat
            {
                NumriTarges = "AA123BB",
                Marka = "BMW",
                Modeli = "X5",
                Ngjyra = "Blue",
                NumriPersonal = "K12345678A",
                Emri = "John",
                Mbiemri = "Doe"
            },
            new Targat
            {
                NumriTarges = "CC456DD",
                Marka = "Mercedes",
                Modeli = "E-Class",
                Ngjyra = "Black",
                NumriPersonal = "K87654321B",
                Emri = "Jane",
                Mbiemri = "Smith"
            }
        );

        // Add test patronazhist
        context.Patronazhist.AddRange(
            new Patronazhist
            {
                NumriPersonal = "K12345678A",
                Emri = "John",
                Mbiemri = "Doe",
                Atesi = "Father Name",
                Datelindja = "1990-01-01",
                QV = "001",
                ListaNr = "L001",
                Tel = "+355691234567",
                Emigrant = "No",
                Country = "Albania",
                ISigurte = "Yes",
                Koment = "Test comment",
                Patronazhisti = "Test patronazh",
                Preferenca = "None",
                Census2013Preferenca = "None",
                Census2013Siguria = "High",
                Vendlindja = "Tirana",
                Kompania = "Test Company",
                KodBanese = "TIR001"
            },
            new Patronazhist
            {
                NumriPersonal = "K87654321B",
                Emri = "Jane",
                Mbiemri = "Smith",
                Atesi = "Father2",
                Datelindja = "1985-05-15",
                QV = "002",
                ListaNr = "L002",
                Tel = "+355697654321",
                Emigrant = "No",
                Country = "Albania",
                ISigurte = "Yes",
                Koment = "Another test",
                Patronazhisti = "Test patronazh 2",
                Preferenca = "None",
                Census2013Preferenca = "None",
                Census2013Siguria = "Medium",
                Vendlindja = "Durres",
                Kompania = "Another Company",
                KodBanese = "DUR001"
            }
        );

        context.SaveChanges();
    }
}

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected ApplicationDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}