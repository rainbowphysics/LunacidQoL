using System;
using BepInEx;
using UnityEngine;

namespace LunacidQoL
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("LUNACID.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo("Lunacid QoL Mod has loaded!");
            On.Item_Emit.Start += (orig, self) =>
            {
                orig(self);
                
                var raycheck = self.gameObject.AddComponent<Raycheck>();
                raycheck.Logger = Logger;
            };

            On.Player_Control_scr.OnEnable += (orig, self) =>
            {
                orig(self);
                self.CAM2.GetComponent<Effect_scr>().Effect_mat.SetFloat("_GAMMA", self.CON.CURRENT_SYS_DATA.SETT_GAMMA);
            };
        }
    }
}
