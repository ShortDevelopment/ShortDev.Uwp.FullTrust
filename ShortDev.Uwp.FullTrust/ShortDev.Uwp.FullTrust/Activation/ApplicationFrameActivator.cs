using ShortDev.Uwp.FullTrust.Interfaces;

namespace ShortDev.Uwp.FullTrust.Activation
{
    public static class ApplicationFrameActivator
    {
        public static IApplicationFrameManager CreateApplicationFrameManager()
        {
            //const string CLSID_ApplicationFrameManagerPriv = "ddc05a5a-351a-4e06-8eaf-54ec1bc2dcea";
            //return InteropHelper.ComCreateInstance<IApplicationFrameManager>(CLSID_ApplicationFrameManagerPriv)!;

            const string CLSID_ApplicationFrameManager = "b9b05098-3e30-483f-87f7-027ca78da287";
            return InteropHelper.ComCreateInstance<IApplicationFrameManager>(CLSID_ApplicationFrameManager)!;
        }
    }
}
