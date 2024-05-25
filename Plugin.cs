using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace LunacidQoL
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("LUNACID.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private List<GameObject> _projectiles = new();
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo("Lunacid QoL Mod has loaded!");
            On.Item_Emit.Start += (orig, self) =>
            {
                orig(self);
                var anchor = new GameObject("Anchor");
                var collider = anchor.AddComponent<SphereCollider>();
                collider.radius = 0.025f;
                collider.center = Vector3.zero;

                var rb = self.gameObject.GetComponent<Rigidbody>();
                rb.detectCollisions = true;
                rb.interpolation = RigidbodyInterpolation.Extrapolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                anchor.AddComponent<CollideOnce>();
                
                anchor.transform.position = self.gameObject.transform.position;
                anchor.transform.parent = self.gameObject.transform;

                _projectiles.Add(gameObject);
            };

            On.Damage_Trigger.Die += (orig, self, obj) =>
            {
                _projectiles.RemoveAll(go => go == null);

                if (_projectiles.Contains(self.gameObject))
                {
                    var anchorGo = self.gameObject.transform.Find("Anchor");
                    Destroy(anchorGo);
                }
                
                orig(self, obj);
            };

            On.Player_Control_scr.OnEnable += (orig, self) =>
            {
                orig(self);
                self.CAM2.GetComponent<Effect_scr>().Effect_mat.SetFloat("_GAMMA", self.CON.CURRENT_SYS_DATA.SETT_GAMMA);
            };
        }
    }
}
