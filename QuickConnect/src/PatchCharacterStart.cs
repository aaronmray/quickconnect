using HarmonyLib;
using UnityEngine;

namespace QuickConnect
{
    [HarmonyPatch(typeof(FejdStartup), "OnCharacterStart")]
    class PatchCharacterStart
    {
        static void Postfix()
        {
            GameObject.Destroy(QuickConnectUI.instance);
        }
    }
}
