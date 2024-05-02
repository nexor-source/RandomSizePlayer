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
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // 可以添加一些方法来转换到或从 Vector3 类型
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static SerializableVector3 FromVector3(Vector3 vector)
    {
        return new SerializableVector3(vector.x, vector.y, vector.z);
    }
}


namespace RandomSizePlayer
{
    /// <summary>
    /// 插件加载
    /// </summary>
    [BepInPlugin(modGUID, modName, modVersion)]
    public class RandomSizePlayer : BaseUnityPlugin
    {
        private const string modGUID = "nexor.RandomSizePlayer";
        private const string modName = "RandomSizePlayer";
        private const string modVersion = "0.0.7";

        private readonly Harmony harmony = new Harmony(modGUID);

        public ConfigEntry<float> k1, k2, k3, player_sharpness, c1, c2, c3, furniture_sharpness;
        public ConfigEntry<string> load_file_name, save_file_name;

        public static RandomSizePlayer Instance;
        public static BepInEx.Logging.ManualLogSource Logger;
        public ConfigEntry<bool> change_player_size, player_random, hide_your_visor, lock_player_portion, load_preset, lock_furniture_portion, furniture_random;
        
        public bool ship = false;


        private Dictionary<string, SerializableVector3> size_original_dict, size_preset_dict;

        private void Load_Dict()
        {
            // 读取本地文件来初始化持久化字典
            if (File.Exists(Application.persistentDataPath + "/size_preset_dict" + load_file_name.Value + ".dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/size_preset_dict" + load_file_name.Value + ".dat", FileMode.Open);
                size_preset_dict = (Dictionary<string, SerializableVector3>)bf.Deserialize(file);
                file.Close();
            }
            else
            {
                size_preset_dict = new Dictionary<string, SerializableVector3>();
            }
            size_original_dict = new Dictionary<string, SerializableVector3>();
        }

        private void Save_Preset_Dict()
        {
            // 将持久化字典保存到本地文件
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/size_preset_dict" + save_file_name.Value + ".dat");
            bf.Serialize(file, size_preset_dict);
            file.Close();
        }

        public bool in_preset_dict(NetworkObject networkObject)
        {
            string objectName = networkObject.name;

            // 检查 NetworkObject.name 是否处于 size_preset_dict.keys 中
            if (RandomSizePlayer.Instance.size_preset_dict.ContainsKey(objectName))
            {
                // 获取预设的大小变换
                Vector3 appliedSize = size_preset_dict[objectName].ToVector3();

                return true;
            }
            return false;
        }

        public Vector3 get_preset_from_preset_dict(NetworkObject networkObject)
        {
            string objectName = networkObject.name;
            // 检查 NetworkObject.name 是否处于 size_preset_dict.keys 中
            if (RandomSizePlayer.Instance.size_preset_dict.ContainsKey(objectName))
            {
                // 获取预设的大小变换
                Vector3 appliedSize = size_preset_dict[objectName].ToVector3();

                return appliedSize;
            }
            return new Vector3(1,1,1);

        }

        public void Update_size_original_dict(NetworkObject networkObject)
        {
            string objectName = networkObject.name;
            // 检查持久化字典中是否存在 networkObject.name 的键
            if (!size_original_dict.ContainsKey(objectName))
            {
                // 如果不存在，添加到字典中，并设置其值为 networkObject.transform.localScale
                size_original_dict.Add(objectName, SerializableVector3.FromVector3(networkObject.transform.localScale));
            }

            return;
        }

        public Vector3 Update_size_preset_dict(NetworkObject networkObject, float x, float y, float z)
        {
            string objectName = networkObject.name;
            Vector3 original_size = size_original_dict[networkObject.name].ToVector3();
            Vector3 applied_size = new Vector3(original_size.x * x, original_size.y * y, original_size.z * z);
            size_preset_dict[networkObject.name] = SerializableVector3.FromVector3(applied_size);
            Save_Preset_Dict();

            return applied_size;
        }

        // 在插件启动时会直接调用Awake()方法
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            
            // 初始化配置项
            change_player_size = Config.Bind<bool>("Player Size(玩家大小调整)",
                                    "change player size? 是否改变玩家大小",
                                    true,
                                    "If false, all config in Player Size will be ignored 如果是false,则会忽略Player Size(玩家大小调整)中的所有变量");

