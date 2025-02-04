using Microsoft.AspNetCore.Identity;
using VsModDb.Data.Entities.Mods;

namespace VsModDb.Data.Entities;

public class User : IdentityUser
{
    /// <summary>
    /// The user's ID on the ModDB API.
    /// </summary>
    public int? ModDbUserId { get; set; }

    public List<ModComment>? Comments { get; set; }
}
