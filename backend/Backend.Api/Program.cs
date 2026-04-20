using System.Text.Json.Serialization;
using Backend.Api.Data;
using Backend.Api.Services.Dishes;
using Backend.Api.Services.Products;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing connection string 'ConnectionStrings:Default'. " +
        "Set it in appsettings.json or via env var ConnectionStrings__Default.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDishService, DishService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend"); // Используем имя политики!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
