using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace ShortDev.Uwp.Node;
internal static class Initializer
{
    public static string BaseDirectory { get; private set; } = null!;

    [ModuleInitializer]
    public static async void Main()
    {
        Debugger.Launch();

        var assembly = typeof(XamlHelper).Assembly;
        BaseDirectory = Path.GetDirectoryName(assembly.Location)!;
        var loadContext = AssemblyLoadContext.GetLoadContext(assembly)!;
        foreach (var filePath in Directory.EnumerateFiles(BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                loadContext.LoadFromAssemblyPath(filePath);
            }
            catch { }
        }
    }
}
