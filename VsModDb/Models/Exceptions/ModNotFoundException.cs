namespace VsModDb.Models.Exceptions;

public class ModNotFoundException : Exception
{
    public ModNotFoundException(int modId) : base($"Could not find mod {modId}") { }

    public ModNotFoundException(string alias) : base($"Could not find mod with alias {alias}") { }
}
