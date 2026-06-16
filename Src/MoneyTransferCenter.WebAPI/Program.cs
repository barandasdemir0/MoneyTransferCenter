using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Infrastructure.Data;
using MoneyTransferCenter.WebAPI.Extension;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(configurations =>
{
    configurations.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
builder.Services.AddOpenApi();

builder.Services.AddScoped<ICurrentUserService, FakeCurrentService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World!");



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
app.Run();
