using System;

namespace TerraUtil.BuildSystem.Core;

public class ModReference(string mod, Version? target)
{
    public readonly string Mod = mod;
    public readonly Version? Target = target;

    public override string ToString()
    {
        return Target == null ? Mod : Mod + '@' + Target;
    }

    public static ModReference Parse(string spec)
    {
        string[] split = spec.Split('@');
        switch (split.Length)
        {
            case 1:
                return new ModReference(split[0], null);
            case > 2:
                throw new Exception("Invalid mod reference: " + spec);
            default:
                try
                {
                    return new ModReference(split[0], new Version(split[1]));
                }
                catch
                {
                    throw new Exception("Invalid mod reference: " + spec);
                }
        }
    }
}
