using Microsoft.EntityFrameworkCore;
using WebApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Lägg till Entity Framework - endast för development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    // Använd In-Memory database för GCP Cloud Run
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDb"));
}

var app = builder.Build();

// Automatisk migration när applikationen startar - endast för development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ett fel uppstod under databas-migrationen");
        }
    }
}
else
{
    // Seed data för In-Memory database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        
        // Lägg till lite test-data
        if (!context.Events.Any())
        {
            context.Events.AddRange(
                new WebApi.Models.Event 
                { 
                    Title = "Demo Event 1", 
                    Description = "Ett demo event", 
                    Start = DateTime.Now.AddDays(1), 
                    AllDay = false,
                    CreatedAt = DateTime.UtcNow 
                },
                new WebApi.Models.Event 
                { 
                    Title = "Demo Event 2", 
                    Description = "Ett annat demo event", 
                    Start = DateTime.Now.AddDays(2), 
                    AllDay = true,
                    CreatedAt = DateTime.UtcNow 
                }
            );
            context.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Lyssna på port från miljövariabel (Cloud Run kräver detta)
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
app.Urls.Add($"http://*:{port}");

app.Run();
