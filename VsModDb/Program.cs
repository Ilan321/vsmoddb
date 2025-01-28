using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VsModDb.Data;
using VsModDb.Data.Entities;
using VsModDb.Extensions;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}               {Message:lj}{NewLine}{Exception}")
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContextPool<ModDbContext>(c => c.UseSqlServer(builder.Configuration.GetConnectionString("db")));

builder.Services.AddIdentity<User, IdentityRole>(c =>
    {
        c.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ModDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(c => c.ConfigureApiDefaults());

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => c.EnableTryItOutByDefault());

app.MapControllers();

app.Run();