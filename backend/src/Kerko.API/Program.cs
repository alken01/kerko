using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SQLite
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "data.db");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Add minimal API endpoints
app.MapGet("/api/people", async (ApplicationDbContext db, string name, string lastName) =>
{
    var personList = await db.People
        .Include(p => p.City)
        .Include(p => p.MaritalStatus) 
        .Include(p => p.Nationality)
        .Include(p => p.Relationship)
        .Where(p => p.Name == name && p.LastName == lastName)
        .ToListAsync();

    if (personList.Count == 0)
    {
        return Results.NotFound();
    }

    return Results.Ok(personList);
});

app.Run();