            k1 = Config.Bind<float>("Player Size(玩家大小调整)",
                                    "width 宽",
                                    1f,
                                    "if you want everybody to be the exact same size 如果你想让所有玩家都一个大小的话");

            k2 = Config.Bind<float>("Player Size(玩家大小调整)",
                                    "height 高",
                                    1f,
                                    "if you want everybody to be the exact same size 如果你想让所有玩家都一个大小的话");

            k3 = Config.Bind<float>("Player Size(玩家大小调整)",
                                    "thickness 厚",
                                    1f,
                                    "if you want everybody to be the exact same size 如果你想让所有玩家都一个大小的话");

            player_random = Config.Bind<bool>("Player Size(玩家大小调整)",
                                    "random size? 是否采用随机大小",
                                    false,
                                    "If a random size is used, the three fixed scale values customized above are ignored 如果采用随机大小，则会忽视上面自定义的三个固定比例值");

            lock_player_portion = Config.Bind<bool>("Player Size(玩家大小调整)",
                                    "lock portion? 锁定随机比例?",
                                    false,
                                    "Whether the random player size changes in proportion 随机玩家大小时是否同比例变化");

            player_sharpness = Config.Bind<float>("Player Size(玩家大小调整)",
                                    "the sharpness of the random player size s curve? 随机玩家大小的概率曲线的陡峭程度",
                                    1f,
                                    "The larger the value, the more likely extreme values are to occur, while the smaller the value, the more likely intermediate values are to occur" +
                                    "越大越容易出现极端值, 越小越容易出现中间值");

            load_preset = Config.Bind<bool>("Furniture Size(船上设施大小调整)",
                                    "load preset size file? 是否加载预设文件",
                                    true,
                                    "Load your size preset files for furniture whenever you host the game. 每当你开房时都可以为家具加载你的大小预设文件");

            c1 = Config.Bind<float>("Furniture Size(船上设施大小调整)",
                                    "width 宽",
                                    1f,
                                    "Affects the size of your newly placed furniture, the guest machine can only see the size of the furniture at the time it entered the room, and it will regain its size after recycling to the terminal and taking it out again 影响你新放置的家具的大小，客机只能看到其进房间时的家具的大小，并且回收到终端再拿出后客机会恢复大小");

            c2 = Config.Bind<float>("Furniture Size(船上设施大小调整)",
                                    "height 高",
                                    1f,
                                    "the same as above 同上");

            c3 = Config.Bind<float>("Furniture Size(船上设施大小调整)",
                                    "thickness 厚",
                                    1f,
                                    "the same as above 同上");
            load_file_name = Config.Bind<string>("Furniture Size(船上设施大小调整)",
                                    "load size-preset-file s name 加载预设文件的名字",
                                    "_default",
                                    "if load size preset file, load file named with xxx. 如果加载预设文件的话，加载名为xxx的预设文件");

            save_file_name = Config.Bind<string>("Furniture Size(船上设施大小调整)",
                                    "save size-preset-file s name 保存预设文件的名字",
                                    "_default",
                                    "Whenever you place a furniture, its size is recorded and written to a preset file called xxx 每当你放置一个家具时，其就会记录该家具的大小并写入名为xxx的预设文件");

            hide_your_visor = Config.Bind<bool>("Other(其他)",
                        "hide you visor? 是否隐藏面罩",
                        false,
                        "hide visor only work on you. 隐藏面罩只对自己生效");


            Logger = base.Logger;
            harmony.PatchAll();
            Logger.LogInfo("RandomSizePlayer 0.0.7 loaded.");

