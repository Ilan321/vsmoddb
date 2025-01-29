namespace VsModDb.Models.Exceptions;

public class ModNotFoundException(int modId) : Exception($"Could not find mod {modId}");
