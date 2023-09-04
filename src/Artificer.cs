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

public static class Artificer
{
    public static int cfgArtificerExplosionCapacity = 10;
    public static void ClassMechanicsArtificer(On.Player.orig_ClassMechanicsArtificer orig, Player self)
    {
        orig.Invoke(self);
/*        if (self.pyroJumpDropLock > 0)
        {
            self.pyroJumpDropLock--;
        }*/
        if// (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer ||
            (self.slugcatStats.name.value == "Metabolite" && self.FoodInStomach >= 1) //||
            //(ExpeditionGame.explosivejump && !self.isSlugpup))
        {
            bool flag = self.wantToJump > 0 && self.input[0].pckp;
            bool flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;
            int num = Mathf.Max(1, cfgArtificerExplosionCapacity - 5);
            if (self.pyroJumpCounter > 0 && (self.Consious || self.dead))
            {
                self.pyroJumpCooldown -= 1f;
                if (self.pyroJumpCooldown <= 0f)
                {
                    if (self.pyroJumpCounter >= num)
                    {
                        self.pyroJumpCooldown = 40f;
                    }
                    else
                    {
                        self.pyroJumpCooldown = 60f;
                    }
                    self.pyroJumpCounter--;
                }
            }
            self.pyroParryCooldown -= 1f;
            if (self.pyroJumpCounter >= num)
            {
                if (Random.value < 0.25f)
                {
                    self.room.AddObject(new Explosion.ExplosionSmoke(self.mainBodyChunk.pos, Custom.RNV() * 2f * Random.value, 1f));
                }
                if (Random.value < 0.5f)
                {
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
                }
            }
            if (flag && !self.pyroJumpped && self.canJump <= 0 && !flag2 && (self.input[0].y >= 0 || (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f))) && self.Consious && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.animation != Player.AnimationIndex.HangFromBeam && self.animation != Player.AnimationIndex.ClimbOnBeam && self.bodyMode != Player.BodyModeIndex.WallClimb && self.bodyMode != Player.BodyModeIndex.Swimming && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab && self.animation != Player.AnimationIndex.ZeroGPoleGrab && self.onBack == null)
            {
                if(self.slugcatStats.name.value == "Metabolite")
                {
                    self.SubtractFood(1);
                }
                self.pyroJumpped = true;
                self.pyroJumpDropLock = 40;
                self.noGrabCounter = 5;
                Vector2 pos = self.firstChunk.pos;
                for (int i = 0; i < 8; i++)
                {
                    self.room.AddObject(new Explosion.ExplosionSmoke(pos, Custom.RNV() * 5f * Random.value, 1f));
                }
                self.room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));
                for (int j = 0; j < 10; j++)
                {
                    Vector2 a = Custom.RNV();
                    self.room.AddObject(new Spark(pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
                }
                self.room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.3f + Random.value * 0.3f, 0.5f + Random.value * 2f);
                self.room.InGameNoise(new InGameNoise(pos, 8000f, self, 1f));
                int num2 = Mathf.Max(1, cfgArtificerExplosionCapacity - 3);
                if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f)
                {
                    float num3 = (float)self.input[0].x;
                    float num4 = (float)self.input[0].y;
                    while (num3 == 0f && num4 == 0f)
                    {
                        num3 = (float)(((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1));
                        num4 = (float)(((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1));
                    }
                    self.bodyChunks[0].vel.x = 9f * num3;
                    self.bodyChunks[0].vel.y = 9f * num4;
                    self.bodyChunks[1].vel.x = 8f * num3;
                    self.bodyChunks[1].vel.y = 8f * num4;
                    self.pyroJumpCooldown = 150f;
                    self.pyroJumpCounter++;
                }
                else
                {
                    if (self.input[0].x != 0)
                    {
                        self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f;
                        self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f;
                        self.jumpBoost = 6f;
                    }
                    if (self.input[0].x == 0 || self.input[0].y == 1)
                    {
                        if (self.pyroJumpCounter >= num2)
                        {
                            self.bodyChunks[0].vel.y = 16f;
                            self.bodyChunks[1].vel.y = 15f;
                            self.jumpBoost = 10f;
                        }
                        else
                        {
                            self.bodyChunks[0].vel.y = 11f;
                            self.bodyChunks[1].vel.y = 10f;
                            self.jumpBoost = 8f;
                        }
                    }
                    if (self.input[0].y == 1)
                    {
                        self.bodyChunks[0].vel.x = 10f * (float)self.input[0].x;
                        self.bodyChunks[1].vel.x = 8f * (float)self.input[0].x;
                    }
                    else
                    {
                        self.bodyChunks[0].vel.x = 15f * (float)self.input[0].x;
                        self.bodyChunks[1].vel.x = 13f * (float)self.input[0].x;
                    }
                    self.animation = Player.AnimationIndex.Flip;
                    self.pyroJumpCounter++;
                    self.pyroJumpCooldown = 150f;
                    self.bodyMode = Player.BodyModeIndex.Default;
                }
                if (self.pyroJumpCounter >= num2)
                {
                    self.Stun(60 * (self.pyroJumpCounter - (num2 - 1)));
                }
                if (self.pyroJumpCounter >= cfgArtificerExplosionCapacity)
                {
                    self.PyroDeath();
                }
            }
            else if (flag && !self.submerged && !flag2 && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl) && (self.canJump > 0 || self.input[0].y < 0) && self.Consious && !self.pyroJumpped && self.pyroParryCooldown <= 0f)
            {
                if (self.canJump <= 0)
                {
                    if (self.slugcatStats.name.value == "Metabolite")
                    {
                        self.SubtractFood(1);
                    }
                    self.pyroJumpped = true;
                    self.bodyChunks[0].vel.y = 8f;
                    self.bodyChunks[1].vel.y = 6f;
                    self.jumpBoost = 6f;
                    self.forceSleepCounter = 0;
                }
                if (self.pyroJumpCounter <= num)
                {
                    self.pyroJumpCounter += 2;
                }
                else
                {
                    self.pyroJumpCounter++;
                }
                self.pyroParryCooldown = 40f;
                self.pyroJumpCooldown = 150f;
                Vector2 pos2 = self.firstChunk.pos;
                for (int k = 0; k < 8; k++)
                {
                    self.room.AddObject(new Explosion.ExplosionSmoke(pos2, Custom.RNV() * 5f * Random.value, 1f));
                }
                self.room.AddObject(new Explosion.ExplosionLight(pos2, 160f, 1f, 3, Color.white));
                for (int l = 0; l < 10; l++)
                {
                    Vector2 a2 = Custom.RNV();
                    self.room.AddObject(new Spark(pos2 + a2 * Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
                }
                self.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
                self.room.PlaySound(SoundID.Fire_Spear_Explode, pos2, 0.3f + Random.value * 0.3f, 0.5f + Random.value * 2f);
                self.room.InGameNoise(new InGameNoise(pos2, 8000f, self, 1f));
                List<Weapon> list = new List<Weapon>();
                for (int m = 0; m < self.room.physicalObjects.Length; m++)
                {
                    for (int n = 0; n < self.room.physicalObjects[m].Count; n++)
                    {
                        if (self.room.physicalObjects[m][n] is Weapon)
                        {
                            Weapon weapon = self.room.physicalObjects[m][n] as Weapon;
                            if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                            {
                                list.Add(weapon);
                            }
                        }
                        bool flag3;
                        if (ModManager.CoopAvailable && !Custom.rainWorld.options.friendlyFire)
                        {
                            Player player = self.room.physicalObjects[m][n] as Player;
                            flag3 = (player == null || player.isNPC);
                        }
                        else
                        {
                            flag3 = true;
                        }
                        bool flag4 = flag3;
                        if (self.room.physicalObjects[m][n] is Creature && self.room.physicalObjects[m][n] != self && flag4)
                        {
                            Creature creature = self.room.physicalObjects[m][n] as Creature;
                            if (Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (Custom.Dist(pos2, creature.firstChunk.pos) < 60f || self.room.VisualContact(self.abstractCreature.pos, creature.abstractCreature.pos)))
                            {
                                self.room.socialEventRecognizer.WeaponAttack(null, self, creature, true);
                                creature.SetKillTag(self.abstractCreature);
                                if (creature is Scavenger)
                                {
                                    (creature as Scavenger).HeavyStun(80);
                                }
                                else
                                {
                                    creature.Stun(80);
                                }
                                creature.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * 30f;
                                if (creature is TentaclePlant)
                                {
                                    for (int num5 = 0; num5 < creature.grasps.Length; num5++)
                                    {
                                        creature.ReleaseGrasp(num5);
                                    }
                                }
                            }
                        }
                    }
                }
                if (list.Count > 0 && self.room.game.IsArenaSession)
                {
                    self.room.game.GetArenaGameSession.arenaSitting.players[0].parries++;
                }
                for (int num6 = 0; num6 < list.Count; num6++)
                {
                    list[num6].ChangeMode(Weapon.Mode.Free);
                    list[num6].firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, list[num6].firstChunk.pos)) * 20f;
                    list[num6].SetRandomSpin();
                }
                int num7 = Mathf.Max(1, cfgArtificerExplosionCapacity - 3);
                if (self.pyroJumpCounter >= num7)
                {
                    self.Stun(60 * (self.pyroJumpCounter - (num7 - 1)));
                }
                if (self.pyroJumpCounter >= cfgArtificerExplosionCapacity)
                {
                    self.PyroDeath();
                }
            }
            if (self.canJump > 0 || !self.Consious || self.Stunned || self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.ClimbOnBeam || self.bodyMode == Player.BodyModeIndex.WallClimb || self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.VineGrab || self.animation == Player.AnimationIndex.ZeroGPoleGrab || self.bodyMode == Player.BodyModeIndex.Swimming || ((self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity <= 0.5f || self.gravity <= 0.5f) && (self.wantToJump == 0 || !self.input[0].pckp)))
            {
                self.pyroJumpped = false;
            }
        }
    }

    public static AbstractPhysicalObject.AbstractObjectType CraftingResults(Player self)
    {
        if (self.FoodInStomach > 0)
        {
            Creature.Grasp[] grasps = self.grasps;
            for (int i = 0; i < grasps.Length; i++)
            {
                if (grasps[i] != null && grasps[i].grabbed is IPlayerEdible && (grasps[i].grabbed as IPlayerEdible).Edible)
                {
                    return null;
                }
            }
            if (grasps[0] != null && grasps[0].grabbed is Spear && !(grasps[0].grabbed as Spear).abstractSpear.explosive)
            {
                return AbstractPhysicalObject.AbstractObjectType.Spear;
            }
            if (grasps[0] == null && grasps[1] != null && grasps[1].grabbed is Spear && !(grasps[1].grabbed as Spear).abstractSpear.explosive && self.objectInStomach == null)
            {
                return AbstractPhysicalObject.AbstractObjectType.Spear;
            }
        }
        return null;
    }
    public static void SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        if (self.slugcatStats.name.value == "Metabolite")
        {
            if (grasp < 0 || self.grasps[grasp] == null)
            {
                return;
            }
            AbstractPhysicalObject abstractPhysicalObject = self.grasps[grasp].grabbed.abstractPhysicalObject;
            if (abstractPhysicalObject is AbstractSpear)
            {
                (abstractPhysicalObject as AbstractSpear).stuckInWallCycles = 0;
            }
            self.objectInStomach = abstractPhysicalObject;
            if (ModManager.MMF && self.room.game.session is StoryGameSession)
            {
                (self.room.game.session as StoryGameSession).RemovePersistentTracker(self.objectInStomach);
            }
            self.ReleaseGrasp(grasp);
            self.objectInStomach.realizedObject.RemoveFromRoom();
            self.objectInStomach.Abstractize(self.abstractCreature.pos);
            self.objectInStomach.Room.RemoveEntity(self.objectInStomach);
            if (ModManager.MSC && true && self.FoodInStomach > 0)
            {
                if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    abstractPhysicalObject = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID());
                    self.SubtractFood(1);
                }
                else if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear && !(abstractPhysicalObject as AbstractSpear).explosive && !(abstractPhysicalObject as AbstractSpear).electric)
                {
                    abstractPhysicalObject = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), true);
                    self.SubtractFood(1);
                }
                else if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
                {
                    abstractPhysicalObject = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                    self.SubtractFood(1);
                }
                else if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
                {
                    abstractPhysicalObject = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID());
                    self.SubtractFood(1);
                }
            }
            self.objectInStomach = abstractPhysicalObject;
            self.objectInStomach.Abstractize(self.abstractCreature.pos);
            BodyChunk mainBodyChunk = self.mainBodyChunk;
            mainBodyChunk.vel.y = mainBodyChunk.vel.y + 2f;
            self.room.PlaySound(SoundID.Slugcat_Swallow_Item, self.mainBodyChunk);
        }
        else
        {
            orig(self, grasp);
        }
    }
}
