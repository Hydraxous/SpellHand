using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpellHand
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class SpellHandPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.hydraxous.spellhand";
        public const string PLUGIN_NAME = "SpellHand";
        public const string PLUGIN_VERSION = "1.0.0";

        public static SpellHandPlugin Instance { get; private set; }

        public AssetLoader AssetLoader { get; private set; }

        public GameObject SpellHandPrefab { get; private set; }
        public GameObject SpellHandInstance;
        
        public static string ModLocation => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public Dictionary<string, (int,int)> AnimationMap = new Dictionary<string, (int, int)>();

        public ConfigEntry<bool> DisableCrosshairOverlay;
        public ConfigEntry<bool> FlipRingPositions;

        private void Awake()
        {
            Instance = this;

            DisableCrosshairOverlay = Config.Bind("General", "DisableCrosshairOverlay", true, "Disables the magic crosshair overlay shown when charging spells.");
            FlipRingPositions = Config.Bind("General", "FlipRingPositions", false, "Flips the positions of the rings on the hand.");

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
                string filePath = Path.Combine(ModLocation, "AnimationMap.txt");
                if (!File.Exists(filePath))
                {
                    //Create a default file.
                    File.WriteAllText(filePath, Properties.Resources.AnimationMap);
                    Logger.LogInfo("Default animation map created.");
                }

                //Parse the file.
                string allText = File.ReadAllText(filePath);
                string[] lines = allText.Split('\n');
                foreach (string line in lines)
                {
                    //Comment. Ignore
                    if (line.StartsWith("//"))
                    {
                        continue;
                    }

                    //Split entry
                    string[] entryParts = line.Split('=');
                    if (entryParts.Length != 2)
                    {
                        continue;
                    }

                    //Get the spell name.
                    string targetSpellName = entryParts[0];
                    if (string.IsNullOrEmpty(targetSpellName))
                    {
                        continue;
                    }

                    //Parse the indexes.
                    string[] animationValues = entryParts[1].Split(',');
                    if (animationValues.Length != 2)
                    {
                        continue;
                    }
                    
                    int handIndex = 0;
                    int fingerIndex = 0;
                    if (!int.TryParse(animationValues[0], out handIndex) || !int.TryParse(animationValues[1], out fingerIndex))
                    {
                        continue;
                    }

                    AnimationMap[targetSpellName] = (handIndex, fingerIndex);
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
