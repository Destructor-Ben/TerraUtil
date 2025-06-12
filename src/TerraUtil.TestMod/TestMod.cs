namespace TerraUtilTestMod;

// Example of using something from TerraUtil.API
public class TestMod : TerraUtilMod
{
    public override void Load()
    {
        // Example of accessing something from a nuget package
        var ignore = new Ignore.Ignore();
    }
}
