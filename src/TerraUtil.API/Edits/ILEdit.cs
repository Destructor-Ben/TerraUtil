using MonoMod.Cil;
using System.Reflection;

namespace TerraUtil.Edits;

/// <summary>
/// An abstraction of an IL edit.
/// </summary>
public abstract class ILEdit : ILoadable
{
    /// <inheritdoc cref="ModType.Mod" />
    public Mod Mod { get; internal set; }

    public void Load(Mod mod)
    {
        Mod = mod;
        AddEdit();
    }

    public void Unload()
    {
        Mod = null;
    }

    /// <summary>
    /// This is where you will add the edit using the auto generated MonoMod hooks.<br />
    /// For example:
    /// <code>
    /// public override void AddEdit()
    /// {
    ///     IL_Main.DoUpdate += PrepareEdit;
    /// }
    /// </code>
    /// </summary>
    public abstract void AddEdit();

    /// <summary>
    /// Call this in <see cref="AddEdit" /> to edit the IL.
    /// </summary>
    /// <param name="il">The IL being edited.</param>
    /// <exception cref="ILPatchFailureException"></exception>
    protected void PrepareEdit(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            Apply(c);
        }
        catch (Exception e)
        {
            throw new ILPatchFailureException(Mod, il, e);
        }
    }

    /// <summary>
    /// This is where you will apply your IL edit. It is wrapped in a try catch so you can use <see cref="ILCursor.Goto(int, MoveType, bool)" /> and other methods that throw exceptions.
    /// </summary>
    /// <param name="c">The <see cref="ILCursor" /> used to edit the IL.</param>
    public abstract void Apply(ILCursor c);
}

/// <summary>
/// An abstraction of an IL edit that uses reflection.
/// </summary>
public abstract class ILEditReflection : ILEdit
{
    /// <summary>
    /// The method being IL edited.
    /// </summary>
    public abstract MethodInfo Method { get; }

    public sealed override void AddEdit()
    {
        MonoModHooks.Modify(Method, PrepareEdit);
    }
}
