using System;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Core.Interfaces
{
    [Guid("1f4df06b-6e3b-46ab-9365-55568e176b53"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IApplicationFrameTitleBarPersistenceInternal
    {
        int GetExtendViewIntoTitleBar(out bool value);
        int SetExtendViewIntoTitleBar(bool value);
        int GetColor(PersistedColor colorType, out int colorValue);
        int SetColor(PersistedColor colorType, int colorValue);
    }

    public enum PersistedColor { }
}
