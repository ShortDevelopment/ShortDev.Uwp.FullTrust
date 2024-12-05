using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Microsoft.UI.Xaml.Markup;

public sealed partial class ReflectionXamlMetadataProvider : IXamlMetadataProvider
{
    public ReflectionXamlMetadataProvider()
    {
        _providers.Add(new(this));
    }

    public IXamlType? GetXamlType(Type type)
        => GetXamlTypeInternal(type);

    public IXamlType? GetXamlType(string fullName)
        => GetXamlTypeInternal(fullName);

    public XmlnsDefinition[] GetXmlnsDefinitions()
        => [];

    internal static IXamlType? GetXamlTypeInternal(Type typeID)
    {
        if (_xamlTypeCacheByType.TryGetValue(typeID, out var xamlType))
            return xamlType;

        xamlType = CreateXamlType(typeID);
        if (xamlType != null)
        {
            _xamlTypeCacheByName.TryAdd(xamlType.FullName, xamlType);
            _xamlTypeCacheByType.TryAdd(xamlType.UnderlyingType, xamlType);
        }
        return xamlType;
    }

    internal static IXamlType? GetXamlTypeInternal(string typeName)
    {
        string compilerTypeName = TypeExtensions.MakeCompilerTypeName(typeName);
        if (string.IsNullOrEmpty(compilerTypeName))
            return null;

        if (_xamlTypeCacheByName.TryGetValue(compilerTypeName, out var xamlType))
            return xamlType;

        xamlType = CreateXamlType(compilerTypeName);
        if (xamlType != null)
        {
            _xamlTypeCacheByName.TryAdd(xamlType.FullName, xamlType);
            _xamlTypeCacheByType.TryAdd(xamlType.UnderlyingType, xamlType);
            if (!xamlType.FullName.Equals(compilerTypeName))
                throw new ReflectionHelperException($"Created Xaml type '{xamlType.FullName}' has a different name than requested type '{compilerTypeName}'");
        }
        return xamlType;
    }

    private static IXamlType CreateXamlType(Type typeID)
        => XamlReflectionType.Create(typeID);

    private static IXamlType? CreateXamlType(string typeName)
    {
        string compilerTypeName = TypeExtensions.MakeCompilerTypeName(typeName);
        if (IsGenericTypeName(compilerTypeName))
            return ConstructGenericType(compilerTypeName);
        return XamlReflectionType.Create(GetNonGenericType(compilerTypeName));
    }

    private static Type? GetNonGenericType(string compilerTypeName)
    {
        compilerTypeName = TypeExtensions.GetCSharpTypeName(compilerTypeName);
        foreach (string assemblyName in _assemblies)
        {
            try
            {
                var type = Type.GetType($"{compilerTypeName}, {assemblyName}");
                if (type != null)
                    return type;
            }
            catch { }
        }
        return null;
    }

    private static IXamlType? ConstructGenericType(string compilerTypeName)
    {
        StringBuilder stringBuilder = new();
        Stack<List<Type>> stack = new();
        Stack<Type> stack2 = new();
        int i = 0;
        while (i < compilerTypeName.Length)
        {
            char c = compilerTypeName[i];
            if (c <= ',')
            {
                if (c != ' ')
                {
                    if (c != ',')
                    {
                        goto IL_140;
                    }
                    if (stringBuilder.Length > 0)
                    {
                        string compilerTypeName2 = stringBuilder.ToString();
                        stringBuilder.Clear();
                        var nonGenericType = GetNonGenericType(compilerTypeName2);
                        if (nonGenericType == null)
                            return null;

                        stack.Peek().Add(nonGenericType);
                    }
                }
            }
            else if (c != '<')
            {
                if (c != '>')
                {
                    goto IL_140;
                }
                if (stringBuilder.Length > 0)
                {
                    string compilerTypeName3 = stringBuilder.ToString();
                    stringBuilder.Clear();
                    Type nonGenericType2 = GetNonGenericType(compilerTypeName3);
                    if (nonGenericType2 == null)
                    {
                        return null;
                    }
                    stack.Peek().Add(nonGenericType2);
                }
                List<Type> list = stack.Pop();
                Type[] array = list.ToArray();
                Type type = stack2.Pop();
                Type type2 = type.MakeGenericType(array);
                if (type2 == null)
                {
                    return null;
                }
                if (stack.Count > 0)
                {
                    stack.Peek().Add(type2);
                }
                else
                {
                    stack2.Push(type2);
                }
            }
            else
            {
                string compilerTypeName4 = stringBuilder.ToString();
                Type nonGenericType3 = GetNonGenericType(compilerTypeName4);
                if (nonGenericType3 == null)
                {
                    return null;
                }
                stack2.Push(nonGenericType3);
                stack.Push(new List<Type>());
                stringBuilder.Clear();
            }
        IL_149:
            i++;
            continue;
        IL_140:
            stringBuilder.Append(c);
            goto IL_149;
        }
        if (stack2.Count != 1)
        {
            throw new ReflectionHelperException("Error constructing generic type '" + compilerTypeName + "'");
        }
        return XamlReflectionType.Create(stack2.Pop());
    }

    private static bool IsGenericTypeName(string compilerTypeName)
        => compilerTypeName.Contains('<') || compilerTypeName.Contains('`');

    private static IXamlMember CreateXamlMember(string longMemberName)
    {
        GetTypeAndMember(longMemberName, out string typeName, out string memberName);
        return XamlReflectionMember.Create(memberName, GetXamlTypeInternal(typeName).UnderlyingType);
    }

    private static void GetTypeAndMember(string longMemberName, out string typeName, out string memberName)
    {
        int num = longMemberName.LastIndexOf('.');
        string text = longMemberName[..num];
        string text2 = longMemberName[(num + 1)..];
        typeName = text;
        memberName = text2;
    }

    private static readonly List<WeakReference<ReflectionXamlMetadataProvider>> _providers = [];

    private static readonly List<string> _assemblies =
    [
        IntrospectionExtensions.GetTypeInfo(typeof(Application)).Assembly.FullName!,
        IntrospectionExtensions.GetTypeInfo(typeof(object)).Assembly.FullName!,
        // .. Directory.GetFiles(AppContext.BaseDirectory, "*.dll", SearchOption.AllDirectories)
    ];

    private static readonly ConcurrentDictionary<string, IXamlType> _xamlTypeCacheByName = [];

    private static readonly ConcurrentDictionary<Type, IXamlType> _xamlTypeCacheByType = [];
}
