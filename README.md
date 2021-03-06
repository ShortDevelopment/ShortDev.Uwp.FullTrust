# ShortDev.Uwp.FullTrust

Create highly performant UI with `WinUi2` (Uwp) and enjoy the freedom of `Win32`!    
You don't need any interop code for the `.GetForCurrentView()` methods.   
    
### Problems
 - The library does currently not support any Uwp-Frame related apis
 - Therefore not frame customization
 - Creating a new window is only possible with the library apis
 - Pickers still need `IInitializeWithWindow` (See [#11](https://github.com/ShortDevelopment/ShortDev.Uwp.FullTrust/issues/11))

## Setup

1. Create new Uwp project (*UI-Project*)
2. Add `<EnableTypeInfoReflection>false</EnableTypeInfoReflection>` in to the project file
3. Add reference to this library
---
5. Create new `netcoreapp3.1` `WinExe` project (*Host-Project*)
6. Add reference to `Microsoft.Toolkit.Win32.UI.SDK`
7. Add reference to Uwp project
8. Add `TargetPlatformVersion` and `AssetTargetFallback` to the project file
9. Redirect main method to the Uwp main method (See below)
 
### *Host-Project* (netcoreapp3.1)
This is needed for the management of all the dependency and runtime stuff.   
Should contain no logic.
```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <Platforms>x86;x64</Platforms>
    <AssetTargetFallback>uap10.0.19041</AssetTargetFallback>
    <TargetPlatformVersion>10.0.19041</TargetPlatformVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Toolkit.Win32.UI.SDK" Version="6.1.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="<PATH_TO_UWP_PROJECT>" />
  </ItemGroup>

</Project>
```

#### `Program.cs`
```csharp
public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        NAMESPACE.Program.Main(args);
    }
}
```

### *UI-Project* (Uwp)
Should contain all logic.   
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <EnableTypeInfoReflection>false</EnableTypeInfoReflection>
  </PropertyGroup>
  ...
</Project>
```

#### `Program.cs`
```csharp
public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        FullTrustApplication.Start((_) => new App());
    }
}
```

## Examples
#### Create New Window
```csharp
var view = FullTrustApplication.CreateNewView();
_ = view.CoreWindow.Dispatcher.RunIdleAsync((x) =>
{
    Window.Current.Content = new MainPage();
    Window.Current.Activate();
});
```