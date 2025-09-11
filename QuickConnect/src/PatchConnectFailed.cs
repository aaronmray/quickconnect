using HarmonyLib;

namespace QuickConnect
{
    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    class PatchConnectFailed
    {
        static void Postfix()
        {
            if (ZNet.GetConnectionStatus() == ZNet.ConnectionStatus.ErrorConnectFailed)
            {
                QuickConnectUI.instance.JoinServerFailed();
            }
        }
    }
}
