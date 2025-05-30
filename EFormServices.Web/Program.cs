// EFormServices.Web/Program.cs
// Got code 30/05/2025
using EFormServices.Application;
using EFormServices.Infrastructure;
using EFormServices.Infrastructure.Data;
using EFormServices.Web.Extensions;
using EFormServices.Web.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapFallbackToPage("/Index");

using (var scope = app.Services.CreateScope())
{
    await ApplicationDbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();