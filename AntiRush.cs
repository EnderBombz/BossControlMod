﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AntiRush
{
    [BepInPlugin("EnderBombz.AntiRush", "AntiRush", "1.0.0")]
    [BepInProcess("valheim.exe")]

    public class AntiRush : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("EnderBombz.AntiRush");

        public static int realDay = 31;
        public static int currentDay;
        public static string[] bosses = { "$piece_offerbowl_eikthyr", "$prop_eldersummoningbowl_name", "$piece_offerbowl_bonemass", "$prop_dragonsummoningbowl_name", "$piece_offerbowl_yagluth" };

        public static ConfigEntry<bool> Real;

        public static ConfigEntry<int> EikthyrInvokeDay;
        public static ConfigEntry<int> EikthyrItemAmount;

        public static ConfigEntry<int> ElderInvokeDay;
        public static ConfigEntry<int> ElderItemAmount;

        public static ConfigEntry<int> BoneMassInvokeDay;
        public static ConfigEntry<int> BoneMassItemAmount;

        public static ConfigEntry<int> ModerInvokeDay;
        public static ConfigEntry<int> YagluthInvokeDay;

        public static List<ControlBossConfig> bossList;

        public class ControlBossConfig
        {
            public string NameTranslate { get; set; }
            public int Days { get; set; }
            public string PlaceName { get; set; }

            public ControlBossConfig()
            {
            }

            public ControlBossConfig(string name)
            {
                this.PlaceName = name;
            }
        }

        void Awake()
        { 
            var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "bossControll_config.cfg"), true);

            Real = customFile.Bind("Real", "realLife", true, "This option changes the calculation method, one day in real life, there are 31 days in the game.");

            EikthyrInvokeDay = customFile.Bind("EikthyrInvokeDay",  "spawnDay", 1, "Eikthyr day to be invoked ");
            EikthyrItemAmount = customFile.Bind("EikthyrItemAmount", "amountItem", 2, "Amout of items to invoke");

            ElderInvokeDay = customFile.Bind("ElderInvokeDay",      "spawnDay", 4,  "Elder day to be invoked ");
            ElderItemAmount = customFile.Bind("ElderItemAmount", "amountItem", 3, "Amout of items to invoke");

            BoneMassInvokeDay = customFile.Bind("BoneMassInvokeDay","spawnDay", 10, "BoneMass day to be invoked");
            BoneMassItemAmount = customFile.Bind("BoneMassItemAmount", "amountItem", 3, "Amout of items to invoke");

            ModerInvokeDay = customFile.Bind("ModerInvokeDay",      "spawnDay", 20, "Moder day to be invoked");
            YagluthInvokeDay = customFile.Bind("YagluthInvokeDay",  "spawnDay", 30, "Yagluth day to be invoked ");

            bossList = new List<ControlBossConfig>
            {
                new ControlBossConfig { NameTranslate = "Eikthyr",      PlaceName = "$piece_offerbowl_eikthyr",         Days = EikthyrInvokeDay.Value },
                new ControlBossConfig { NameTranslate = "Ancião",       PlaceName = "$prop_eldersummoningbowl_name",    Days = ElderInvokeDay.Value },
                new ControlBossConfig { NameTranslate = "Massa Óssea",  PlaceName = "$piece_offerbowl_bonemass",        Days = BoneMassInvokeDay.Value },
                new ControlBossConfig { NameTranslate = "Moder",        PlaceName = "$prop_dragonsummoningbowl_name",   Days = ModerInvokeDay.Value },
                new ControlBossConfig { NameTranslate = "Yagluth",      PlaceName = "$piece_offerbowl_yagluth",         Days = YagluthInvokeDay.Value }
            };
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(OfferingBowl), "Interact")]
        public static class AntiRushInteraction_patch
        {
            static bool Prefix(OfferingBowl __instance, Humanoid user)
            {

                __instance.m_bossItems = 3;
                currentDay = EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds());
                Debug.Log("Interact debugging...");
                Debug.Log($"Current day is: { currentDay }");
                Debug.Log($"Current boss is: { Localization.instance.Localize(__instance.m_name) }");
                Debug.Log($"Current boss altar name: { __instance.m_name }");
                return CheckBossAccess(__instance.m_name, user);
            }
        }

        [HarmonyPatch(typeof(OfferingBowl), "UseItem")]
        public static class AntiRushUseItem_patch
        {
            static bool Prefix(OfferingBowl __instance, Humanoid user, ItemDrop.ItemData item)
            {
                __instance.m_bossItems = 3;
                currentDay = EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds());
                Debug.Log("UseItem debugging...");
                Debug.Log($"{currentDay}<{realDay} && {__instance.m_name}=={bosses[0]}?");

                return !CheckBossAccess(__instance.m_name, user); ;
            }
        }

        public static bool CheckBossAccess(string bossPlace, Humanoid user)
        {
            foreach (var boss in bossList)
            {
                var bossCalc = (realDay * boss.Days);
                if ( (currentDay < bossCalc) && (bossPlace == boss.PlaceName) )
                {
                    Debug.Log("Yes he is entering in exeption");
                    user.Message(MessageHud.MessageType.Center, $"O {boss.NameTranslate} só pode ser invocado em {currentDay} / {bossCalc} dias!");
                    return true;
                }
            }
            return false;
        }
    }
}