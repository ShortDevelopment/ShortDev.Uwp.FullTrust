using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.InteropServices.ComTypes;

namespace ShortDev.Uwp.FullTrust.Core
{

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020400-0000-0000-C000-000000000046")]
    public interface IDispatch
    {
        [Obsolete]
        void GetTypeInfoCount();

        [Obsolete]
        void GetTypeInfo();

        [PreserveSig]
        int GetIDsOfNames([In] ref Guid riid, [In, MarshalAs(UnmanagedType.LPArray)] string[] rgszNames, [In, MarshalAs(UnmanagedType.U4)] int cNames, [In, MarshalAs(UnmanagedType.U4)] int lcid, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);

        [PreserveSig]
        int Invoke(int dispIdMember, ref Guid riid, int lcid, IDispatchInvokeFlags wFlags, ref DISPPARAMS pDispParams, out Variant pVarResult, out IntPtr pExcepInfo, out uint puArgErr);

    }

    [Flags]
    public enum IDispatchInvokeFlags : short
    {
        DISPATCH_METHOD = 1,
        DISPATCH_PROPERTYGET = 2,
        DISPATCH_PROPERTYPUT = 4,
        DISPATCH_PROPERTYPUTREF = 8
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct Variant
    {
#if DEBUG
        static Variant()
        {
            // Variant size is the size of 4 pointers (16 bytes) on a 32-bit processor,
            // and 3 pointers (24 bytes) on a 64-bit processor.
            int variantSize = Marshal.SizeOf<Variant>();
            if (IntPtr.Size == 4)
            {
                Debug.Assert(variantSize == (4 * IntPtr.Size));
            }
            else
            {
                Debug.Assert(IntPtr.Size == 8);
                Debug.Assert(variantSize == (3 * IntPtr.Size));
            }
        }
#endif

        // Most of the data types in the Variant are carried in _typeUnion
        [FieldOffset(0)] private TypeUnion _typeUnion;

        // Decimal is the largest data type and it needs to use the space that is normally unused in TypeUnion._wReserved1, etc.
        // Hence, it is declared to completely overlap with TypeUnion. A Decimal does not use the first two bytes, and so
        // TypeUnion._vt can still be used to encode the type.
        [FieldOffset(0)] private decimal _decimal;

        [StructLayout(LayoutKind.Sequential)]
        private struct TypeUnion
        {
            public ushort _vt;
            public ushort _wReserved1;
            public ushort _wReserved2;
            public ushort _wReserved3;

            public UnionTypes _unionTypes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Record
        {
            public IntPtr _record;
            public IntPtr _recordInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UnionTypes
        {
            [FieldOffset(0)] public sbyte _i1;
            [FieldOffset(0)] public short _i2;
            [FieldOffset(0)] public int _i4;
            [FieldOffset(0)] public long _i8;
            [FieldOffset(0)] public byte _ui1;
            [FieldOffset(0)] public ushort _ui2;
            [FieldOffset(0)] public uint _ui4;
            [FieldOffset(0)] public ulong _ui8;
            [FieldOffset(0)] public int _int;
            [FieldOffset(0)] public uint _uint;
            [FieldOffset(0)] public short _bool;
            [FieldOffset(0)] public int _error;
            [FieldOffset(0)] public float _r4;
            [FieldOffset(0)] public double _r8;
            [FieldOffset(0)] public long _cy;
            [FieldOffset(0)] public double _date;
            [FieldOffset(0)] public IntPtr _bstr;
            [FieldOffset(0)] public IntPtr _unknown;
            [FieldOffset(0)] public IntPtr _dispatch;
            [FieldOffset(0)] public IntPtr _pvarVal;
            [FieldOffset(0)] public IntPtr _byref;
            [FieldOffset(0)] public Record _record;
        }

        /// <summary>
        /// Primitive types are the basic COM types. It includes valuetypes like ints, but also reference types
        /// like BStrs. It does not include composite types like arrays and user-defined COM types (IUnknown/IDispatch).
        /// </summary>
        public static bool IsPrimitiveType(VarEnum varEnum)
        {
            switch (varEnum)
            {
                case VarEnum.VT_I1:
                case VarEnum.VT_I2:
                case VarEnum.VT_I4:
                case VarEnum.VT_I8:
                case VarEnum.VT_UI1:
                case VarEnum.VT_UI2:
                case VarEnum.VT_UI4:
                case VarEnum.VT_UI8:
                case VarEnum.VT_INT:
                case VarEnum.VT_UINT:
                case VarEnum.VT_BOOL:
                case VarEnum.VT_ERROR:
                case VarEnum.VT_R4:
                case VarEnum.VT_R8:
                case VarEnum.VT_DECIMAL:
                case VarEnum.VT_CY:
                case VarEnum.VT_DATE:
                case VarEnum.VT_BSTR:
                    return true;
            }

            return false;
        }

        public unsafe void CopyFromIndirect(object value)
        {
            VarEnum vt = (VarEnum)(((int)this.VariantType) & ~((int)VarEnum.VT_BYREF));

            if (value == null)
            {
                if (vt == VarEnum.VT_DISPATCH || vt == VarEnum.VT_UNKNOWN || vt == VarEnum.VT_BSTR)
                {
                    *(IntPtr*)this._typeUnion._unionTypes._byref = IntPtr.Zero;
                }
                return;
            }

            if ((vt & VarEnum.VT_ARRAY) != 0)
            {
                Variant vArray;
                Marshal.GetNativeVariantForObject(value, (IntPtr)(void*)&vArray);
                *(IntPtr*)this._typeUnion._unionTypes._byref = vArray._typeUnion._unionTypes._byref;
                return;
            }

            switch (vt)
            {
                case VarEnum.VT_I1:
                    *(sbyte*)this._typeUnion._unionTypes._byref = (sbyte)value;
                    break;

                case VarEnum.VT_UI1:
                    *(byte*)this._typeUnion._unionTypes._byref = (byte)value;
                    break;

                case VarEnum.VT_I2:
                    *(short*)this._typeUnion._unionTypes._byref = (short)value;
                    break;

                case VarEnum.VT_UI2:
                    *(ushort*)this._typeUnion._unionTypes._byref = (ushort)value;
                    break;

                case VarEnum.VT_BOOL:
                    // VARIANT_TRUE  = -1
                    // VARIANT_FALSE = 0
                    *(short*)this._typeUnion._unionTypes._byref = (bool)value ? (short)-1 : (short)0;
                    break;

                case VarEnum.VT_I4:
                case VarEnum.VT_INT:
                    *(int*)this._typeUnion._unionTypes._byref = (int)value;
                    break;

                case VarEnum.VT_UI4:
                case VarEnum.VT_UINT:
                    *(uint*)this._typeUnion._unionTypes._byref = (uint)value;
                    break;

                case VarEnum.VT_ERROR:
                    *(int*)this._typeUnion._unionTypes._byref = ((ErrorWrapper)value).ErrorCode;
                    break;

                case VarEnum.VT_I8:
                    *(long*)this._typeUnion._unionTypes._byref = (long)value;
                    break;

                case VarEnum.VT_UI8:
                    *(ulong*)this._typeUnion._unionTypes._byref = (ulong)value;
                    break;

                case VarEnum.VT_R4:
                    *(float*)this._typeUnion._unionTypes._byref = (float)value;
                    break;

                case VarEnum.VT_R8:
                    *(double*)this._typeUnion._unionTypes._byref = (double)value;
                    break;

                case VarEnum.VT_DATE:
                    *(double*)this._typeUnion._unionTypes._byref = ((DateTime)value).ToOADate();
                    break;

                case VarEnum.VT_UNKNOWN:
                    *(IntPtr*)this._typeUnion._unionTypes._byref = Marshal.GetIUnknownForObject(value);
                    break;

                case VarEnum.VT_BSTR:
                    *(IntPtr*)this._typeUnion._unionTypes._byref = Marshal.StringToBSTR((string)value);
                    break;

                case VarEnum.VT_CY:
                    *(long*)this._typeUnion._unionTypes._byref = decimal.ToOACurrency((decimal)value);
                    break;

                case VarEnum.VT_DECIMAL:
                    *(decimal*)this._typeUnion._unionTypes._byref = (decimal)value;
                    break;

                case VarEnum.VT_VARIANT:
                    Marshal.GetNativeVariantForObject(value, this._typeUnion._unionTypes._byref);
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Get the managed object representing the Variant.
        /// </summary>
        /// <returns></returns>
        public object? ToObject()
        {
            // Check the simple case upfront
            if (IsEmpty)
            {
                return null;
            }

            switch (VariantType)
            {
                case VarEnum.VT_NULL:
                    return DBNull.Value;

                case VarEnum.VT_I1: return AsI1;
                case VarEnum.VT_I2: return AsI2;
                case VarEnum.VT_I4: return AsI4;
                case VarEnum.VT_I8: return AsI8;
                case VarEnum.VT_UI1: return AsUi1;
                case VarEnum.VT_UI2: return AsUi2;
                case VarEnum.VT_UI4: return AsUi4;
                case VarEnum.VT_UI8: return AsUi8;
                case VarEnum.VT_INT: return AsInt;
                case VarEnum.VT_UINT: return AsUint;
                case VarEnum.VT_BOOL: return AsBool;
                case VarEnum.VT_ERROR: return AsError;
                case VarEnum.VT_R4: return AsR4;
                case VarEnum.VT_R8: return AsR8;
                case VarEnum.VT_DECIMAL: return AsDecimal;
                case VarEnum.VT_CY: return AsCy;
                case VarEnum.VT_DATE: return AsDate;
                case VarEnum.VT_BSTR: return AsBstr;
                case VarEnum.VT_UNKNOWN: return AsUnknown;

                default:
                    unsafe
                    {
                        fixed (void* pThis = &this)
                        {
                            return Marshal.GetObjectForNativeVariant((System.IntPtr)pThis);
                        }
                    }
            }
        }

        public VarEnum VariantType
        {
            get => (VarEnum)_typeUnion._vt;
            set => _typeUnion._vt = (ushort)value;
        }

        public bool IsEmpty => _typeUnion._vt == ((ushort)VarEnum.VT_EMPTY);

        public bool IsByRef => (_typeUnion._vt & ((ushort)VarEnum.VT_BYREF)) != 0;

        public void SetAsNULL()
        {
            Debug.Assert(IsEmpty);
            VariantType = VarEnum.VT_NULL;
        }

        // VT_I1

        public sbyte AsI1
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_I1);
                return _typeUnion._unionTypes._i1;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_I1;
                _typeUnion._unionTypes._i1 = value;
            }
        }

        // VT_I2

        public short AsI2
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_I2);
                return _typeUnion._unionTypes._i2;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_I2;
                _typeUnion._unionTypes._i2 = value;
            }
        }

