using Cetus;
using Cetus.Api.Configuration;
using Cetus.Api.Configuration.Validators;
using Cetus.Api.Extensions;
using Cetus.Api.Realtime;
using FluentValidation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureDatabase();
builder.ConfigureCors();
builder.ConfigureAuthentication();
builder.ConfigureEmail();
builder.ConfigureRateLimit();
builder.ConfigureCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
    configuration.RegisterServicesFromAssembly(typeof(CetusAssemblyHelper).Assembly);
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CetusAssemblyHelper).Assembly);

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors(Cors.AllowAll);
app.UseMiddleware<RequestLogContextMiddleware>();
app.UseRateLimiter();
app.UseExceptionHandler();

app.MapControllers();

app.MapHub<OrdersHub>("/api/realtime/orders").RequireCors(Cors.AllowAll);

app.Run();

public partial class Program;
