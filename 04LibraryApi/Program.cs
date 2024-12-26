using _04LibraryApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Set the SQL Server instance to use
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));  
});

builder.Services.AddTransient<DataSeed>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();



app.MapControllers();

// Seed the database
using (var serviceScope = app.Services.CreateScope())
{
    var seed = serviceScope.ServiceProvider.GetRequiredService<DataSeed>();
    await seed.InitSeedAsync();
}

app.Run();