using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Markup;

namespace Microsoft.UI.Xaml.Markup;

[DebuggerNonUserCode]
internal sealed class XamlReflectionType : IXamlType, IXamlType2
{
    private XamlReflectionType(Type underlyingType)
    {
        _underlyingType = underlyingType;
        _info = IntrospectionExtensions.GetTypeInfo(_underlyingType);
        _fullName = TypeExtensions.GetFullGenericNestedName(_underlyingType);
        IsMarkupExtension = XamlReflectionType.markupExtTypeInfo.IsAssignableFrom(_info);
        RuntimeHelpers.RunClassConstructor(_underlyingType.TypeHandle);
        IEnumerable<Type> implementedInterfaces = IntrospectionExtensions.GetTypeInfo(_underlyingType).ImplementedInterfaces;
        Type type = null;
        Type type2 = null;
        Type type3 = null;
        foreach (Type type4 in implementedInterfaces)
        {
            string fullName = type4.FullName;
            if (type == null && fullName.StartsWith("System.Collections.Generic.ICollection`1"))
            {
                type = type4;
            }
            else if (type2 == null && fullName.StartsWith("System.Collections.Generic.IDictionary`2"))
            {
                type2 = type4;
            }
            else if (type3 == null && fullName.StartsWith("System.Collections.Generic.IList`1"))
            {
                type3 = type4;
            }
        }
        bool flag = XamlReflectionType.iListTypeInfo.IsAssignableFrom(_info);
        bool flag2 = XamlReflectionType.iDictionaryTypeInfo.IsAssignableFrom(_info);
        bool flag3 = XamlReflectionType.iEnumerableTypeInfo.IsAssignableFrom(_info);
        IsCollection = (type != null || type3 != null || flag);
        IsDictionary = (flag2 || type2 != null);
        if (IsDictionary)
        {
            if (type2 != null)
            {
                ItemType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(IntrospectionExtensions.GetTypeInfo(type2).GenericTypeArguments[0]);
                KeyType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(IntrospectionExtensions.GetTypeInfo(type2).GenericTypeArguments[1]);
            }
            else
            {
                ItemType = (KeyType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(typeof(object)));
            }
        }
        else if (IsCollection)
        {
            if (type3 != null)
            {
                ItemType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(IntrospectionExtensions.GetTypeInfo(type3).GenericTypeArguments[0]);
            }
            else if (type != null)
            {
                ItemType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(IntrospectionExtensions.GetTypeInfo(type).GenericTypeArguments[0]);
            }
            else
            {
                ItemType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(typeof(object));
            }
        }
        if (IsCollection || IsDictionary || flag3)
        {
            IEnumerable<MethodInfo> runtimeMethods = RuntimeReflectionExtensions.GetRuntimeMethods(_underlyingType);
            IEnumerable<MethodInfo> enumerable = null;
            if (type != null)
            {
                enumerable = RuntimeReflectionExtensions.GetRuntimeMethods(type);
            }
            else if (type2 != null)
            {
                enumerable = RuntimeReflectionExtensions.GetRuntimeMethods(type3);
            }
            List<MethodInfo> list;
            if (enumerable == null)
            {
                list = Enumerable.ToList<MethodInfo>(runtimeMethods);
            }
            else
            {
                list = Enumerable.ToList<MethodInfo>(Enumerable.Concat<MethodInfo>(runtimeMethods, enumerable));
            }
            foreach (MethodInfo methodInfo in list)
            {
                if (methodInfo.IsPublic && methodInfo.Name.Equals("Add"))
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (flag3 && !IsCollection && !IsDictionary)
                    {
                        if (parameters.Length == 1)
                        {
                            IsCollection = true;
                            ItemType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(parameters[0].ParameterType);
                        }
                        else if (parameters.Length == 2)
                        {
                            IsDictionary = true;
                            KeyType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(parameters[0].ParameterType);
                            ItemType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(parameters[1].ParameterType);
                        }
                    }
                    _adderInfo = methodInfo;
                    break;
                }
            }
        }
        IsConstructible = GetIsConstructible();
        IsBindable = HasBindableAttribute();
    }

    public static XamlReflectionType Create(Type typeID)
    {
        if (typeID == null)
        {
            return null;
        }
        return new XamlReflectionType(typeID);
    }

    // (get) Token: 0x06000014 RID: 20 RVA: 0x000029F5 File Offset: 0x00000BF5
    public string FullName
    {
        get
        {
            return _fullName;
        }
    }

    // (get) Token: 0x06000015 RID: 21 RVA: 0x000029FD File Offset: 0x00000BFD
    public Type UnderlyingType
    {
        get
        {
            return _underlyingType;
        }
    }

    // (get) Token: 0x06000016 RID: 22 RVA: 0x00002A08 File Offset: 0x00000C08
    public IXamlType BaseType
    {
        get
        {
            if (!_gotBaseType)
            {
                _gotBaseType = true;
                if (_info.BaseType == null || _info.BaseType.FullName == "System.Runtime.InteropServices.WindowsRuntime.RuntimeClass")
                {
                    _baseType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(typeof(object));
                }
                else
                {
                    _baseType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(_info.BaseType);
                }
            }
            return _baseType;
        }
    }

    // (get) Token: 0x06000017 RID: 23 RVA: 0x00002A80 File Offset: 0x00000C80
    public IXamlType BoxedType
    {
        get
        {
            if (!_gotBoxedType)
            {
                _gotBoxedType = true;
                if (FullName.StartsWith("System.Nullable`1") || FullName.StartsWith("Windows.Foundation.IReference`1"))
                {
                    Type typeID = _info.GenericTypeArguments[0];
                    _boxedType = ReflectionXamlMetadataProvider.GetXamlTypeInternal(typeID);
                }
                else
                {
                    _boxedType = null;
                }
            }
            return _boxedType;
        }
    }

    // (get) Token: 0x06000018 RID: 24 RVA: 0x00002AEC File Offset: 0x00000CEC
    public IXamlMember ContentProperty
    {
        get
        {
            if (!_gotContentProperty)
            {
                _gotContentProperty = true;
                string stringNamedArgumentFromAttribute = GetStringNamedArgumentFromAttribute("Windows.UI.Xaml.Markup.ContentPropertyAttribute", "Name");
                if (stringNamedArgumentFromAttribute != null)
                {
                    _contentProperty = GetMember(GetStringNamedArgumentFromAttribute("Windows.UI.Xaml.Markup.ContentPropertyAttribute", "Name"));
                }
            }
            return _contentProperty;
        }
    }

    public IXamlMember GetMember(string name)
    {
        IXamlMember xamlMember = null;
        if (_members == null)
        {
            _members = new Dictionary<string, IXamlMember>();
        }
        else if (_members.TryGetValue(name, out xamlMember))
        {
            return xamlMember;
        }
        xamlMember = XamlReflectionMember.Create(name, _underlyingType);
        _members.Add(name, xamlMember);
        return xamlMember;
    }

    // (get) Token: 0x0600001A RID: 26 RVA: 0x00002B91 File Offset: 0x00000D91
    public bool IsArray
    {
        get
        {
            return _underlyingType.IsArray;
        }
    }

    // (get) Token: 0x0600001B RID: 27 RVA: 0x00002B9E File Offset: 0x00000D9E
    public bool IsCollection { get; }

    // (get) Token: 0x0600001C RID: 28 RVA: 0x00002BA6 File Offset: 0x00000DA6
    public bool IsConstructible { get; }

    // (get) Token: 0x0600001D RID: 29 RVA: 0x00002BAE File Offset: 0x00000DAE
    public bool IsDictionary { get; }

    // (get) Token: 0x0600001E RID: 30 RVA: 0x00002BB6 File Offset: 0x00000DB6
    public bool IsMarkupExtension { get; }

    // (get) Token: 0x0600001F RID: 31 RVA: 0x00002BBE File Offset: 0x00000DBE
    public bool IsBindable { get; }

    // (get) Token: 0x06000020 RID: 32 RVA: 0x00002BC6 File Offset: 0x00000DC6
    public IXamlType ItemType { get; }

    // (get) Token: 0x06000021 RID: 33 RVA: 0x00002BCE File Offset: 0x00000DCE
    public IXamlType KeyType { get; }

    public object ActivateInstance()
    => Activator.CreateInstance(_underlyingType);

    public void AddToMap(object instance, object key, object value)
    {
        _adderInfo.Invoke(instance, new object[]
        {
            key,
            value
        });
    }

    public void AddToVector(object instance, object value)
    {
        _adderInfo.Invoke(instance, new object[]
        {
            value
        });
    }

    public void RunInitializer()
    {
        RuntimeHelpers.RunClassConstructor(_underlyingType.TypeHandle);
    }

    public object CreateFromString(string value)
    {
        if (!_gotCreateFromStringMethod)
        {
            _gotCreateFromStringMethod = true;
            string stringNamedArgumentFromAttribute = GetStringNamedArgumentFromAttribute("Windows.Foundation.Metadata.CreateFromStringAttribute", "MethodName");
            if (stringNamedArgumentFromAttribute != null)
            {
                int num = stringNamedArgumentFromAttribute.LastIndexOf('.');
                Type type;
                string text;
                if (num == -1)
                {
                    type = _underlyingType;
                    text = stringNamedArgumentFromAttribute;
                }
                else
                {
                    int num2 = stringNamedArgumentFromAttribute.IndexOf('+');
                    if (num2 == -1)
                    {
                        string typeName = stringNamedArgumentFromAttribute.Substring(0, num);
                        string text2 = stringNamedArgumentFromAttribute.Substring(num + 1);
                        type = ReflectionXamlMetadataProvider.GetXamlTypeInternal(typeName).UnderlyingType;
                        text = text2;
                    }
                    else
                    {
                        string[] array = stringNamedArgumentFromAttribute.Split(new char[]
                        {
                            '+'
                        });
                        string typeName2 = array[0];
                        Type type2 = ReflectionXamlMetadataProvider.GetXamlTypeInternal(typeName2).UnderlyingType;
                        for (int i = 1; i < array.Length - 1; i++)
                        {
                            type2 = IntrospectionExtensions.GetTypeInfo(type2).GetDeclaredNestedType(array[i]).AsType();
                        }
                        string text3 = array[array.Length - 1];
                        int num3 = text3.LastIndexOf('.');
                        string text4 = text3.Substring(0, num3);
                        string text5 = text3.Substring(num3 + 1);
                        type = IntrospectionExtensions.GetTypeInfo(type2).GetDeclaredNestedType(text4).AsType();
                        text = text5;
                    }
                }
                _createFromStringMethod = RuntimeReflectionExtensions.GetRuntimeMethod(type, text, new Type[]
                {
                    typeof(string)
                });
            }
        }
        if (BoxedType != null)
        {
            object obj = BoxedType.CreateFromString(value);
            foreach (ConstructorInfo constructorInfo in _info.DeclaredConstructors)
            {
                ParameterInfo[] parameters = constructorInfo.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType.Equals(_info.GenericTypeArguments[0]))
                {
                    return constructorInfo.Invoke(new object[]
                    {
                        obj
                    });
                }
            }
            throw new ReflectionHelperException("Couldn't locate appropriate boxing constructor for boxed type '" + FullName + "'");
        }
        if (_createFromStringMethod != null)
        {
            return _createFromStringMethod.Invoke(null, new object[]
            {
                value
            });
        }
        return ParseEnumValue(value);
    }

    private object ParseEnumValue(string value)
    {
        string[] parts = value.Split(new char[]
        {
            ','
        });
        if (_enumType == null)
        {
            Type underlyingType = Enum.GetUnderlyingType(_underlyingType);
            if (underlyingType.Equals(typeof(sbyte)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.SByte);
            }
            else if (underlyingType.Equals(typeof(byte)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.Byte);
            }
            else if (underlyingType.Equals(typeof(short)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.Int16);
            }
            else if (underlyingType.Equals(typeof(ushort)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.UInt16);
            }
            else if (underlyingType.Equals(typeof(int)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.Int32);
            }
            else if (underlyingType.Equals(typeof(uint)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.UInt32);
            }
            else if (underlyingType.Equals(typeof(long)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.Int64);
            }
            else if (underlyingType.Equals(typeof(ulong)))
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.UInt64);
            }
            else
            {
                _enumType = new XamlReflectionType.EnumType?(XamlReflectionType.EnumType.Int32);
            }
        }
        XamlReflectionType.EnumType? enumType = _enumType;
        if (enumType != null)
        {
            switch (enumType.GetValueOrDefault())
            {
                case XamlReflectionType.EnumType.SByte:
                    return (sbyte)ProcessEnumStringSigned(parts, _underlyingType);
                case XamlReflectionType.EnumType.Byte:
                    return (byte)ProcessEnumStringUnsigned(parts, _underlyingType);
                case XamlReflectionType.EnumType.Int16:
                    return (short)ProcessEnumStringSigned(parts, _underlyingType);
                case XamlReflectionType.EnumType.UInt16:
                    return (ushort)ProcessEnumStringUnsigned(parts, _underlyingType);
                case XamlReflectionType.EnumType.Int32:
                    return (int)ProcessEnumStringSigned(parts, _underlyingType);
                case XamlReflectionType.EnumType.UInt32:
                    return (uint)ProcessEnumStringUnsigned(parts, _underlyingType);
                case XamlReflectionType.EnumType.Int64:
                    return ProcessEnumStringSigned(parts, _underlyingType);
                case XamlReflectionType.EnumType.UInt64:
                    return ProcessEnumStringUnsigned(parts, _underlyingType);
            }
        }
        throw new ReflectionHelperException("Couldn't resolve underlying enum type for type '" + _underlyingType.FullName + "'");
    }

    private ulong ProcessEnumStringUnsigned(IEnumerable<string> parts, Type underlyingType)
    {
        ulong num = 0UL;
        foreach (string text in parts)
        {
            object obj = Enum.Parse(underlyingType, text.Trim());
            ulong num2 = Convert.ToUInt64(obj);
            num |= num2;
        }
        return num;
    }

    private long ProcessEnumStringSigned(IEnumerable<string> parts, Type underlyingType)
    {
        long num = 0L;
        foreach (string text in parts)
        {
            object obj = Enum.Parse(underlyingType, text.Trim());
            long num2 = Convert.ToInt64(obj);
            num |= num2;
        }
        return num;
    }

    private bool GetIsConstructible()
    {
        foreach (ConstructorInfo constructorInfo in _info.DeclaredConstructors)
        {
            ParameterInfo[] parameters = constructorInfo.GetParameters();
            if (parameters.Length == 0)
            {
                return true;
            }
        }
        return false;
    }

    private bool HasBindableAttribute()
    {
        return HasAttribute("Windows.UI.Xaml.Data.BindableAttribute");
    }

    private string GetStringNamedArgumentFromAttribute(string attributeTypeName, string attributeTypedArgName)
    {
        foreach (CustomAttributeData customAttributeData in _info.CustomAttributes)
        {
            if (customAttributeData.AttributeType.FullName == attributeTypeName)
            {
                foreach (CustomAttributeNamedArgument customAttributeNamedArgument in customAttributeData.NamedArguments)
                {
                    if (customAttributeNamedArgument.MemberName.Equals(attributeTypedArgName))
                    {
                        return customAttributeNamedArgument.TypedValue.Value as string;
                    }
                }
            }
        }
        return null;
    }

    private bool HasAttribute(string attrName)
    {
        foreach (CustomAttributeData customAttributeData in _info.CustomAttributes)
        {
            if (customAttributeData.AttributeType.FullName == attrName)
            {
                return true;
            }
        }
        return false;
    }

    // Note: this type is marked as 'beforefieldinit'.
    static XamlReflectionType()
    {
    }

    private static TypeInfo iListTypeInfo = IntrospectionExtensions.GetTypeInfo(typeof(IList));

    private static TypeInfo iDictionaryTypeInfo = IntrospectionExtensions.GetTypeInfo(typeof(IDictionary));

    private static TypeInfo markupExtTypeInfo = IntrospectionExtensions.GetTypeInfo(typeof(MarkupExtension));

    private static TypeInfo iEnumerableTypeInfo = IntrospectionExtensions.GetTypeInfo(typeof(IEnumerable));

    private const string nullableTypeName = "System.Nullable`1";

    private const string referenceTypeName = "Windows.Foundation.IReference`1";

    private Type _underlyingType;

    private TypeInfo _info;

    private Dictionary<string, IXamlMember> _members;

    private string _fullName;

    private MethodInfo _adderInfo;

    private MethodInfo _createFromStringMethod;

    private bool _gotCreateFromStringMethod;

    private IXamlMember _contentProperty;

    private bool _gotContentProperty;

    private IXamlType _baseType;

    private bool _gotBaseType;

    private IXamlType _boxedType;

    private bool _gotBoxedType;

    private XamlReflectionType.EnumType? _enumType;

    private enum EnumType
    {
        SByte,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64
    }
}
