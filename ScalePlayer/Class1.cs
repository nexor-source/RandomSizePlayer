using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

using System;
using System.Security.Cryptography;
using Unity.Netcode;

namespace RandomSizePlayer
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class RandomSizePlayer : BaseUnityPlugin
    {
        private const string modGUID = "nexor.RandomSizePlayer";
        private const string modName = "RandomSizePlayer";
        private const string modVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        public ConfigEntry <float> k1, k2, k3, sharpness;

        public static RandomSizePlayer Instance;
        public static BepInEx.Logging.ManualLogSource Logger;
        public ConfigEntry <bool> random, hide_your_visor;
        public bool ship = false;

        // 在插件启动时会直接调用Awake()方法
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            // 初始化配置项
            k1 = Config.Bind<float>("Random Size Player Config",
                                    "width 宽",
                                    1f,
                                    "if you want everybody to be the exact same size 如果你想让所有玩家都一个大小的话");

            k2 = Config.Bind<float>("Random Size Player Config",
                                    "height 高",
                                    1f,
                                    "if you want everybody to be the exact same size 如果你想让所有玩家都一个大小的话");

            k3 = Config.Bind<float>("Random Size Player Config",
                                    "thickness 厚",
                                    1f,
                                    "if you want everybody to be the exact same size 如果你想让所有玩家都一个大小的话");

            random = Config.Bind<bool>("Random Size Player Config",
                                    "random size? 是否采用随机大小",
                                    false,
                                    "");

            hide_your_visor = Config.Bind<bool>("Random Size Player Config",
                                    "hide you visor? 是否隐藏面罩",
                                    false,
                                    "");

            sharpness = Config.Bind<float>("Random Size Player Config",
                                    "the sharpness of the random size s curve? 随机大小的概率曲线的陡峭程度",
                                    1f,
                                    "The larger the value, the more likely extreme values are to occur, while the smaller the value, the more likely intermediate values are to occur" +
                                    "越大越容易出现极端值, 越小越容易出现中间值");


            Logger = base.Logger;
            harmony.PatchAll();
            Logger.LogInfo("RandomSizePlayer 0.0.1 loaded.");
        }
    }


    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerB_Awake_Patch
    {
        // Token: 0x06000003 RID: 3 RVA: 0x000020DC File Offset: 0x000002DC
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void postfix(PlayerControllerB __instance)
        {
            float x, y, z;

            x = RandomSizePlayer.Instance.k1.Value;
            y = RandomSizePlayer.Instance.k2.Value;
            z = RandomSizePlayer.Instance.k3.Value;

            // 隐藏面罩
            if (RandomSizePlayer.Instance.hide_your_visor.Value)
            {
                __instance.localVisorTargetPoint.position = new Vector3(10f, 10f, 10f);
            }

            // 不随机则取设定值
            if (!RandomSizePlayer.Instance.random.Value)
            {
                __instance.transform.localScale = new Vector3(x, y, z);
            }

            // 随机则使用sigmoid实现偏态分布，更倾向于极端值的生成
            else
            {
                float _x, _y, _z;

                // 分别从区间[-1, 1]中取三个随机数
                _x = UnityEngine.Random.Range(-1f, 1f);
                _y = UnityEngine.Random.Range(-1f, 1f);
                _z = UnityEngine.Random.Range(-1f, 1f);

                x = (1f / (1f + Mathf.Exp(-4f * _x * RandomSizePlayer.Instance.sharpness.Value)) - 0.5f) *2f * 1.6f;
                y = 1f / (1f + Mathf.Exp(-2f * _y * RandomSizePlayer.Instance.sharpness.Value)) * 1.8f;
                z = 1f / (1f + Mathf.Exp(-4f * _z * RandomSizePlayer.Instance.sharpness.Value)) * 1.65f;

                // 将随机的缩放值应用到 __instance.transform.localScale 的每个分量
                __instance.transform.localScale = new Vector3(x, y, z);
            }

            // 高 +速度+手-探索  
            /*__instance.grabDistance = __instance.grabDistance * y * z;
            __instance.movementSpeed = __instance.movementSpeed * y;
            __instance.jumpForce = __instance.jumpForce * y;
            __instance.throwPower = __instance.throwPower;*/

        }
    }


    // __instance.transform.localScale = new Vector3(RandomSizePlayer.Instance.k1.Value, RandomSizePlayer.Instance.k2.Value, RandomSizePlayer.Instance.k3.Value);

    // ___localVisorTargetPoint.position = new Vector3(RandomSizePlayer.Instance.h1.Value, RandomSizePlayer.Instance.h2.Value, RandomSizePlayer.Instance.h3.Value);
    // 宽度，高度，厚度
    // ___thisPlayerBody.localScale = new Vector3(RandomSizePlayer.Instance.k1.Value, RandomSizePlayer.Instance.k2.Value, RandomSizePlayer.Instance.k3.Value);
    // ___playerGlobalHead.localScale = new Vector3(RandomSizePlayer.Instance.h1.Value, RandomSizePlayer.Instance.h2.Value, RandomSizePlayer.Instance.h3.Value);
    // ___usernameBillboard.position = new Vector3(___usernameBillboard.position.x, ___usernameBillboard.position.y + 0.23f, ___usernameBillboard.position.z);
    // ___usernameBillboard.localScale *= 1.5f;
    // ___gameplayCamera.transform.GetChild(0).position = new Vector3(___gameplayCamera.transform.GetChild(0).position.x + RandomSizePlayer.Instance.c1.Value, ___gameplayCamera.transform.GetChild(0).position.y + RandomSizePlayer.Instance.c2.Value, ___gameplayCamera.transform.GetChild(0).position.z + RandomSizePlayer.Instance.c3.Value);
    // ___localVisor.transform.position = new Vector3(RandomSizePlayer.Instance.c1.Value, RandomSizePlayer.Instance.c2.Value, RandomSizePlayer.Instance.c3.Value);




