using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup;

// Token: 0x02000005 RID: 5
[DebuggerNonUserCode]
internal static class TypeExtensions
{
    // Token: 0x06000039 RID: 57 RVA: 0x000036D4 File Offset: 0x000018D4
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

    // Token: 0x0600003A RID: 58 RVA: 0x0000375C File Offset: 0x0000195C
    public static string GetStandardTypeName(string typeName)
    {
        EnsureInitialized();
        if (_projectToCompilerTypeName.ContainsKey(typeName))
        {
            return _projectToCompilerTypeName[typeName];
        }
        return typeName;
    }

    // Token: 0x0600003B RID: 59 RVA: 0x0000377D File Offset: 0x0000197D
    public static string GetCSharpTypeName(string typeName)
    {
        EnsureInitialized();
        if (_projectFromCompilerTypeName.ContainsKey(typeName))
        {
            return _projectFromCompilerTypeName[typeName];
        }
        return typeName;
    }

    // Token: 0x0600003C RID: 60 RVA: 0x000037A0 File Offset: 0x000019A0
    public static PropertyInfo GetStaticProperty(this Type type, string propertyName)
    {
        foreach (PropertyInfo propertyInfo in IntrospectionExtensions.GetTypeInfo(type).DeclaredProperties)
        {
            if (propertyInfo.Name.Equals(propertyName))
            {
                return propertyInfo;
            }
        }
        return null;
    }

    // Token: 0x0600003D RID: 61 RVA: 0x00003800 File Offset: 0x00001A00
    public static FieldInfo GetStaticField(this Type type, string fieldName)
    {
        foreach (FieldInfo fieldInfo in IntrospectionExtensions.GetTypeInfo(type).DeclaredFields)
        {
            if (fieldInfo.Name.Equals(fieldName))
            {
                return fieldInfo;
            }
        }
        return null;
    }

    // Token: 0x0600003E RID: 62 RVA: 0x00003860 File Offset: 0x00001A60
    public static string MakeCompilerTypeName(string typeToString)
    {
        string text = typeToString.Replace('[', '<');
        return text.Replace(']', '>');
    }

    // Token: 0x0600003F RID: 63 RVA: 0x00003884 File Offset: 0x00001A84
    public static string GetFullGenericNestedName(Type type)
    {
        string typeName = MakeCompilerTypeName(type.ToString());
        string standardTypeName = GetStandardTypeName(typeName);
        if (!IntrospectionExtensions.GetTypeInfo(type).IsGenericType)
        {
            return standardTypeName;
        }
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

    // Token: 0x06000040 RID: 64 RVA: 0x00003954 File Offset: 0x00001B54
    // Note: this type is marked as 'beforefieldinit'.
    static TypeExtensions()
    {
    }

    // Token: 0x0400002C RID: 44
    private static Dictionary<string, string> _projectToCompilerTypeName = new Dictionary<string, string>();

    // Token: 0x0400002D RID: 45
    private static Dictionary<string, string> _projectFromCompilerTypeName = new Dictionary<string, string>();

    // Token: 0x0400002E RID: 46
    private static readonly string[] _projectionNames = new string[]
    {
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
    };
}
