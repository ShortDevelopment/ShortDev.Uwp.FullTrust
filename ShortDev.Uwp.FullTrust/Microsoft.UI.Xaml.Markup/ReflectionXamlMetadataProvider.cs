using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Microsoft.UI.Xaml.Markup;

public sealed class ReflectionXamlMetadataProvider : IXamlMetadataProvider
{
    public ReflectionXamlMetadataProvider()
    {
        lock (_assembliesLock)
        {
            if (!gotAssemblies)
            {
                _providers ??= new List<WeakReference<ReflectionXamlMetadataProvider>>();
                _providers.Add(new WeakReference<ReflectionXamlMetadataProvider>(this));
                _assemblies = new List<string>
                {
                    IntrospectionExtensions.GetTypeInfo(typeof(Application)).Assembly.FullName,
                    IntrospectionExtensions.GetTypeInfo(typeof(object)).Assembly.FullName
                };
                _assemblies.AddRange(GetAssemblyList());
            }
        }
    }

    private static IReadOnlyList<string> GetAssemblyList()
        => Directory.GetFiles(AppContext.BaseDirectory, "*.dll", SearchOption.AllDirectories);

    public IXamlType GetXamlType(Type type)
        => GetXamlTypeInternal(type);

    public IXamlType GetXamlType(string fullName)
        => GetXamlTypeInternal(fullName);

    public XmlnsDefinition[] GetXmlnsDefinitions()
        => Array.Empty<XmlnsDefinition>();

    internal static IXamlType GetXamlTypeInternal(Type typeID)
    {
        object xamlCacheLock = _xamlCacheLock;
        IXamlType xamlType;
        lock (xamlCacheLock)
        {
            if (_xamlTypeCacheByType.TryGetValue(typeID, out xamlType))
            {
                return xamlType;
            }
            xamlType = CreateXamlType(typeID);
            if (xamlType != null)
            {
                object xamlCacheLock2 = _xamlCacheLock;
                lock (xamlCacheLock2)
                {
                    _xamlTypeCacheByName.Add(xamlType.FullName, xamlType);
                    _xamlTypeCacheByType.Add(xamlType.UnderlyingType, xamlType);
                }
            }
        }
        return xamlType;
    }

    internal static IXamlType GetXamlTypeInternal(string typeName)
    {
        string text = TypeExtensions.MakeCompilerTypeName(typeName);
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        object xamlCacheLock = _xamlCacheLock;
        IXamlType xamlType;
        lock (xamlCacheLock)
        {
            if (_xamlTypeCacheByName.TryGetValue(text, out xamlType))
            {
                return xamlType;
            }
            xamlType = CreateXamlType(text);
            if (xamlType != null)
            {
                object xamlCacheLock2 = _xamlCacheLock;
                lock (xamlCacheLock2)
                {
                    _xamlTypeCacheByName.Add(xamlType.FullName, xamlType);
                    _xamlTypeCacheByType.Add(xamlType.UnderlyingType, xamlType);
                    if (!xamlType.FullName.Equals(text))
                    {
                        throw new ReflectionHelperException(string.Concat(new string[]
                        {
                            "Created Xaml type '",
                            xamlType.FullName,
                            "' has a different name than requested type '",
                            text,
                            "'"
                        }));
                    }
                }
            }
        }
        return xamlType;
    }

    private static IXamlType CreateXamlType(Type typeID)
        => XamlReflectionType.Create(typeID);

    private static IXamlType CreateXamlType(string typeName)
    {
        string compilerTypeName = TypeExtensions.MakeCompilerTypeName(typeName);
        if (IsGenericTypeName(compilerTypeName))
        {
            return ConstructGenericType(compilerTypeName);
        }
        return XamlReflectionType.Create(GetNonGenericType(compilerTypeName));
    }

    private static Type GetNonGenericType(string compilerTypeName)
    {
        compilerTypeName = TypeExtensions.GetCSharpTypeName(compilerTypeName);
        foreach (string text in _assemblies)
        {
            try
            {
                Type type = Type.GetType(compilerTypeName + ", " + text);
                if (type != null)
                {
                    return type;
                }
            }
            catch (Exception)
            {
            }
        }
        return null;
    }

    private static IXamlType ConstructGenericType(string compilerTypeName)
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
                        Type nonGenericType = GetNonGenericType(compilerTypeName2);
                        if (nonGenericType == null)
                        {
                            return null;
                        }
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

    private IXamlMember CreateXamlMember(string longMemberName)
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

    private static List<WeakReference<ReflectionXamlMetadataProvider>> _providers;

    private static List<string> _assemblies;

    private static readonly object _assembliesLock = new();

    private static bool gotAssemblies = false;

    private static readonly object _xamlCacheLock = new();

    private static readonly Dictionary<string, IXamlType> _xamlTypeCacheByName = new();

    private static readonly Dictionary<Type, IXamlType> _xamlTypeCacheByType = new();
}
