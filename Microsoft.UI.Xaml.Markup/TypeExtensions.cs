using System.Diagnostics;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup;

[DebuggerNonUserCode]
internal static class TypeExtensions
{
    internal static void EnsureInitialized()
    {
        Dictionary<string, string> projectToCompilerTypeName = _projectToCompilerTypeName;
        lock (projectToCompilerTypeName)
        {
            if (_projectToCompilerTypeName.Count == 0)
            {
                foreach (string text in _projectionNames)
                {
                    string text2 = "System." + text;
                    _projectToCompilerTypeName.Add(text2, text);
                    _projectFromCompilerTypeName.Add(text, text2);
                }
            }
        }
    }

    public static string GetStandardTypeName(string typeName)
    {
        EnsureInitialized();
        if (_projectToCompilerTypeName.TryGetValue(typeName, out var value))
            return value;
        return typeName;
    }

    public static string GetCSharpTypeName(string typeName)
    {
        EnsureInitialized();
        if (_projectFromCompilerTypeName.TryGetValue(typeName, out var value))
            return value;
        return typeName;
    }

    public static PropertyInfo? GetStaticProperty(this Type type, string propertyName)
    {
        foreach (PropertyInfo propertyInfo in IntrospectionExtensions.GetTypeInfo(type).DeclaredProperties)
        {
            if (propertyInfo.Name.Equals(propertyName))
                return propertyInfo;
        }
        return null;
    }

    public static FieldInfo? GetStaticField(this Type type, string fieldName)
    {
        foreach (FieldInfo fieldInfo in IntrospectionExtensions.GetTypeInfo(type).DeclaredFields)
        {
            if (fieldInfo.Name.Equals(fieldName))
                return fieldInfo;
        }
        return null;
    }

    public static string MakeCompilerTypeName(string typeToString)
        => typeToString.Replace('[', '<').Replace(']', '>');

    public static string GetFullGenericNestedName(Type type)
    {
        string typeName = MakeCompilerTypeName(type.ToString());
        string standardTypeName = GetStandardTypeName(typeName);
        if (!IntrospectionExtensions.GetTypeInfo(type).IsGenericType)
            return standardTypeName;

        string text = "<";
        string text2 = ">";
        Type[] genericTypeArguments = IntrospectionExtensions.GetTypeInfo(type).GenericTypeArguments;
        Type genericTypeDefinition = IntrospectionExtensions.GetTypeInfo(type).GetGenericTypeDefinition();
        string text3 = MakeCompilerTypeName(genericTypeDefinition.ToString());
        text3 = text3.Substring(0, text3.IndexOf('<'));
        string text4 = text3;
        text4 += text;
        for (int i = 0; i < genericTypeArguments.Length; i++)
        {
            text4 += GetFullGenericNestedName(genericTypeArguments[i]);
            if (i < genericTypeArguments.Length - 1)
            {
                text4 += ", ";
            }
        }
        return text4 + text2;
    }

    static readonly Dictionary<string, string> _projectToCompilerTypeName = [];
    static readonly Dictionary<string, string> _projectFromCompilerTypeName = [];

    private static readonly string[] _projectionNames =
    [
        "Byte",
        "UInt8",
        "SByte",
        "Int8",
        "Char",
        "Char16",
        "Single",
        "Double",
        "Int16",
        "Int32",
        "Int64",
        "UInt16",
        "UInt32",
        "UInt64",
        "Boolean",
        "String",
        "Object",
        "Guid",
        "TimeSpan",
        "DateTime"
    ];
}
