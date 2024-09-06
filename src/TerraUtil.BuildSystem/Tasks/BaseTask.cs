using Task = Microsoft.Build.Utilities.Task;

namespace TerraUtil.BuildSystem.Tasks;

public abstract class BaseTask : Task
{
    public sealed override bool Execute()
    {
        try
        {
            Run();
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e, true);
        }

        return !Log.HasLoggedErrors;
    }

    protected abstract void Run();
}
