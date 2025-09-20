using FeatureFlags.Client;
using FeatureFlags.Controllers;
using FeatureFlags.Extensions;
using FeatureFlags.Extensions.Program;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Serilog;
using Serilog.Events;

// minimal logger config while host is being built
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// now add proper logging configuration
builder.ConfigureLogging();

builder.Services
    .Configure<IISServerOptions>(options => options.AllowSynchronousIO = true)
    .AddCustomServices()
    .AddCustomHealthChecks()
    .AddSession()
    .AddCookieAuthentication()
    .AddAntiforgery()
    .AddSingleton<IAssemblyService, AssemblyService>()
    .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
    .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
    .AddLocalization(x => x.ResourcesPath = "Resources")
    .AddCustomResponseCompression()
    .AddSwagger()
    .AddFeatureFlags()
    .AddMvc(options => options.Filters.Add(new RequireHttpsAttribute()))
    .AddRazorRuntimeCompilation()
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

var app = builder.Build();

app
    .UseHealthChecks()
    .UseHttpsRedirection()
    .UseSecurityHeaders()
    .UseStatusCodePagesWithReExecute($"/{nameof(ErrorController).StripController()}", "?code={0}")
    .UseExceptionHandling(builder)
    .UseResponseCompression()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseMiddleware<LogEnricherMiddleware>()
    .UseSession()
    .UseLocalization()
    .UseMiddleware<VersionCheckMiddleware>();

app.MapControllerRoute("parentChild", "{controller}/{action}/{parentId:int}/{id:int}");
app.MapControllerRoute("default", $"{{controller={nameof(DashboardController).StripController()}}}/{{action={nameof(DashboardController.Index)}}}/{{id:int?}}");

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();
