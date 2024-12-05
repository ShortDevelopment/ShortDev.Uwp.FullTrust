using System.Diagnostics;
using System.Reflection;
using Windows.UI.Xaml.Markup;

namespace Microsoft.UI.Xaml.Markup;

[DebuggerNonUserCode]
internal sealed partial class XamlReflectionMember : IXamlMember
{
    public static XamlReflectionMember Create(string memberName, Type declaringType)
    {
        bool isDependencyProperty = false;
        bool isReadOnly = false;
        Type type = null;
        bool isAttachable = false;
        Type targetType = null;
        MethodInfo attachableGetterInfo = null;
        MethodInfo attachableSetterInfo = null;
        PropertyInfo runtimeProperty = declaringType.GetRuntimeProperty(memberName);
        if (runtimeProperty != null)
        {
            type = runtimeProperty.PropertyType;
            isReadOnly = !runtimeProperty.CanWrite;
        }
        PropertyInfo staticProperty = declaringType.GetStaticProperty(memberName + "Property");
        if (staticProperty != null)
        {
            isDependencyProperty = true;
        }
        else
        {
            FieldInfo staticField = declaringType.GetStaticField(memberName + "Property");
            if (staticField != null)
            {
                isDependencyProperty = true;
            }
        }
        bool flag = false;
        bool flag2 = false;
        Type type2 = null;
        Type type3 = null;
        IEnumerable<MethodInfo> runtimeMethods = RuntimeReflectionExtensions.GetRuntimeMethods(declaringType);
        foreach (MethodInfo methodInfo in runtimeMethods)
        {
            if (flag && flag2)
            {
                break;
            }
            if (methodInfo.IsStatic && methodInfo.IsPublic)
            {
                if (methodInfo.Name.Equals("Set" + memberName))
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == 2)
                    {
                        if (type == null)
                        {
                            type = parameters[1].ParameterType;
                        }
                        type3 = parameters[0].ParameterType;
                        flag2 = true;
                        attachableSetterInfo = methodInfo;
                    }
                }
                else if (methodInfo.Name.Equals("Get" + memberName))
                {
                    ParameterInfo[] parameters2 = methodInfo.GetParameters();
                    if (parameters2.Length == 1)
                    {
                        type ??= methodInfo.ReturnType;

                        type2 = parameters2[0].ParameterType;
                        flag = true;
                        attachableGetterInfo = methodInfo;
                    }
                }
            }
        }
        if (flag || flag2)
        {
            isAttachable = true;
            isReadOnly = !flag2;
            if (flag)
            {
                targetType = type2;
            }
            else if (flag2)
            {
                targetType = type3;
            }
        }

        if (type == null)
            return null;

        return new XamlReflectionMember(memberName, isDependencyProperty, isReadOnly, type, declaringType, isAttachable, targetType, attachableGetterInfo, attachableSetterInfo);
    }

    public string Name { get; }
    public bool IsAttachable { get; }
    public bool IsDependencyProperty { get; }
    public bool IsReadOnly { get; }

    readonly Type _underlyingType;
    readonly Type _declaringType;
    readonly Type _targetType;
    readonly MethodInfo _attachableGetterInfo;
    readonly MethodInfo _attachableSetterInfo;

    XamlReflectionMember(string memberName, bool isDependencyProperty, bool isReadOnly, Type underlyingType, Type declaringType, bool isAttachable, Type targetType, MethodInfo attachableGetterInfo, MethodInfo attachableSetterInfo)
    {
        Name = memberName;
        IsDependencyProperty = isDependencyProperty;
        IsReadOnly = isReadOnly;
        _underlyingType = underlyingType;
        _declaringType = declaringType;
        IsAttachable = isAttachable;
        _targetType = targetType;
        _attachableGetterInfo = attachableGetterInfo;
        _attachableSetterInfo = attachableSetterInfo;
    }

    public IXamlType Type
        => ReflectionXamlMetadataProvider.GetXamlTypeInternal(_underlyingType);

    public IXamlType TargetType
        => ReflectionXamlMetadataProvider.GetXamlTypeInternal(_targetType);

    public object GetValue(object instance)
    {
        if (!IsAttachable)
        {
            PropertyInfo runtimeProperty = RuntimeReflectionExtensions.GetRuntimeProperty(_declaringType, Name);
            object obj = runtimeProperty.GetValue(instance);
            if (obj == null && runtimeProperty.PropertyType.Equals(typeof(string)))
            {
                obj = string.Empty;
            }
            return obj;
        }
        return _attachableGetterInfo.Invoke(null, new object[]
        {
            instance
        });
    }

    public void SetValue(object instance, object value)
    {
        if (!IsAttachable)
        {
            PropertyInfo runtimeProperty = RuntimeReflectionExtensions.GetRuntimeProperty(_declaringType, Name);
            if (value == null && runtimeProperty.PropertyType.Equals(typeof(string)))
            {
                value = string.Empty;
            }
            runtimeProperty.SetValue(instance, value);
            return;
        }
        if (!IsReadOnly)
        {
            _attachableSetterInfo.Invoke(null, new object[]
            {
                instance,
                value
            });
            return;
        }
        throw new ReflectionHelperException(string.Concat(new string[]
        {
            "Attempted to write to read-only attachable property '",
            _declaringType.FullName,
            ".",
            Name,
            "'"
        }));
    }
}
