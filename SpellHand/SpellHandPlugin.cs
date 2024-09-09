using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpellHand
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class SpellHandPlugin : BaseUnityPlugin
    {

        public const string PLUGIN_GUID = "com.hydraxous.spellhand";
        public const string PLUGIN_NAME = "SpellHand";
        public const string PLUGIN_VERSION = "0.0.1";

        private Player_Control_scr player;
        private CONTROL gameManager;
        private Transform mainCamera;

        public static SpellHandPlugin Instance { get; private set; }

        public AssetLoader AssetLoader;
        public GameObject SpellHandPrefab;

        public GameObject SpellHandInstance;
        
        public static string modPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public Dictionary<string, (int,int)> AnimationMap = new Dictionary<string, (int, int)>();

        public ConfigEntry<bool> DisableCrosshairOverlay;

        private void Awake()
        {
            Instance = this;

            DisableCrosshairOverlay = Config.Bind("General", "DisableCrosshairOverlay", true, "Disables the magic crosshair overlay shown when charging spells.");

            //Load assetbundle
            AssetLoader = new AssetLoader(Properties.Resources.SpellHandBundle);
            SpellHandPrefab = AssetLoader.LoadAsset<GameObject>(nameof(SpellHandPrefab));

            LoadAnimationMap();

            Harmony harmony = new Harmony(PLUGIN_GUID + ".harmony");
            harmony.PatchAll();

            Logger.LogInfo("Patched Successfully.");

            Logger.LogInfo($"{PLUGIN_NAME} ({PLUGIN_VERSION}) is loaded!");

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //Reload the animation map
            LoadAnimationMap();
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.K))
            {
                LoadAnimationMap();
            }
        }

        private void LoadAnimationMap()
        {
            AnimationMap ??= new Dictionary<string, (int, int)>();
            AnimationMap.Clear();

            Logger.LogInfo("Reloading AnimationMap...");

            try
            {
                string filePath = Path.Combine(modPath, "AnimationMap.txt");
                if (!File.Exists(filePath))
                {
                    //Create a default file.
                    File.WriteAllText(filePath, Properties.Resources.AnimationMapping);
                    Logger.LogInfo("Default animation map created.");
                }

                string allText = File.ReadAllText(filePath);
                string[] lines = allText.Split('\n');
                foreach (string line in lines)
                {
                    //Comment.
                    if (line.StartsWith("//"))
                    {
                        continue;
                    }

                    string[] parts = line.Split('=');
                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    string targetSpell = parts[0];
                    if (string.IsNullOrEmpty(targetSpell))
                    {
                        continue;
                    }

                    string[] animationArgs = parts[1].Split(',');
                    if (animationArgs.Length != 2)
                    {
                        continue;
                    }

                    int handIndex = 0;
                    int fingerIndex = 0;
                    if (!int.TryParse(animationArgs[0], out handIndex) || !int.TryParse(animationArgs[1], out fingerIndex))
                    {
                        continue;
                    }

                    AnimationMap[targetSpell] = (handIndex, fingerIndex);
                }
                
                Logger.LogInfo($"AnimationMap loaded. {AnimationMap.Count} pairs.");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load AnimationMap.");
                Debug.LogException(e);
            }
        }
    }
}
