using BepInEx;
using UnityEngine;

namespace LunacidQoL
{
    [BepInPlugin("org.bepinex.plugins.lunacidqol", "Lunacid QoL", "0.1.0")]
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
        }
    }
}
