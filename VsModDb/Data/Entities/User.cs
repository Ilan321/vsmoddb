using Microsoft.AspNetCore.Identity;
using VsModDb.Data.Entities.Mods;

namespace VsModDb.Data.Entities;

public class User : IdentityUser
{
    public List<ModComment>? Comments { get; set; }
}
