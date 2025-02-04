using HtmlAgilityPack;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VsModDb.Data.Entities;
using VsModDb.Data.Entities.Account;
using VsModDb.Data.Repositories;
using VsModDb.Extensions;
using VsModDb.Models.Account;
using VsModDb.Models.Options;
using VsModDb.Services.LegacyApi;

namespace VsModDb.Services.Account;

public interface IAccountService
{
    /// <summary>
    /// Creates an <see cref="AccountLinkRequest"/> for the given user.
    /// </summary>
    /// <returns>The user's link token.</returns>
    Task<string> StartAccountLinkAsync(
        string username,
        string email,
        CancellationToken cancellationToken = default
    );

    Task<bool> VerifyAccountLinkAsync(string token, CancellationToken cancellationToken);
}

public class AccountService(
    ILogger<AccountService> log,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    TimeProvider timeProvider,
    IAccountLinkRepository accountLinkRepository,
    IOptions<AccountOptions> accountOptions,
    HttpClient httpClient
) : IAccountService
{
    public async Task<string> StartAccountLinkAsync(
        string username,
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var token = Guid.NewGuid().ToString("N");

        log.LogDebug(
            "Created account link token for user {username}: {token}",
            username,
            token
        );

        await accountLinkRepository.AddLinkRequestAsync(
            username,
            email,
            token,
            cancellationToken
        );

        return token;
    }

    public async Task<bool> VerifyAccountLinkAsync(string token, CancellationToken cancellationToken)
    {
        log.LogDebug("Verifying account link token {token}", token);

        var request = await accountLinkRepository.GetLinkRequestAsync(token, cancellationToken);

        if (request is null)
        {
            log.LogWarning("Could not find link request with token {token}", token);

            return false;
        }

        var tokenTime = timeProvider.GetUtcNow() - request.TimeCreatedUtc;

        if (tokenTime.TotalMinutes >= accountOptions.Value.LinkTokenExpirationMinutes)
        {
            log.LogWarning("Link request with token {token} has expired, deleting from database", token);

            await accountLinkRepository.DeleteLinkRequestAsync(token, CancellationToken.None);

            return false;
        }

        log.LogDebug(
            "Fetched account link request with token {token} successfully - verifying the user has commented the token on {url}",
            token,
            accountOptions.Value.LinkTokenModPostUrl
        );

        var comments = await GetAccountLinkPostCommentsAsync(cancellationToken);

        log.LogDebug("Found {count} comments on account link mod post", comments.Count);

        var match = comments.FirstOrDefault(f => f.Comment.Contains(token));

        if (match is null)
        {
            log.LogWarning("Could not find comment with matching token {token}", request.LinkToken);

            return false;
        }

        if (!string.Equals(match.Author, request.Username))
        {
            log.LogWarning("Found matching link token, but author ({author}) does not match request username ({username})", match.Author, request.Username);

            return false;
        }

        log.LogInformation("Successfully found matching link token for user {username}, creating user", request.Username);

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            log.LogError("Failed to create user {username}: {result}", request.Username, result);

            throw new InvalidOperationException($"User creation returned failure: {result}");
        }

        log.LogInformation("User creation succeeded, signing user in and deleting link token request");

        await signInManager.SignInAsync(user, isPersistent: true);

        await accountLinkRepository.DeleteLinkRequestAsync(token, CancellationToken.None);

        return true;
    }

    private async Task<List<AccountLinkVerificationComment>> GetAccountLinkPostCommentsAsync(
        CancellationToken cancellationToken
    )
    {
        const string CommentsXPath = "//div[@class='comments']/div[@class='editbox comment' and contains(@id, 'cmt-')]";
        const string AuthorXPath = "./div[@class='title']";
        const string ContentXPath = "div[@class='body']";

        using var response = await httpClient.GetAsync(accountOptions.Value.LinkTokenModPostUrl, cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();

        doc.LoadHtml(content);

        var comments = doc.DocumentNode
            .SelectNodes(CommentsXPath)
            ?.ToList() ?? [];

        return comments
            .Select(
                f => new AccountLinkVerificationComment
                {
                    Author = ParseAuthorName(f.SelectSingleNode(AuthorXPath)),
                    Comment = f.SelectSingleNode(ContentXPath).InnerText
                }
            )
            .ToList();

        string ParseAuthorName(HtmlNode authorNode)
        {
            const string StartSplit = "</a>";
            const string EndSplit = "<span";

            return authorNode
                .InnerHtml
                .Split(StartSplit)[1..].JoinString()
                .Split(EndSplit)[0]
                .Trim();
        }
    }
}