            Load_Dict();
        }
    }


    /// <summary>
    /// 玩家size/visor的修改
    /// </summary>
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerB_Awake_Patch
    {

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void postfix(PlayerControllerB __instance)
        {

            // 隐藏面罩
            if (RandomSizePlayer.Instance.hide_your_visor.Value)
            {
                __instance.localVisorTargetPoint.position = new Vector3(0f, 0f, 1000f);
            }

            // 如果关闭了调整玩家大小的开关，则直接返回
            if (!RandomSizePlayer.Instance.change_player_size.Value) return;

            // 如果自己不是server的话，该mod就不要生效
            NetworkManager networkManager = __instance.NetworkManager;
            if (!networkManager.IsServer) return;


            float x, y, z;
            x = RandomSizePlayer.Instance.k1.Value;
            y = RandomSizePlayer.Instance.k2.Value;
            z = RandomSizePlayer.Instance.k3.Value;


            // 不随机则取设定值
            if (!RandomSizePlayer.Instance.player_random.Value)
            {
                // 0 头  1 右前臂  2 左前臂  3 右前腿  4 左前腿  5 上半身  6 整体  7 右腿  8 左腿  9 左臂  10 右臂
                // __instance.bodyParts[RandomSizePlayer.Instance.test.Value].localScale = new Vector3(x, y, z);

                /*__instance.bodyParts[7].localScale = new Vector3(x, y, z);
                __instance.bodyParts[8].localScale = new Vector3(x, y, z);
                __instance.bodyParts[9].localScale = new Vector3(x, y, z);
                __instance.bodyParts[10].localScale = new Vector3(x, y, z);*/

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

                x = (1f / (1f + Mathf.Exp(-4f * _x * RandomSizePlayer.Instance.player_sharpness.Value)) - 0.5f) * 2f * 1.6f;
                y = 1f / (1f + Mathf.Exp(-2f * _y * RandomSizePlayer.Instance.player_sharpness.Value)) * 1.8f;
                z = 1f / (1f + Mathf.Exp(-4f * _z * RandomSizePlayer.Instance.player_sharpness.Value)) * 1.65f;

                // 是否按比例缩放
                if (RandomSizePlayer.Instance.lock_player_portion.Value) __instance.transform.localScale = new Vector3(y, y, y);
                else __instance.transform.localScale = new Vector3(x, y, z);
            }

        }
    }


    /// <summary>
    /// 家具size的修改
    /// </summary>


    [HarmonyPatch(typeof(ShipBuildModeManager))]
    internal class ShipBuildModeManager_Patch
    {
        [HarmonyPatch("PlaceShipObjectServerRpc")]
        [HarmonyPrefix]
        private static void prefix(ShipBuildModeManager __instance, Vector3 newPosition, Vector3 newRotation, NetworkObjectReference objectRef, int playerWhoMoved)
        {

            // 如果自己不是server的话，该mod就不要生效
            NetworkManager networkManager = __instance.NetworkManager;
            if (!networkManager.IsServer) return;

            // 客机不许应用大小
            if (playerWhoMoved != (int) StartOfRound.Instance.localPlayerController.playerClientId) return;

            // RandomSizePlayer.Logger.LogInfo("listen client id " + playerWhoMoved + ", your id is " + StartOfRound.Instance.localPlayerController.playerClientId);
            NetworkObject networkObject;
            objectRef.TryGet(out networkObject, null);
            float x, y, z;
            x = RandomSizePlayer.Instance.c1.Value;
            y = RandomSizePlayer.Instance.c2.Value;
            z = RandomSizePlayer.Instance.c3.Value;

            // 保存一下物体的初始scale
            RandomSizePlayer.Instance.Update_size_original_dict(networkObject);
            
            // 计算应用后的scale并保存
            Vector3 applied_size = RandomSizePlayer.Instance.Update_size_preset_dict(networkObject, x, y, z);

            networkObject.transform.localScale = applied_size;
            RandomSizePlayer.Logger.LogInfo("changed!");

        }
    }

    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRound_Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void postfix(StartOfRound __instance)
        {
            // 如果自己不是server的话，该mod就不要生效
            NetworkManager networkManager = __instance.NetworkManager;
            if (!networkManager.IsServer) return;

            // 遍历所有的 NetworkObject
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
            {
                // 获取预设的大小变换
                if (!RandomSizePlayer.Instance.load_preset.Value) return;
                if (RandomSizePlayer.Instance.in_preset_dict(networkObject))
                {
                    RandomSizePlayer.Instance.Update_size_original_dict(networkObject);
                    Vector3 appliedSize = RandomSizePlayer.Instance.get_preset_from_preset_dict(networkObject);
                    // 应用大小变换
                    networkObject.transform.localScale = appliedSize;
                }
                
            }
        }
    }
}