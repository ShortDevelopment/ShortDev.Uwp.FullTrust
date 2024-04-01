using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace ShortDev.Uwp.Node;
internal class Initializer
{
    [ModuleInitializer]
    public static void Main()
    {
        var assembly = typeof(XamlHelper).Assembly;
        var directory = Path.GetDirectoryName(assembly.Location)!;
        var loadContext = AssemblyLoadContext.GetLoadContext(assembly)!;
        foreach (var filePath in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                loadContext.LoadFromAssemblyPath(filePath);
            }
            catch { }
        }
    }
}
