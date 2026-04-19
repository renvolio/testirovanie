using System.Text.Json.Serialization;
using Backend.Api.Data;
using Backend.Api.Services.Dishes;
using Backend.Api.Services.Products;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var sqliteConnectionString = ResolveSqliteConnectionString(
    builder.Configuration.GetConnectionString("Default"),
    builder.Environment.ContentRootPath);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnectionString));

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
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();

static string ResolveSqliteConnectionString(string? configured, string contentRoot)
{
    var raw = string.IsNullOrWhiteSpace(configured)
        ? "Data Source=Data/products.db"
        : configured;

    var csb = new SqliteConnectionStringBuilder(raw);
    var dataSource = csb.DataSource;
    var fullPath = Path.IsPathRooted(dataSource)
        ? dataSource
        : Path.GetFullPath(Path.Combine(contentRoot, dataSource));

    var directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);

    csb.DataSource = fullPath;
    return csb.ConnectionString;
}
