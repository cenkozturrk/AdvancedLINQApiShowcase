using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Middleware;
using AdvancedLINQApiShowcase.Services;
using AdvancedLINQApiShowcase.Validation;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


// Configure Serilog conf
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Read config from appsettings.json
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(@"C:\Users\Ceku\source\repos\AdvancedLINQApiShowcase\AdvancedLINQApiShowcase\Serilog\log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();




// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Lifecycle
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();


builder.Services.AddControllers();

// Fluent Validation conf
// Register FluentValidation services
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Register Validators
builder.Services.AddValidatorsFromAssemblyContaining<OrderValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapControllers();
    app.MapScalarApiReference();
}
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapFallback(() => Results.Redirect("/swagger"));

app.MapControllers();
try
{
    Log.Information("Starting the application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


app.Run();
