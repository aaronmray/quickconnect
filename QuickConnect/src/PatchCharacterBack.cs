using HarmonyLib;

namespace QuickConnect
{
    [HarmonyPatch(typeof(FejdStartup), "OnSelelectCharacterBack")]
    class PatchCharacterBack
    {
        static void Postfix()
        {
            QuickConnectUI.instance.AbortConnect();
        }
    }
}
