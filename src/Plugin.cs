using System;
using BepInEx;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Collections.Generic;
using JetBrains.Annotations;
using Expedition;
using Noise;
using Random = UnityEngine.Random;
using System.Runtime.CompilerServices;
using MonoMod;
using System.Security.Policy;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "The Metabolite", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {

        private const string MOD_ID = "adalynn.metabolite";
        private ConditionalWeakTable<Player, SpearTime> spearTimeTable = new();
        /*public static readonly PlayerFeature<float> SuperJump = PlayerFloat("TheMetabolite/super_jump");
        public static readonly PlayerFeature<bool> ExplodeOnDeath = PlayerBool("TheMetabolite/explode_on_death");
        public static readonly GameFeature<float> MeanLizards = GameFloat("TheMetabolite/mean_lizards");*/
        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            //On.Player.InitiateGraphicsModule += Player_CWT;
            // Put your custom hooks here!
            //On.Player.Jump += Player_Jump;
            //On.Player.Die += Player_Die;
            //On.Lizard.ctor += Lizard_ctor;
            On.Player.ctor += Saint.Player_ctor;
            On.Player.SaintTongueCheck += Saint.Player_SaintTongueCheck;
            On.Player.ClassMechanicsSaint += Saint.Player_ClassMechanicsSaint;
            On.Player.ClassMechanicsArtificer += Artificer.ClassMechanicsArtificer;
            On.PlayerGraphics.ctor += Saint.PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += Saint.PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += Saint.PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ApplyPalette += Saint.PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.MSCUpdate += Saint.PlayerGraphics_MSCUpdate;
            On.Player.Grabability += Player_Grabability;
            On.Player.ThrownSpear += Gourmand.ThrownSpear;
            On.Player.Update += Player_Update;
            //            On.Player.ClassMechanicsSpearmaster += Spearmaster.ClassMechanicsSpearmaster;
            On.Player.GrabUpdate += Spearmaster.GrabUpdate;
            On.Player.CraftingResults += Player_CraftingResults;
            On.Player.GraspsCanBeCrafted += Spearmaster.GraspsCanBeCrafted;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            On.Player.SwallowObject += Artificer.SwallowObject;
        }
        private class SpearTime
        {
            public int spearTime;
            public int STime
            {
                get => spearTime;
                set
                {
                    if (value < 0) spearTime = 0;
                    else spearTime = value;
                }
            }
            public SpearTime()
            {
                STime = 60;
            }
            public SpearTime(int spearTime)
            {
                STime = spearTime;
            }
        }
        private Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (self.slugcatStats.name.value == "Metabolite" && obj is Weapon)
            {
                return Player.ObjectGrabability.OneHand;
            }
            return orig.Invoke(self, obj);
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {

        }
        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            SpearTime data = new();
            bool temp = spearTimeTable.TryGetValue(self, out data);
            if (temp
                && self.slugcatStats.name.value == "Metabolite"
                && self.FoodInStomach >= 1
                && self.bodyMode == Player.BodyModeIndex.Crawl
                && self.eatMeat == 0
                && self.input[0].pckp
                && self.FreeHand() > -1)
            {
                SpearTime replacement = new(data.STime--);
                if (replacement.spearTime == 0)
                {
                    Debug.Log("Spear Generation!");
                    self.SubtractFood(1);
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Grab, 0f, 1f, 0.5f + Random.value * 1.5f);
                    spearTimeTable.Remove(self);
                    spearTimeTable.Add(self, new());
                    AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), false);
                    self.room.abstractRoom.AddEntity(abstractSpear);
                    abstractSpear.pos = self.abstractCreature.pos;
                    abstractSpear.RealizeInRoom();
                    PlayerGraphics.TailSpeckles tailSpecks = (self.graphicsModule as PlayerGraphics).tailSpecks;
                    if (abstractSpear.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                    {
                        (abstractSpear.realizedObject as Spear).Spear_makeNeedle(tailSpecks.spearType, false);
                        if ((self.graphicsModule as PlayerGraphics).useJollyColor)
                        {
                            (abstractSpear.realizedObject as Spear).jollyCustomColor = new Color?(PlayerGraphics.JollyColor(self.playerState.playerNumber, 2));
                        }
                    }
                    if (self.FreeHand() > -1)
                    {
                        self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                    }
                }
            }
            else if (self.slugcatStats.name.value == "Metabolite")
            {
                spearTimeTable.Remove(self);
                spearTimeTable.Add(self, new());
            }
            orig(self, eu);
        }

        public void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            if (self.slugcatStats.name.value == "Metabolite")
            {
                var S = self.SlugCatClass;
                self.SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
                orig(self, eu);
                self.SlugCatClass = S;
            }

            else
            {
                orig(self, eu);
            }


        }

        public AbstractPhysicalObject.AbstractObjectType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
        {
            Debug.Log("Crafting");
            if (self.slugcatStats.name.value == "Metabolite")
            {
                if(self.input[0].y == 0)
                {
                    return Artificer.CraftingResults(self);
                }
                else if (self.input[0].y == 1)
                {
                    return Gourmand.CraftingResults(self);
                }
                else
                {
                    return orig(self);
                }
            }
            else
            {
                return orig(self);
            }
        }

        public void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
        {
            self.craftingTutorial = true;
            self.room.PlaySound(SoundID.Slugcat_Swallow_Item, self.mainBodyChunk);
            if (true)
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i] != null)
                    {
                        AbstractPhysicalObject abstractPhysicalObject = self.grasps[i].grabbed.abstractPhysicalObject;
                        if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear && !(abstractPhysicalObject as AbstractSpear).explosive)
                        {
                            if ((abstractPhysicalObject as AbstractSpear).electric && (abstractPhysicalObject as AbstractSpear).electricCharge > 0)
                            {
                                self.room.AddObject(new ZapCoil.ZapFlash(self.firstChunk.pos, 10f));
                                self.room.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 1f, 1.5f + Random.value * 1.5f);
                                if (self.Submersion > 0.5f)
                                {
                                    self.room.AddObject(new UnderwaterShock(self.room, null, self.firstChunk.pos, 10, 800f, 2f, self, new Color(0.8f, 0.8f, 1f)));
                                }
                                self.Stun(200);
                                self.room.AddObject(new CreatureSpasmer(self, false, 200));
                                return;
                            }
                            self.ReleaseGrasp(i);
                            abstractPhysicalObject.realizedObject.RemoveFromRoom();
                            self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                            self.SubtractFood(1);
                            AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), true);
                            self.room.abstractRoom.AddEntity(abstractSpear);
                            abstractSpear.RealizeInRoom();
                            if (self.FreeHand() != -1)
                            {
                                self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                            }
                            return;
                        }
                    }
                }
            }
            AbstractPhysicalObject abstractPhysicalObject2 = null;
            if (GourmandCombos.CraftingResults_ObjectData(self.grasps[0], self.grasps[1], true) == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
            {
                if (ModManager.Expedition && ModManager.MSC && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-crafting") && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                {
                    if (abstractPhysicalObject2 != null && self.FreeHand() != -1)
                    {
                        self.SlugcatGrab(abstractPhysicalObject2.realizedObject, self.FreeHand());
                    }
                    return;
                }
                while ((self.grasps[0] != null && self.grasps[0].grabbed is IPlayerEdible) || (self.grasps[1] != null && self.grasps[1].grabbed is IPlayerEdible))
                {
                    self.BiteEdibleObject(true);
                }
                self.AddFood(1);
            }
            else
            {
                abstractPhysicalObject2 = GourmandCombos.CraftingResults(self, self.grasps[0], self.grasps[1]);
                self.room.abstractRoom.AddEntity(abstractPhysicalObject2);
                abstractPhysicalObject2.RealizeInRoom();
                for (int j = 0; j < self.grasps.Length; j++)
                {
                    AbstractPhysicalObject abstractPhysicalObject3 = self.grasps[j].grabbed.abstractPhysicalObject;
                    if (self.room.game.session is StoryGameSession)
                    {
                        (self.room.game.session as StoryGameSession).RemovePersistentTracker(abstractPhysicalObject3);
                    }
                    self.ReleaseGrasp(j);
                    for (int k = abstractPhysicalObject3.stuckObjects.Count - 1; k >= 0; k--)
                    {
                        if (abstractPhysicalObject3.stuckObjects[k] is AbstractPhysicalObject.AbstractSpearStick && abstractPhysicalObject3.stuckObjects[k].A.type == AbstractPhysicalObject.AbstractObjectType.Spear && abstractPhysicalObject3.stuckObjects[k].A.realizedObject != null)
                        {
                            (abstractPhysicalObject3.stuckObjects[k].A.realizedObject as Spear).ChangeMode(Weapon.Mode.Free);
                        }
                    }
                    abstractPhysicalObject3.LoseAllStuckObjects();
                    abstractPhysicalObject3.realizedObject.RemoveFromRoom();
                    self.room.abstractRoom.RemoveEntity(abstractPhysicalObject3);
                }
            }
            if (abstractPhysicalObject2 != null && self.FreeHand() != -1)
            {
                self.SlugcatGrab(abstractPhysicalObject2.realizedObject, self.FreeHand());
            }
        }
    }

}