using System;

namespace Microsoft.UI.Xaml.Markup;

// Token: 0x02000006 RID: 6
internal sealed class ReflectionHelperException : Exception
{
	// Token: 0x06000041 RID: 65 RVA: 0x00003A2C File Offset: 0x00001C2C
	public ReflectionHelperException(string message) : base(ReflectionHelperException.ReflectionExceptionMessage + ".  " + message)
	{
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00003A44 File Offset: 0x00001C44
	// Note: this type is marked as 'beforefieldinit'.
	static ReflectionHelperException()
	{
	}

	// Token: 0x0400002F RID: 47
	private static string ReflectionExceptionMessage = "Error in reflection helper.  Please add '<PropertyGroup><EnableTypeInfoReflection>false</EnableTypeInfoReflection></PropertyGroup>' to your project file.";
}
