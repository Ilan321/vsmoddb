using ilandev.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VsModDb.Data;
using VsModDb.Data.Entities;
using VsModDb.Data.Repositories;
using VsModDb.Extensions;
using VsModDb.Models.Options;
using VsModDb.Services.Mods;
using VsModDb.Services.Storage;
using VsModDb.Services.Storage.Providers;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.ConfigureAppOptions<DiskStorageProviderOptions>(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddControllers()
    .AddJsonOptions(f => f.JsonSerializerOptions.ConfigureDefaults());

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => c.EnableTryItOutByDefault());

app.MapControllers();

app.Run();