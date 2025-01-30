using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VsModDb.Data;
using VsModDb.Data.Entities;
using VsModDb.Data.Entities.Assets;
using VsModDb.Data.Entities.Mods;
using VsModDb.Models.Assets;
using VsModDb.Models.Mods;
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
        var apiSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        using var modsHttpResponse = await httpClient.GetAsync("https://mods.vintagestory.at/api/mods", cancellationToken);

        modsHttpResponse.EnsureSuccessStatusCode();

        var modsResponse = await modsHttpResponse.Content.ReadFromJsonAsync<FetchedModsResponse>(apiSerializerOptions, cancellationToken: cancellationToken);

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
                UrlAlias = fetchedMod.UrlAlias,
                Comments = new()
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

            using var commentsResponse = await httpClient.GetAsync($"https://mods.vintagestory.at/api/comments/{fetchedMod.AssetId}", cancellationToken);

            var comments =
                await commentsResponse.Content.ReadFromJsonAsync<FetchedModCommentsResponse>(apiSerializerOptions,
                    cancellationToken);

            foreach (var fetchedComment in comments.Comments)
            {
                var userName = await GetOrCreateUserAsync(fetchedComment.UserId);

                if (string.IsNullOrWhiteSpace(userName))
                {
                    continue;
                }

                mod.Comments.Add(new()
                {
                    Comment = fetchedComment.Text,
                    TimeCreatedUtc = fetchedComment.CreatedDateTime,
                    TimeUpdatedUtc = fetchedComment.LastModifiedDateTime,
                    LinkedUserId = userName,
                    ContentType = ModCommentContentType.Html
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private Dictionary<int, string> _authorCache = new();

    private async Task<string?> GetOrCreateUserAsync(int apiUserId)
    {
        if (!_authorCache.Any())
        {
            using var authorsResponse = await httpClient.GetAsync("https://mods.vintagestory.at/api/authors");

            var authors = await authorsResponse.Content.ReadFromJsonAsync<FetchAuthorsResponse>();

            _authorCache = authors.Authors.ToDictionary(f => f.UserId, f => f.Name);
        }

        var userName = _authorCache.GetValueOrDefault(apiUserId);

        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var user = await userManager.FindByNameAsync(userName);

        if (user is null)
        {
            user = new User
            {
                UserName = userName
            };

            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create user {userName}: {result}");
            }
        }

        return user.Id;
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
    public int AssetId { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public List<string> Tags { get; set; }
    public string Summary { get; set; }
    public string LastReleased { get; set; }
    public DateTime LastReleasedDateTime => DateTime.ParseExact(LastReleased, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    public string UrlAlias { get; set; }
    public string? Logo { get; set; }
}

public class FetchedModCommentsResponse
{
    public string StatusCode { get; set; }
    public List<FetchedModComment> Comments { get; set; }
}

public class FetchedModComment
{
    public int CommentId { get; set; }
    public string Text { get; set; }
    public int UserId { get; set; }
    public string Created { get; set; }
    public DateTime CreatedDateTime => DateTime.ParseExact(Created, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    public string LastModified { get; set; }
    public DateTime LastModifiedDateTime => DateTime.ParseExact(LastModified, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

public class FetchAuthorsResponse
{
    public string StatusCode { get; set; }
    public List<FetchedAuthor> Authors { get; set; }
}

public class FetchedAuthor
{
    public int UserId { get; set; }
    public string Name { get; set; }
}

#endif