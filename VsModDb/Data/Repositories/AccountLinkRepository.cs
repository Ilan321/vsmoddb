using Microsoft.EntityFrameworkCore;
using VsModDb.Data.Entities.Account;

namespace VsModDb.Data.Repositories;

public interface IAccountLinkRepository
{
    Task AddLinkRequestAsync(
        string username,
        string email,
        string token,
        CancellationToken cancellationToken = default
    );

    Task<AccountLinkRequest?> GetLinkRequestAsync(string token, CancellationToken cancellationToken = default);
    Task DeleteLinkRequestAsync(string token, CancellationToken cancellationToken = default);
}

public class AccountLinkRepository(
    ILogger<AccountLinkRepository> log,
    ModDbContext context
) : IAccountLinkRepository
{
    public async Task AddLinkRequestAsync(
        string username,
        string email,
        string token,
        CancellationToken cancellationToken = default
    )
    {
        context.AccountLinkRequests.Add(
            new AccountLinkRequest
            {
                Username = username,
                Email = email,
                LinkToken = token
            }
        );

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<AccountLinkRequest?> GetLinkRequestAsync(string token, CancellationToken cancellationToken = default) =>
        context
            .AccountLinkRequests
            .AsNoTracking()
            .Where(f => f.LinkToken == token)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    public Task DeleteLinkRequestAsync(string token, CancellationToken cancellationToken = default) => context
        .AccountLinkRequests
        .Where(f => f.LinkToken == token)
        .ExecuteDeleteAsync(cancellationToken: cancellationToken);
}