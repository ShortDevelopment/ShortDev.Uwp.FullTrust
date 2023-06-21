using Windows.UI;

namespace ApplicationTheme;

public static class AppThemeHelper
{
    public static Color ApplicationColor
    {
        get => AppThemeAPI.GetThemeColor(ThemeAccentColorVariant.ThemeBaseApplication);
        set => AppThemeAPI.SetThemeBaseApplicationColor(value);
    }

    public static Color ApplicationTextColor
    {
        get => AppThemeAPI.GetThemeColor(ThemeAccentColorVariant.ThemeTextApplication);
    }

    public static Color SystemColor
    {
        get => AppThemeAPI.GetThemeColor(ThemeAccentColorVariant.ThemeBaseSystem);
        set => AppThemeAPI.SetThemeBaseSystemColor(value);
    }

    public static Color SystemTextColor
    {
        get => AppThemeAPI.GetThemeColor(ThemeAccentColorVariant.ThemeTextSystem);
    }

    public static Color AccentColor
    {
        get => AppThemeAPI.GetThemeColor(ThemeAccentColorVariant.ThemeAccent);
        set => AppThemeAPI.SetThemeAccentColor(value);
    }

    public static bool AdvancedEffectsEnabled
        => AppThemeAPI.AdvancedEffectsEnabled;
}