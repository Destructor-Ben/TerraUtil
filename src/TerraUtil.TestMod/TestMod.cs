using Terraria.ModLoader;

namespace TerraUtilTestMod;

public class TestMod : Mod
{
    public override void Load()
    {
        // Example of accessing something from a nuget package
        var ignore = new Ignore.Ignore();
    }
}