        // VT_I4

        public int AsI4
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_I4);
                return _typeUnion._unionTypes._i4;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_I4;
                _typeUnion._unionTypes._i4 = value;
            }
        }

        // VT_I8

        public long AsI8
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_I8);
                return _typeUnion._unionTypes._i8;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_I8;
                _typeUnion._unionTypes._i8 = value;
            }
        }

        // VT_UI1

        public byte AsUi1
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_UI1);
                return _typeUnion._unionTypes._ui1;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_UI1;
                _typeUnion._unionTypes._ui1 = value;
            }
        }

        // VT_UI2

        public ushort AsUi2
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_UI2);
                return _typeUnion._unionTypes._ui2;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_UI2;
                _typeUnion._unionTypes._ui2 = value;
            }
        }

        // VT_UI4

        public uint AsUi4
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_UI4);
                return _typeUnion._unionTypes._ui4;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_UI4;
                _typeUnion._unionTypes._ui4 = value;
            }
        }

        // VT_UI8

        public ulong AsUi8
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_UI8);
                return _typeUnion._unionTypes._ui8;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_UI8;
                _typeUnion._unionTypes._ui8 = value;
            }
        }

        // VT_INT

        public int AsInt
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_INT);
                return _typeUnion._unionTypes._int;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_INT;
                _typeUnion._unionTypes._int = value;
            }
        }

        // VT_UINT

        public uint AsUint
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_UINT);
                return _typeUnion._unionTypes._uint;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_UINT;
                _typeUnion._unionTypes._uint = value;
            }
        }

        // VT_BOOL

        public bool AsBool
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_BOOL);
                return _typeUnion._unionTypes._bool != 0;
            }
            set
            {
                Debug.Assert(IsEmpty);
                // VARIANT_TRUE  = -1
                // VARIANT_FALSE = 0
                VariantType = VarEnum.VT_BOOL;
                _typeUnion._unionTypes._bool = value ? (short)-1 : (short)0;
            }
        }

        // VT_ERROR

        public int AsError
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_ERROR);
                return _typeUnion._unionTypes._error;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_ERROR;
                _typeUnion._unionTypes._error = value;
            }
        }

        // VT_R4

        public float AsR4
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_R4);
                return _typeUnion._unionTypes._r4;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_R4;
                _typeUnion._unionTypes._r4 = value;
            }
        }

        // VT_R8

        public double AsR8
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_R8);
                return _typeUnion._unionTypes._r8;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_R8;
                _typeUnion._unionTypes._r8 = value;
            }
        }

        // VT_DECIMAL

        public decimal AsDecimal
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_DECIMAL);
                // The first byte of Decimal is unused, but usually set to 0
                Variant v = this;
                v._typeUnion._vt = 0;
                return v._decimal;
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_DECIMAL;
                _decimal = value;
                // _vt overlaps with _decimal, and should be set after setting _decimal
                _typeUnion._vt = (ushort)VarEnum.VT_DECIMAL;
            }
        }

        // VT_CY

        public decimal AsCy
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_CY);
                return decimal.FromOACurrency(_typeUnion._unionTypes._cy);
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_CY;
                _typeUnion._unionTypes._cy = decimal.ToOACurrency(value);
            }
        }

        // VT_DATE

        public DateTime AsDate
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_DATE);
                return DateTime.FromOADate(_typeUnion._unionTypes._date);
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_DATE;
                _typeUnion._unionTypes._date = value.ToOADate();
            }
        }

        // VT_BSTR

        public string? AsBstr
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_BSTR);
                if (_typeUnion._unionTypes._bstr == IntPtr.Zero)
                {
                    return null;
                }
                return (string)Marshal.PtrToStringBSTR(this._typeUnion._unionTypes._bstr);
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_BSTR;
                this._typeUnion._unionTypes._bstr = Marshal.StringToBSTR(value);
            }
        }

        // VT_UNKNOWN

        public object? AsUnknown
        {
            get
            {
                Debug.Assert(VariantType == VarEnum.VT_UNKNOWN);
                if (_typeUnion._unionTypes._unknown == IntPtr.Zero)
                {
                    return null;
                }
                return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._unknown);
            }
            set
            {
                Debug.Assert(IsEmpty);
                VariantType = VarEnum.VT_UNKNOWN;
                if (value == null)
                {
                    _typeUnion._unionTypes._unknown = IntPtr.Zero;
                }
                else
                {
                    _typeUnion._unionTypes._unknown = Marshal.GetIUnknownForObject(value);
                }
            }
        }

        // VT_DISPATCH


        public IntPtr AsByRefVariant
        {
            get
            {
                Debug.Assert(VariantType == (VarEnum.VT_BYREF | VarEnum.VT_VARIANT));
                return _typeUnion._unionTypes._pvarVal;
            }
        }
    }


    public sealed class RemoteThread
    {
        #region standard imports from kernel32
        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(
          IntPtr hProcess,
          IntPtr lpThreadAttributes,
          uint dwStackSize,
          IntPtr lpStartAddress,
          IntPtr lpParameter,
          uint dwCreationFlags,
          out uint lpThreadId
        );

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        const uint PROCESS_ALL_ACCESS = 0x000F0000 | 0x00100000 | 0xFFF;
        [DllImport("kernel32")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(
             IntPtr hProcess,
             IntPtr lpBaseAddress,
             byte[] lpBuffer,
             Int32 nSize,
             out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern
        uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out int lpExitCode);

        #endregion // standard imports from kernel32

        static void WaitForThreadToExit(IntPtr hThread)
        {
            WaitForSingleObject(hThread, unchecked((uint)-1));
            GetExitCodeThread(hThread, out _);
        }

        public static void StartServer()
        {
            uint pid = (uint)Process.GetProcessesByName("ApplicationFrameHost")[0].Id;
            LoadLibraryRemote(pid, @"D:\Programmieren\Visual Studio Projects\ShortDev\ShortDev.Uwp.FullTrust\x64\Debug\UncloakHelper.dll");
            CallFunctionRemote(pid, "UncloakHelper.dll", "StartServer", IntPtr.Zero);
        }

        private static void LoadLibraryRemote(uint pid, string dllName)
        {
            IntPtr hModule = GetModuleHandle("kernel32.dll");
            IntPtr hProc = GetProcAddress(hModule, "LoadLibraryA");
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, pid);

            LoadLibrary(dllName);

            IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllName.Length + 1) * sizeof(char)), AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ReadWrite);

            WriteProcessMemory(hProcess, allocMemAddress, Encoding.Default.GetBytes(dllName), (dllName.Length + 1) * sizeof(char), out _);

            IntPtr hThread = CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                hProc, allocMemAddress,
                0,
                out _);
            // WaitForThreadToExit(hThread);
            CloseHandle(hThread);
            CloseHandle(hProcess);
        }

        public static void CallFunctionRemote(uint pid, string library, string functionName, IntPtr arg)
        {
            IntPtr hModule = GetModuleHandle(library);
            IntPtr hProc = GetProcAddress(hModule, functionName);
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, pid);

            IntPtr hThread = CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                hProc, arg,
                0,
                out _);
            WaitForThreadToExit(hThread);
            CloseHandle(hThread);
            CloseHandle(hProcess);
        }
    }
}