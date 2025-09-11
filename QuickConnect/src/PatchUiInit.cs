using HarmonyLib;

namespace QuickConnect
{
    [HarmonyPatch(typeof(FejdStartup), "SetupGui")]
    class PatchUiInit
    {
        static void Postfix()
        {
            QuickConnectUI.instance.GetInstanceID();
        }
    }
}
