using HarmonyLib;

namespace QuickConnect
{
    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    class PatchConnectFailed
    {
        static void Postfix()
        {
            if (Mod.customConnectionError.Value && ZNet.GetConnectionStatus() == ZNet.ConnectionStatus.ErrorConnectFailed)
            {
                QuickConnectUI.instance.JoinServerFailed();
            }
        }
    }
}