/*    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        // Token: 0x06000003 RID: 3 RVA: 0x000020DC File Offset: 0x000002DC
        [HarmonyPatch("SpawnUnlockable")]
        [HarmonyPrefix]
        private static void prefix()
        {
            RandomSizePlayer.Instance.ship = true;
        }

        [HarmonyPatch("SpawnUnlockable")]
        [HarmonyPostfix]
        private static void postfix()
        {
            RandomSizePlayer.Instance.ship = false;
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void postfix2()
        {
            GrabbableObject[] list = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
            foreach(GrabbableObject obj in list)
            {
                if (obj.isInShipRoom)
                {
                    obj.transform.localScale = new Vector3(RandomSizePlayer.Instance.k1.Value, RandomSizePlayer.Instance.k2.Value, RandomSizePlayer.Instance.k3.Value);
                }
            }
        }
    }

    [HarmonyPatch(typeof(NetworkObject))]
    internal class NetworkObjectPatch
    {
        // Token: 0x06000003 RID: 3 RVA: 0x000020DC File Offset: 0x000002DC
        [HarmonyPatch("Spawn")]
        [HarmonyPostfix]
        private static void postfix(NetworkObject __instance)
        {
            if (!RandomSizePlayer.Instance.ship)
            {
                __instance.transform.localScale = new Vector3(RandomSizePlayer.Instance.k1.Value, RandomSizePlayer.Instance.k2.Value, RandomSizePlayer.Instance.k3.Value);
            }
        }
    }*/

    /*        [HarmonyPatch("Update")]
            [HarmonyPostfix]
            private static void postfix(ref Transform ___thisPlayerBody)
            {

                ___thisPlayerBody.localScale = new Vector3(1, 1, 1);

            }


            [HarmonyPatch("Update")]
            [HarmonyPrefix]
            private static void prefix(ref Transform ___thisPlayerBody)
            {
                ___thisPlayerBody.localScale = new Vector3(RandomSizePlayer.Instance.k1.Value, RandomSizePlayer.Instance.k2.Value, RandomSizePlayer.Instance.k3.Value);
            }*/

    /*        // Token: 0x06000004 RID: 4 RVA: 0x000021DC File Offset: 0x000003DC
            [HarmonyPatch("Update")]
            [HarmonyPostfix]
            private static void OtherCorrections(ref bool ___inTerminalMenu, ref Transform ___thisPlayerBody, ref float ___fallValue)
            {
                bool flag = ___inTerminalMenu;
                if (flag)
                {
                    ___thisPlayerBody.position = new Vector3(___thisPlayerBody.position.x, ___thisPlayerBody.position.y + RandomSizePlayer.Instance.k4.Value, ___thisPlayerBody.position.z);
                    ___fallValue = 0f;
                }
            }*//*

    // Token: 0x06000005 RID: 5 RVA: 0x00002230 File Offset: 0x00000430
    [HarmonyPatch("SpawnDeadBody")]
    [HarmonyPrefix]
    private static void DeadPlayer(ref DeadBodyInfo ___deadBody)
    {
        ___deadBody.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
}
*/
}