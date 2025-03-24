using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Kerko.Infrastructure;
using Kerko.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(); // Add CORS middleware
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", async (ApplicationDbContext db) =>
{
    try
    {
        await db.Database.OpenConnectionAsync();
        await db.Database.CloseConnectionAsync();
        return Results.Ok(new
        {
            status = "healthy",
            database = "connected",
            timestamp = DateTime.UtcNow,
            statistics = new
            {
                persons = await db.Person.CountAsync(),
                rrogat = await db.Rrogat.CountAsync(),
                targat = await db.Targat.CountAsync(),
                patronazhist = await db.Patronazhist.CountAsync()
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Database Connection Error",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
});

app.MapGet("/api/kerko", async (ApplicationDbContext db, [FromQuery] string? mbiemri, [FromQuery] string? emri) =>
{
    if (string.IsNullOrEmpty(mbiemri) || string.IsNullOrEmpty(emri))
    {
        return Results.BadRequest("Emri dhe mbiemri nuk mund te jene bosh");
    }
    
    var personResults = await db.Person
        .Where(p => p.Mbiemer != null && p.Mbiemer.ToLower().Contains(mbiemri.ToLower()) &&
                    p.Emer != null && p.Emer.ToLower().Contains(emri.ToLower()))
        .Select(p => new PersonResponse
        {
            Adresa = p.Adresa,
            NrBaneses = p.NrBaneses,
            Emri = p.Emer,
            Mbiemri = p.Mbiemer,
            Atesi = p.Atesi,
            Amesi = p.Amesi,
            Datelindja = p.Datelindja,
            Vendlindja = p.Vendlindja,
            Seksi = p.Seksi,
            LidhjaMeKryefamiljarin = p.LidhjaMeKryefamiljarin,
            Qyteti = p.Qyteti,
            GjendjeCivile = p.GjendjeCivile,
            Kombesia = p.Kombesia
        })
        .ToListAsync();

    var rrogatResults = await db.Rrogat
        .Where(p => p.Mbiemri != null && p.Mbiemri.ToLower().Contains(mbiemri.ToLower()) &&
                    p.Emri != null && p.Emri.ToLower().Contains(emri.ToLower()))
        .Select(r => new RrogatResponse
        {
            NumriPersonal = r.NumriPersonal,
            Emri = r.Emri,
            Mbiemri = r.Mbiemri,
            NIPT = r.NIPT,
            DRT = r.DRT,
            PagaBruto = r.PagaBruto,
            Profesioni = r.Profesioni,
            Kategoria = r.Kategoria
        })
        .ToListAsync();

    var targatResults = await db.Targat
        .Where(p => p.Mbiemri != null && p.Mbiemri.ToLower().Contains(mbiemri.ToLower()) &&
                    p.Emri != null && p.Emri.ToLower().Contains(emri.ToLower()))
        .Select(t => new TargatResponse
        {
            NumriTarges = t.NumriTarges,
            Marka = t.Marka,
            Modeli = t.Modeli,
            Ngjyra = t.Ngjyra,
            NumriPersonal = t.NumriPersonal,
            Emri = t.Emri,
            Mbiemri = t.Mbiemri
        })
        .ToListAsync();

    var patronazhistResults = await db.Patronazhist
        .Where(p => p.Mbiemri != null && p.Mbiemri.ToLower().Contains(mbiemri.ToLower()) &&
                    p.Emri != null && p.Emri.ToLower().Contains(emri.ToLower()))
        .Select(p => new PatronazhistResponse
        {
            NumriPersonal = p.NumriPersonal,
            Emri = p.Emri,
            Mbiemri = p.Mbiemri,
            Atesi = p.Atesi,
            Datelindja = p.Datelindja,
            QV = p.QV,
            ListaNr = p.ListaNr,
            Tel = p.Tel,
            Emigrant = p.Emigrant,
            Country = p.Country,
            ISigurte = p.ISigurte,
            Koment = p.Koment,
            Patronazhisti = p.Patronazhisti,
            Preferenca = p.Preferenca,
            Census2013Preferenca = p.Census2013Preferenca,
            Census2013Siguria = p.Census2013Siguria,
            Vendlindja = p.Vendlindja,
            Kompania = p.Kompania,
            KodBanese = p.KodBanese
        })
        .ToListAsync();

    var response = new SearchResponse()
    {
        Person = personResults,
        Rrogat = rrogatResults,
        Targat = targatResults,
        Patronazhist = patronazhistResults
    };
    return Results.Ok(response);
});

// Create database and tables
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();