using ShortDev.Uwp.FullTrust.Core;
using System;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI;

namespace ApplicationTheme
{
    public static class AppThemeApi
    {
        /// <summary>
        /// Windows.UI.Immersive.dll
        /// </summary>
        [Guid("c5f80e59-a9fc-439d-9fc4-d290858e1867"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        interface IAppThemeApiStatics
        {
            [PreserveSig]
            int SetThemeBaseApplicationColor(Color appColor);

            [PreserveSig]
            int SetThemeBaseSystemColor(Color systemColor);

            [PreserveSig]
            int SetThemeAccentColor(Color accentColor);

            [PreserveSig]
            int GetThemeColor(ThemeAccentColorVariant colorVariant, out Color themeColor);

            event TypedEventHandler<object, object> ThemeColorsChanged;

            bool AdvancedEffectsEnabled { get; }
        }

        static IAppThemeApiStatics _themeApiStatics;
        static AppThemeApi()
        {
            _themeApiStatics = InteropHelper.RoGetActivationFactory<IAppThemeApiStatics>("ApplicationTheme.AppThemeAPI");
        }

        public static Color ApplicationColor
        {
            get
            {
                Marshal.ThrowExceptionForHR(_themeApiStatics.GetThemeColor(ThemeAccentColorVariant.ThemeBaseApplication, out var result));
                return result;
            }
            set => Marshal.ThrowExceptionForHR(_themeApiStatics.SetThemeBaseApplicationColor(value));
        }

        public static Color ApplicationTextColor
        {
            get
            {
                Marshal.ThrowExceptionForHR(_themeApiStatics.GetThemeColor(ThemeAccentColorVariant.ThemeTextApplication, out var result));
                return result;
            }
        }

        public static Color SystemColor
        {
            get
            {
                Marshal.ThrowExceptionForHR(_themeApiStatics.GetThemeColor(ThemeAccentColorVariant.ThemeBaseSystem, out var result));
                return result;
            }
            set => Marshal.ThrowExceptionForHR(_themeApiStatics.SetThemeBaseSystemColor(value));
        }

        public static Color SystemTextColor
        {
            get
            {
                Marshal.ThrowExceptionForHR(_themeApiStatics.GetThemeColor(ThemeAccentColorVariant.ThemeTextSystem, out var result));
                return result;
            }
        }

        public static Color AccentColor
        {
            get
            {
                Marshal.ThrowExceptionForHR(_themeApiStatics.GetThemeColor(ThemeAccentColorVariant.ThemeAccent, out var result));
                return result;
            }
            set => Marshal.ThrowExceptionForHR(_themeApiStatics.SetThemeAccentColor(value));
        }

        public static bool AdvancedEffectsEnabled
            => _themeApiStatics.AdvancedEffectsEnabled;
    }
}
