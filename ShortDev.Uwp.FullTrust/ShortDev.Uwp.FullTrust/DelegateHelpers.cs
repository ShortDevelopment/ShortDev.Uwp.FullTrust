using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ShortDev.Uwp.FullTrust
{
    internal static partial class DelegateHelpers
    {
        private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
        private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
        private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };

        static ModuleBuilder _moduleBuilder;
        static DelegateHelpers()
        {
            AssemblyName assemblyName = new("ShortDev.DynamicDeleagtes");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        public static Delegate CreateDelegate(MethodInfo method)
        {
            Type? type = CreateDelegateType(method);
            if (type == null)
                throw new NullReferenceException();

            return Delegate.CreateDelegate(type, method);
        }

        public static Delegate CreateDelegate(object target, MethodInfo method)
        {
            Type? type = CreateDelegateType(method);
            if (type == null)
                throw new NullReferenceException();

            return Delegate.CreateDelegate(type, target, method);
        }

        public static Type? CreateDelegateType(MethodInfo method)
        {
            Type declaringType = method.DeclaringType;
            Assembly assembly = declaringType.Assembly;

            string name = $"{assembly.FullName.Replace(".", "")}_{method.DeclaringType.FullName.Replace(".", "")}_{method.Name}_Sig";

            return CreateDelegateType(
                name,
                method.GetParameters().Select((x) => x.ParameterType).ToArray(),
                method.ReturnType
            );
        }

        public static Type? CreateDelegateType(string name, Type[] parameters, Type returnType)
        {
            var cachedResult = _moduleBuilder.GetType(name);
            if (cachedResult != null)
                return cachedResult;

            TypeBuilder builder = _moduleBuilder.DefineType(
                name,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                typeof(MulticastDelegate)
            );
            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(ImplAttributes);
            builder.DefineMethod("Invoke", InvokeAttributes, returnType, parameters).SetImplementationFlags(ImplAttributes);
            return builder.CreateTypeInfo();
        }
    }
}
