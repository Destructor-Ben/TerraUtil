namespace TerraUtil.BuildSystem.Core;

public class ModReference(string mod, Version? version)
{
    public readonly string Mod = mod;
    public readonly Version? Version = version;

    public override string ToString()
    {
        return Version == null ? Mod : Mod + '@' + Version;
    }
}
