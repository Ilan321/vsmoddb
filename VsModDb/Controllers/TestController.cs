using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VsModDb.Data;
using VsModDb.Data.Entities;
using VsModDb.Data.Entities.Assets;
using VsModDb.Models.Assets;
using VsModDb.Services.Storage.Providers;

namespace VsModDb.Controllers;

#if DEBUG

[ApiController]
[Route("api/v1/test")]
public class TestController(
    HttpClient httpClient,
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    ModDbContext context,
    IStorageProvider storageProvider
) : ControllerBase
{
    [HttpGet]
    public string Ping() => "Pong!";

    [HttpPost("login")]
    public async Task<Results<Ok, BadRequest>> Login([FromQuery] string username)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user is null)
        {
            return TypedResults.BadRequest();
        }

        await signInManager.SignInAsync(new User
        {
            UserName = username
        }, isPersistent: true);

        return TypedResults.Ok();
    }

    [HttpPost("create-user")]
    public async Task CreateUser([FromBody] CreateTestUserRequest request)
    {
        var user = new User()
        {
            Email = request.Email,
            UserName = request.Username,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user);

        await signInManager.SignInAsync(user, true);
    }

    [HttpPost("fetch-mods")]
    public async Task FetchMods([FromQuery] int count, CancellationToken cancellationToken)
    {
        using var modsHttpResponse = await httpClient.GetAsync("https://mods.vintagestory.at/api/mods", cancellationToken);

        modsHttpResponse.EnsureSuccessStatusCode();

        var modsResponse = await modsHttpResponse.Content.ReadFromJsonAsync<FetchedModsResponse>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        }, cancellationToken: cancellationToken);

        var mods = modsResponse.Mods.OrderByDescending(f => f.ModId)
            .Take(count)
            .ToList();

        foreach (var fetchedMod in mods)
        {
            var mod = new Mod
            {
                Name = fetchedMod.Name,
                Summary = fetchedMod.Summary,
                TimeCreatedUtc = fetchedMod.LastReleasedDateTime,
                TimeUpdatedUtc = fetchedMod.LastReleasedDateTime,
                Tags = fetchedMod.Tags.Select(tag => new ModTag
                {
                    Value = tag
                }).ToList(),
                UrlAlias = fetchedMod.UrlAlias
            };

            context.Mods.Add(mod);

            await context.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(fetchedMod.Logo))
            {
                using var bannerResponse = await httpClient.GetAsync($"https://mods.vintagestory.at/{fetchedMod.Logo}", cancellationToken);

                await using var bannerStream = await bannerResponse.Content.ReadAsStreamAsync(cancellationToken);

                var assetStream = new AssetStream(bannerStream, "banner.png", bannerResponse.Content.Headers.ContentType?.MediaType ?? "image/png");

                var bannerAssetPath = $"mods/{mod.Id}/banner.png";

                await storageProvider.SaveFileAsync(bannerAssetPath, bannerStream, cancellationToken);

                mod.Banner = new Asset
                {
                    FileName = assetStream.FileName,
                    ContentType = assetStream.ContentType,
                    AssetPath = bannerAssetPath
                };

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

public class CreateTestUserRequest
{
    public required string Username { get; set; }
    public required string Email { get; set; }
}

public class FetchedModsResponse
{
    public string StatusCode { get; set; }
    public List<FetchedMod> Mods { get; set; }
}

public class FetchedMod
{
    public int ModId { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public List<string> Tags { get; set; }
    public string Summary { get; set; }
    public string LastReleased { get; set; }
    public DateTime LastReleasedDateTime => DateTime.ParseExact(LastReleased, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    public string UrlAlias { get; set; }
    public string? Logo { get; set; }
}

#endif