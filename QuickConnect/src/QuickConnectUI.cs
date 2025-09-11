using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace QuickConnect
{
    class QuickConnectUI : MonoBehaviour
    {
        private static QuickConnectUI _instance;

        public static QuickConnectUI instance
        {
            get
            {
                if (_instance == null)
                {
                    Servers.Init();
                    var go = new GameObject("QuickConnect");
                    _instance = go.AddComponent<QuickConnectUI>();
                }
                return _instance;
            }
        }

        public Rect windowRect, lastRect, errorWinRect;
        private Rect dragRect = new Rect(0, 0, 10000, 20);
        private int buttonFontSize;
        private int labelFontSize;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        public bool redraw = true;
        private enum windowState
        {
            ServerList,
            Connecting,
            NoServers
        }

        private windowState state = windowState.ServerList;

        private Task<IPHostEntry> resolveTask;
        public static Servers.Entry connecting;
        private static string errorMsg;

        void Update()
        {
            if (resolveTask != null)
            {
                if (resolveTask.IsFaulted)
                {
                    Mod.Log.LogError($"Error resolving IP: {resolveTask.Exception}");
                    if (resolveTask.Exception is AggregateException)
                    {
                        ShowError((resolveTask.Exception as AggregateException).InnerException.Message);
                    }
                    else
                    {
                        ShowError(resolveTask.Exception.Message);
                    }
                    resolveTask = null;
                    connecting = null;
                }
                else if (resolveTask.IsCanceled)
                {
                    resolveTask = null;
                    connecting = null;
                }
                else if (resolveTask.IsCompleted)
                {
                    foreach (var addr in resolveTask.Result.AddressList)
                    {
                        if (addr.AddressFamily == AddressFamily.InterNetwork)
                        {
                            Mod.Log.LogInfo($"Resolved: {addr}");
                            resolveTask = null;
                            ZSteamMatchmaking.instance.QueueServerJoin($"{addr}:{connecting.port}");
                            return;
                        }
                    }
                    resolveTask = null;
                    connecting = null;
                    ShowError("Server DNS resolved to no valid addresses");
                }
            }
        }

        void Draw()
        {
            windowRect.x = Mod.windowPosX.Value;
            windowRect.y = Mod.windowPosY.Value;

            buttonFontSize = Mod.buttonFontSize.Value;
            labelFontSize = Mod.labelFontSize.Value;

            windowRect.width = Mod.windowWidth.Value;
            windowRect.height = Mod.windowHeight.Value;
            lastRect = windowRect;
        }

        void Awake()
        {
            Draw();
        }

        void OnGUI()
        {
            if (buttonStyle == null)
                buttonStyle = new GUIStyle(GUI.skin.button);

            if (labelStyle == null)
                labelStyle = new GUIStyle(GUI.skin.label);

            if (errorMsg != null)
            {
                errorWinRect = GUILayout.Window(1586464, errorWinRect, DrawErrorWindow, "Error");
            }
            else
            {
                windowRect = GUILayout.Window(1586463, windowRect, DrawConnectWindow, "Quick Connect");
                if (!lastRect.Equals(windowRect))
                {
                    Mod.windowPosX.Value = windowRect.x;
                    Mod.windowPosY.Value = windowRect.y;
                    lastRect = windowRect;
                }
                else if (redraw)
                {
                    Draw();
                    buttonStyle.fontSize = buttonFontSize;
                    labelStyle.fontSize = labelFontSize;
                    redraw = false;
                }
            }
        }

        void DrawConnectWindow(int windowID)
        {
            GUI.DragWindow(dragRect);
            if (connecting != null)
            {
                CheckWindowState(windowState.Connecting);

                GUILayout.Label("Connecting to " + connecting.name, labelStyle);
                if (GUILayout.Button("Cancel", buttonStyle))
                {
                    AbortConnect();
                }
            }
            else if (Servers.entries.Count > 0)
            {
                CheckWindowState(windowState.ServerList);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Choose A Server:", labelStyle);
                if (GUILayout.Button("Reload from Disk", buttonStyle))
                {
                    Servers.Init();
                    DrawConnectWindow(windowID);
                }
                GUILayout.EndHorizontal();
                foreach (var ent in Servers.entries)
                {
                    if (GUILayout.Button(ent.name, buttonStyle))
                    {
                        DoConnect(ent);
                    }
                }
            }
            else
            {
                CheckWindowState(windowState.NoServers);

                GUILayout.BeginHorizontal();
                GUILayout.Label("No servers defined", labelStyle);
                if (GUILayout.Button("Reload from Disk", buttonStyle))
                {
                    Servers.Init();
                    DrawConnectWindow(windowID);
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("Add/modify quick_connect_servers.cfg", labelStyle);
            }
        }

        void DrawErrorWindow(int windowID)
        {
            GUILayout.Label(errorMsg, labelStyle);
            if (GUILayout.Button("Close", buttonStyle))
            {
                errorMsg = null;
            }
        }

        private void DoConnect(Servers.Entry server)
        {
            connecting = server;
            try
            {
                IPAddress.Parse(server.ip);
                ZSteamMatchmaking.instance.QueueServerJoin($"{server.ip}:{server.port}");
            }
            catch (FormatException)
            {
                Mod.Log.LogInfo($"Resolving: {server.ip}");
                resolveTask = Dns.GetHostEntryAsync(server.ip);
            }
        }

        public static string CurrentPass()
        {
            if (connecting != null)
                return connecting.pass;
            return null;
        }

        public void JoinServerFailed()
        {
            if (Mod.customConnectionError.Value)
            {
                ShowError("Server connection failed");
            }
            connecting = null;
        }

        public void ShowError(string msg)
        {
            errorMsg = msg;
            errorWinRect = new Rect(Screen.width / 2 - 125, Screen.height / 2, 250, 30);
        }

        public void AbortConnect()
        {
            connecting = null;
            resolveTask = null;
        }

        private void CheckWindowState(windowState state)
        {
            if (this.state != state)
            {
                this.state = state;
                Draw();
            }
        }
    }
}
