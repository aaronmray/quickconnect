using HarmonyLib;
using System.Reflection;

namespace QuickConnect
{
    [HarmonyPatch(typeof(ZNet), "RPC_ClientHandshake")]
    class PatchPasswordPrompt
    {
        static bool Prefix(ZNet __instance, ZRpc rpc, bool needPassword, string serverPasswordSalt)
        {
            string currentPass = QuickConnectUI.CurrentPass();
            if (currentPass != null)
            {
                if (needPassword)
                {
                    Mod.Log.LogInfo("Authenticating with saved password...");
                    __instance.m_connectingDialog.gameObject.SetActive(false);
                    FieldInfo saltField = typeof(ZNet).GetField("m_serverPasswordSalt", BindingFlags.NonPublic | BindingFlags.Static);
                    saltField.SetValue(null, serverPasswordSalt);
                    MethodInfo sendPeerMethod = typeof(ZNet).GetMethod("SendPeerInfo", BindingFlags.NonPublic | BindingFlags.Instance);
                    sendPeerMethod.Invoke(__instance, new object[] { rpc, currentPass });
                    QuickConnectUI.connecting = null;
                    return false;
                }
                Mod.Log.LogInfo("Server didn't want password?");
            }
            return true;
        }
    }
}
