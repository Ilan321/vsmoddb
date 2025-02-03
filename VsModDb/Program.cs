using Hangfire;
using ilandev.AspNetCore.ExceptionHandler;
using ilandev.AspNetCore.ExceptionHandler.Models;
using ilandev.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using VsModDb.Data;
using VsModDb.Data.Entities;
using VsModDb.Data.Repositories;
using VsModDb.Extensions;
using VsModDb.Models.Exceptions;
using VsModDb.Models.Options;
using VsModDb.Services.Jobs;
using VsModDb.Services.LegacyApi;
using VsModDb.Services.Mods;
using VsModDb.Services.Storage.Providers;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["ASPNETCORE_SERVER"] is { } serverName)
{
    builder.Configuration.AddJsonFile($"appsettings.{serverName}.json", optional: true);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}               {Message:lj}{NewLine}{Exception}")
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContextPool<ModDbContext>(c => c.UseSqlServer(builder.Configuration.GetConnectionString("db")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ModDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(c => c.ConfigureApiDefaults());

builder.Services
    .AddScoped<IModService, ModService>()
    .AddScoped<IStorageProvider, DiskStorageProvider>();

builder.Services
    .AddScoped<IModRepository, ModRepository>();

builder.Services
    .ConfigureAppOptions<DiskStorageProviderOptions>(builder.Configuration)
    .ConfigureAppOptions<LegacyClientOptions>(builder.Configuration);

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient();

builder.Services.AddHttpClient<ILegacyApiClient, LegacyApiClient>((sp, client) =>
{
    var legacyClientOptions = sp.GetRequiredService<IOptions<LegacyClientOptions>>();

    client.BaseAddress = new Uri(legacyClientOptions.Value.BaseAddress);
});

builder.Services.AddControllers()
    .AddJsonOptions(f => f.JsonSerializerOptions.ConfigureDefaults());

builder.Services.Configure<JsonOptions>(c => c.SerializerOptions.ConfigureDefaults());

builder.Services.AddExceptionHandlingMiddleware(c => c.JsonSerializerOptions(j => j.ConfigureDefaults())
    .AllowExceptionInheritance()
    .IgnoreTaskCancellation()
    .LogExceptionDetails()
    .AddHandler<StatusCodeException>(e => new ExceptionMapping
    {
        StatusCode = e.StatusCode,
        Response = e.ErrorCode != null
            ? new
            {
                e.ErrorCode
            }
            : null
    }));

builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(c => c
    .UseInMemoryStorage()
    .UseRecommendedSerializerSettings()
);

builder.Services.AddHangfireServer();

builder.Services.AddDistributedMemoryCache();

if (builder.Configuration.GetConnectionString("redis") is { } redisConnectionString)
{
    builder.Services.AddStackExchangeRedisCache(c => c.Configuration = redisConnectionString);
}

var app = builder.Build();

var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard();
}

app.UseAuthorization();

app.UseExceptionHandlingMiddleware();

app.UseSwagger();
app.UseSwaggerUI(c => c.EnableTryItOutByDefault());

app.MapControllers();

if (app.Configuration.GetAppOptions<LegacyClientOptions>()?.EnablePeriodicModFetch == true)
{
    // Add the HydrateModDetailsJob to the recurring job manager, run it every 5 minutes

    jobManager.AddOrUpdate<HydrateModDetailsJob>(HydrateModDetailsJob.JobId, job => job.ExecuteAsync(app.Lifetime.ApplicationStopping), "*/5 * * * *");

    jobManager.Trigger(HydrateModDetailsJob.JobId);
}

app.Run();