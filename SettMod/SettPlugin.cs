﻿using BepInEx;
using R2API.Utils;
using RoR2;
using SettMod.Modules;
using SettMod.Modules.Survivors;
using SettMod.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SettMod
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [R2APISubmoduleDependency(new string[]
    {
        "PrefabAPI",
        "LanguageAPI",
        "SoundAPI",
        "NetworkingAPi",
    })]

    public class SettPlugin : BaseUnityPlugin
    {
        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string developerPrefix = "Jojo";

        public const string MODNAME = "Sett";

        // if you don't change these you're giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.Lemonlust.Sett";

        public const string MODVERSION = "1.5.1";
        public static SettPlugin instance;
        internal List<SurvivorBase> Survivors = new List<SurvivorBase>();
        private GritGauge gritGauge;

        public void OnDestroy()
        {
            try
            {
                UnHooks();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }
        }

        private void Awake()
        {
            instance = this;

            // load assets and read config
            Modules.Assets.Initialize();
            Modules.Config.ReadConfig();
            Modules.States.RegisterStates(); // register states for networking
            Modules.Buffs.RegisterBuffs(); // add and register custom buffs/debuffs
            Modules.Projectiles.RegisterProjectiles(); // add and register custom projectiles
            Modules.Tokens.AddTokens(); // register name tokens
            Modules.ItemDisplays.PopulateDisplays(); // collect item display prefabs for use in our display rules

            // survivor initialization
            new MyCharacter().Initialize();

            // now make a content pack and add it- this part will change with the next update
            new Modules.ContentPacks().Initialize();

            RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;

            Hook();
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self)
            {
                if (self.HasBuff(Modules.Buffs.regenBuff))
                {
                    float count = self.GetBuffCount(Modules.Buffs.regenBuff);
                    float _level = Mathf.Ceil(self.level / 4);
                    self.regen += count * (0.25f * _level);
                }
            }
        }

        private void CreateGritGauge(RoR2.UI.HUD hud)
        {
            if (!gritGauge)
            {
                if (hud != null && hud.mainUIPanel != null)
                {
                    gritGauge = hud.mainUIPanel.GetComponentInChildren<GritGauge>();
                    if (!gritGauge)
                    {
                        var gritGaugePanel = Instantiate(Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("GritGaugePanel"));
                        gritGauge = gritGaugePanel.AddComponent<GritGauge>();
                        gritGaugePanel.transform.SetParent(hud.mainUIPanel.transform);
                        var rectTransform = gritGaugePanel.GetComponent<RectTransform>();
                        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        rectTransform.sizeDelta = new Vector2(120, 120);
                        rectTransform.anchoredPosition = new Vector2(0, -430f);
                        rectTransform.localScale = new Vector3(2, 2, 2);
                        gritGaugePanel.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void Hook()
        {
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;
            On.RoR2.PickupPickerController.FixedUpdateServer += PickupPickerController_FixedUpdateServer;
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            CreateGritGauge(self);
            orig(self);
        }

        private void HUD_onHudTargetChangedGlobal(RoR2.UI.HUD obj)
        {
            if (obj && obj.targetBodyObject && gritGauge)
            {
                var grit = obj.targetBodyObject.GetComponent<GritComponent>();
                if (grit)
                {
                    gritGauge.gameObject.SetActive(true);
                    gritGauge.source = grit;
                }
                else
                {
                    gritGauge.gameObject.SetActive(false);
                    gritGauge.source = null;
                }
            }
        }

        private void LateSetup(HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
            // have to set item displays later now because they require direct object references..
            //Modules.Survivors.MyCharacter.instance.SetItemDisplays();
        }

        private void PickupPickerController_FixedUpdateServer(On.RoR2.PickupPickerController.orig_FixedUpdateServer orig, PickupPickerController self)
        {
            CharacterMaster currentParticipantMaster = self.networkUIPromptController.currentParticipantMaster;
            if (currentParticipantMaster)
            {
                CharacterBody body = currentParticipantMaster.GetBody();
                var interactor = (body) ? body.GetComponent<Interactor>() : null;
                if (!body || (body.inputBank.aimOrigin - self.transform.position).sqrMagnitude > ((interactor) ? Math.Pow((interactor.maxInteractionDistance + self.cutoffDistance), 2f) : (self.cutoffDistance * self.cutoffDistance)))
                {
                    self.networkUIPromptController.SetParticipantMaster(null);
                }
            }
        }

        private void UnHooks()
        {
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.UI.HUD.Awake -= HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal -= HUD_onHudTargetChangedGlobal;
            On.RoR2.PickupPickerController.FixedUpdateServer -= PickupPickerController_FixedUpdateServer;
        }
    }
}