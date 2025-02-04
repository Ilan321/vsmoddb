using System.Net;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VsModDb.Constants;
using VsModDb.Data.Entities;
using VsModDb.Data.Entities.Account;
using VsModDb.Data.Repositories;
using VsModDb.Extensions;
using VsModDb.Models.Account;
using VsModDb.Models.Exceptions;
using VsModDb.Models.Options;
using VsModDb.Models.Responses.Account;
using VsModDb.Services.LegacyApi;

namespace VsModDb.Services.Account;

public interface IAccountService
{
    /// <summary>
    /// Creates an <see cref="AccountLinkRequest"/> for the given user.
    /// </summary>
    /// <returns>The user's link token.</returns>
    Task<StartAccountLinkDetails> StartAccountLinkAsync(
        string username,
        string email,
        CancellationToken cancellationToken = default
    );

    Task VerifyAccountLinkAsync(
        string token,
        string? secret,
        CancellationToken cancellationToken
    );

    Task SetAccountPasswordAsync(
        User user,
        string password,
        string token,
        string? secret,
        CancellationToken cancellationToken = default
    );
}

public class AccountService(
    ILogger<AccountService> log,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    TimeProvider timeProvider,
    IAccountLinkRepository accountLinkRepository,
    IOptions<AccountOptions> accountOptions,
    HttpClient httpClient,
    ILegacyApiClient moddbClient
) : IAccountService
{
    public async Task<StartAccountLinkDetails> StartAccountLinkAsync(
        string username,
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var token = Guid.NewGuid().ToString("N");
        var secret = Guid.NewGuid().ToString("N");

        log.LogDebug(
            "Created account link token for user {username}: {token}",
            username,
            token
        );

        await accountLinkRepository.AddLinkRequestAsync(
            username,
            email,
            token,
            secret,
            cancellationToken
        );

        return new()
        {
            Token = token,
            Secret = secret
        };
    }

    public async Task VerifyAccountLinkAsync(
        string token,
        string? secret,
        CancellationToken cancellationToken
    )
    {
        log.LogDebug("Verifying account link token {token}", token);

        var request = await accountLinkRepository.GetLinkRequestAsync(token, cancellationToken);

        if (request is null)
        {
            log.LogWarning("Could not find link request with token {token}", token);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.INVALID_LINK_REQUEST);
        }

        if (request.Secret != secret)
        {
            log.LogWarning("Request secret does not match given secret");

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.INVALID_LINK_REQUEST);
        }
        
        if (!IsLinkRequestActive(request))
        {
            log.LogWarning("Link request with token {token} has expired", token);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.LINK_REQUEST_EXPIRED);
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

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.LINK_VERIFICATION_FAILED);
        }

        if (!string.Equals(match.Author, request.Username))
        {
            log.LogWarning("Found matching link token, but author ({author}) does not match request username ({username})", match.Author, request.Username);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.LINK_VERIFICATION_FAILED);
        }

        var moddbId = await moddbClient.GetUserIdByNameAsync(request.Username, cancellationToken);

        if (!moddbId.HasValue)
        {
            log.LogWarning("Could not get moddb user id for user {username}", request.Username);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.LINK_VERIFICATION_FAILED);
        }

        log.LogInformation("Successfully found matching link token for user {username}, creating user", request.Username);

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = false,
            ModDbUserId = moddbId
        };

        var result = await userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            log.LogError("Failed to create user {username}: {result}", request.Username, result);

            throw new InvalidOperationException($"User creation returned failure: {result}");
        }

        log.LogInformation("User creation succeeded, signing user in");

        await signInManager.SignInAsync(user, isPersistent: true);
    }

    public async Task SetAccountPasswordAsync(
        User user,
        string password,
        string token,
        string? secret,
        CancellationToken cancellationToken = default
    )
    {
        log.LogDebug("Checking if token {token} is still active", token);

        var request = await accountLinkRepository.GetLinkRequestAsync(token, cancellationToken);

        if (request is null)
        {
            log.LogWarning("Could not find link request for token {token}", token);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.INVALID_LINK_REQUEST);
        }

        if (request.Secret != secret)
        {
            log.LogWarning("Request secret do not match saved request details");

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.INVALID_LINK_REQUEST);
        }

        if (!IsLinkRequestActive(request))
        {
            log.LogWarning("Link request {token} is inactive, deleting from db", token);

            await accountLinkRepository.DeleteLinkRequestAsync(token, CancellationToken.None);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.LINK_REQUEST_EXPIRED);
        }

        log.LogDebug("Link request {token} is still active, setting password for user {username}", token, user.UserName);

        var result = await userManager.AddPasswordAsync(user, password);

        if (!result.Succeeded)
        {
            log.LogError("Failed to set password for user {username}: {result}", user.UserName, result);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.PASSWORD_BAD);
        }

        log.LogInformation("Reset password successfully for user {username}, deleting link request", user.UserName);

        await accountLinkRepository.DeleteLinkRequestAsync(token, cancellationToken);
    }

    private bool IsLinkRequestActive(AccountLinkRequest request)
    {
        var timeSinceCreation = timeProvider.GetUtcNow() - request.TimeCreatedUtc;

        return timeSinceCreation.TotalMinutes < accountOptions.Value.LinkTokenExpirationMinutes;
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