using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args); // 👉 önce builder'ı oluştur

// 👉 sonra connection string'i builder üzerinden al
var connectionString = builder.Configuration.GetConnectionString("Pizzas")
                      ?? "Data Source=Pizzas.db";

// EF Core için SQLite veritabanı servisini ekle
builder.Services.AddSqlite<PizzaDb>(connectionString);

// Swagger servisini ekle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1"
    });
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



var app = builder.Build();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
         c.RoutePrefix = string.Empty; // root URL'den Swagger açılır
    });
}

app.MapGet("/", () => "Hello World!");

// GET
app.MapGet("/pizzas", async (PizzaDb db) =>
    await db.Pizzas.ToListAsync());

// POST
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});

// GET by ID
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) =>
    await db.Pizzas.FindAsync(id) is Pizza pizza
        ? Results.Ok(pizza)
        : Results.NotFound());

// PUT
app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatepizza, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});


// Seed verisi: uygulama başlatıldığında örnek pizzalar eklenir(opsiyonel ekledim)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaDb>();

    if (!db.Pizzas.Any())
    {
        db.Pizzas.AddRange(
            new Pizza { Name = "Margherita", Description = "Domates, Mozarella" },
            new Pizza { Name = "Pepperoni", Description = "Sucuk, Mozarella" },
            new Pizza { Name = "Vegetarian", Description = "Sebzeler ve Mozarella" }
        );
        db.SaveChanges();
    }
}






app.Run();
