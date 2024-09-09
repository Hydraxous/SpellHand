using HarmonyLib;
using System;
using UnityEngine;

namespace SpellHand
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(CONTROL), nameof(CONTROL.EQMagic)), HarmonyPostfix]
        private static void CONTROL_EQMagic(CONTROL __instance)
        {
            //Instantiate our hand and apply the equipped spells.
            if(SpellHandPlugin.Instance.SpellHandInstance != null)
            {
                GameObject toDestroy = SpellHandPlugin.Instance.SpellHandInstance;
                SpellHandPlugin.Instance.SpellHandInstance = null;
                GameObject.Destroy(toDestroy);
            }

            Transform instanceLocation = __instance.PL.HANDS.transform;
            SpellHandPlugin.Instance.SpellHandInstance = GameObject.Instantiate(SpellHandPlugin.Instance.SpellHandPrefab, instanceLocation);
            SpellHandPlugin.Instance.SpellHandInstance.AddComponent<SpellHandController>();
            Debug.Log("New SpellHandInstance created");
        }

        public static event Action<Magic_scr> OnMagicScrCast;


        //__state is the charge pre and post cast.
        [HarmonyPatch(typeof(Magic_scr), nameof(Magic_scr.Cast)), HarmonyPrefix]
        private static void Magic_scr_Cast_Prefix(Magic_scr __instance, ref float[] __state)
        {
            __state = new float[2];
            __state[0] = __instance.Player.GetComponent<Player_Control_scr>().CON.CURRENT_PL_DATA.PLAYER_M;
            __state[1] = __instance.Player.GetComponent<Player_Control_scr>().CON.CURRENT_PL_DATA.PLAYER_B;
        }


        [HarmonyPatch(typeof(Magic_scr), nameof(Magic_scr.Cast)), HarmonyPostfix]
        private static void Magic_scr_Cast(Magic_scr __instance, float[] __state)
        {
            if (__state[0] != __instance.Player.GetComponent<Player_Control_scr>().CON.CURRENT_PL_DATA.PLAYER_M || __state[1] != __instance.Player.GetComponent<Player_Control_scr>().CON.CURRENT_PL_DATA.PLAYER_B)
            {
                //Magic was cast since the resources changed.
                OnMagicScrCast?.Invoke(__instance);
            }
        }

        [HarmonyPatch(typeof(Magic_scr), nameof(Magic_scr.SetValues)), HarmonyPostfix]
        private static void Magic_scr_SetValues(Magic_scr __instance)
        {
            bool showReticle = !SpellHandPlugin.Instance.DisableCrosshairOverlay.Value;
            __instance.ICON_SPR3.enabled = showReticle;
        }
    }
}
