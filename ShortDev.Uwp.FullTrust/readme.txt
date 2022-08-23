# Changes `0.1.7`

## Deprecation
 - You should use `XamlWindowSubclass.Win32Window` to customize the win32 window frame

## Changed Behavior
 - Window is no longer visible by default   
   You have to set `XamlWindowConfig.IsVisible = true` or call `Window.Activate()`


# Changes `0.1.6`

## Fixes
 - Acrylic HostBackdropBrush is now working

## New Features
 - Expose some internal apis

## Changed Behavior
 - Window is no longer transparent by default