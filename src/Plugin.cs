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
            //On.Player.GrabUpdate += Player_GrabUpdate;
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
        /*
        // Implement MeanLizards
        private void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if(MeanLizards.TryGet(world.game, out float meanness))
            {
                self.spawnDataEvil = Mathf.Min(self.spawnDataEvil, meanness);
            }
        }


        // Implement SuperJump
        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (SuperJump.TryGet(self, out var power))
            {
                self.jumpBoost *= 1f + power;
            }
        }

        // Implement ExlodeOnDeath
        private void Player_Die(On.Player.orig_Die orig, Player self)
        {
            bool wasDead = self.dead;

            orig(self);

            if(!wasDead && self.dead
                && ExplodeOnDeath.TryGet(self, out bool explode)
                && explode)
            {
                // Adapted from ScavengerBomb.Explode
                var room = self.room;
                var pos = self.mainBodyChunk.pos;
                var color = self.ShortCutColor();
                room.AddObject(new Explosion(room, self, pos, 7, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f));
                room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, color));
                room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                room.AddObject(new ExplosionSpikes(room, pos, 14, 30f, 9f, 7f, 170f, color));
                room.AddObject(new ShockWave(pos, 330f, 0.045f, 5, false));

                room.ScreenMovement(pos, default, 1.3f);
                room.PlaySound(SoundID.Bomb_Explode, pos);
                room.InGameNoise(new Noise.InGameNoise(pos, 9000f, self, 1f));
            }
        }*/
        //Creating the saint tongue
        /*
        public static void Player_ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(player, abstractCreature, world);
            player.tongue = new Player.Tongue(player, 0);
        }
        #region Saint
        //A method used in various(2) places to check if the tongue can be shot
        public static bool Player_SaintTongueCheck(On.Player.orig_SaintTongueCheck orig, Player player)
        {
            if(player.slugcatStats.name.value != "Metabolite" && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return false;
            }
            return player.Consious && player.tongue.mode == Player.Tongue.Mode.Retracted && player.bodyMode != Player.BodyModeIndex.CorridorClimb && !player.corridorDrop && player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && player.bodyMode != Player.BodyModeIndex.WallClimb && player.bodyMode != Player.BodyModeIndex.Swimming && player.animation != Player.AnimationIndex.VineGrab && player.animation != Player.AnimationIndex.ZeroGPoleGrab;
        }

        //Checking for player tongue imput (the default, not the Remix old tongue controls)
        //((the Remix old tongue controls only require the SaintTongueCheck hook))
        public static void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player player)
        {
            orig.Invoke(player);
            if (player.slugcatStats.name.value != "Metabolite" && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return;
            }
            if (!MMF.cfgOldTongue.Value && player.input[0].jmp && !player.input[1].jmp && !player.input[0].pckp && player.canJump <= 0 && player.bodyMode != Player.BodyModeIndex.Crawl && player.animation != Player.AnimationIndex.ClimbOnBeam && player.animation != Player.AnimationIndex.AntlerClimb && player.animation != Player.AnimationIndex.HangFromBeam && player.SaintTongueCheck())
            {
                Vector2 vector = new Vector2(player.flipDirection, 0.7f);
                Vector2 normalized = vector.normalized;
                if (player.input[0].y > 0)
                {
                    normalized = new Vector2(0f, 1f);
                }
                normalized = (normalized + player.mainBodyChunk.vel.normalized * 0.2f).normalized;
                player.tongue.Shoot(normalized);
            }
        }

        //Initializing the ropeSegments for the tongue sprite
        public static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig.Invoke(self, ow);
            if (self.player.slugcatStats.name.value != "Metabolite" && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return;
            }
            self.ropeSegments = new PlayerGraphics.RopeSegment[20];
            for (int k = 0; k < self.ropeSegments.Length; k++)
            {
                self.ropeSegments[k] = new PlayerGraphics.RopeSegment(k, self);
            }
        }
        //This dictionary exists to make sure we always get the correct index of our new tongue sprite
        public static Dictionary<PlayerGraphics, int> TongueSpriteIndex = new();

        //Making it so theres a correct amount of sprites, cause the tongue is a sprite on the player
        public static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            //Note: the game's code uses sLeaser.sprite[12] for saint as the tongue
            orig.Invoke(self, sLeaser, rCam);
            //We dont want to add a sprite if the player is saint, since they already have a tongue
            //We're adding one sprite
            if (self.player.slugcatStats.name.value != "Metabolite" && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return;
            }
            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

            //Set the value of self dictionary to the last sprite
            if (TongueSpriteIndex.ContainsKey(self)) TongueSpriteIndex[self] = sLeaser.sprites.Length - 1;
            else TongueSpriteIndex.Add(self, sLeaser.sprites.Length - 1);

            //We make the last sprite of the array the tongue mesh
            sLeaser.sprites[TongueSpriteIndex[self]] = TriangleMesh.MakeLongMesh(self.ropeSegments.Length - 1, false, true);

            //Manually add the sprites to a container here cause it creates 1 less headache
            //Remove it first, cause it was added earlier (in the orig method)
            sLeaser.sprites[TongueSpriteIndex[self]].RemoveFromContainer();
            //Then add it to where we want it to be 
            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[TongueSpriteIndex[self]]);
        }

        //Updating the tongue
        public static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.slugcatStats.name.value != "Metabolite" && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return;
            }
            //All of self is copied from the game's code
            Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
            Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
            float b = Mathf.Lerp(self.lastStretch, self.stretch, timeStacker);
            vector = Vector2.Lerp(self.ropeSegments[0].lastPos, self.ropeSegments[0].pos, timeStacker);
            vector += Custom.DirVec(Vector2.Lerp(self.ropeSegments[1].lastPos, self.ropeSegments[1].pos, timeStacker), vector) * 1f;
            float num7 = 0f;
            for (int k = 1; k < self.ropeSegments.Length; k++)
            {
                float num8 = (float)k / (float)(self.ropeSegments.Length - 1);
                if (k >= self.ropeSegments.Length - 2)
                {
                    vector2 = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
                }
                else
                {
                    vector2 = Vector2.Lerp(self.ropeSegments[k].lastPos, self.ropeSegments[k].pos, timeStacker);
                }
                Vector2 a2 = Custom.PerpendicularVector((vector - vector2).normalized);
                float d4 = 0.2f + 1.6f * Mathf.Lerp(1f, b, Mathf.Pow(Mathf.Sin(num8 * 3.1415927f), 0.7f));
                Vector2 vector11 = vector - a2 * d4;
                Vector2 vector12 = vector2 + a2 * d4;
                float num9 = Mathf.Sqrt(Mathf.Pow(vector11.x - vector12.x, 2f) + Mathf.Pow(vector11.y - vector12.y, 2f));
                if (!float.IsNaN(num9))
                {
                    num7 += num9;
                }
                (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4, vector11 - camPos);
                (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4 + 1, vector + a2 * d4 - camPos);
                (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4 + 2, vector2 - a2 * d4 - camPos);
                (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).MoveVertice((k - 1) * 4 + 3, vector12 - camPos);
                vector = vector2;
            }
            if (self.player.tongue.Free || self.player.tongue.Attached)
            {
                sLeaser.sprites[TongueSpriteIndex[self]].isVisible = true;
            }
            else
            {
                sLeaser.sprites[TongueSpriteIndex[self]].isVisible = false;
            }
        }

        //Coloring the tongue
        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
            if (self.player.slugcatStats.name.value != "Metabolite" && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return;
            }
            //Relevant parts copied from the game's code
            float a = 0.95f;
            float b = 1f;
            float sl = 1f;
            float a2 = 0.75f;
            float b2 = 0.9f;
            for (int j = 0; j < (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).verticeColors.Length; j++)
            {
                float num2 = Mathf.Clamp(Mathf.Sin((float)j / (float)((sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).verticeColors.Length - 1) * 3.1415927f), 0f, 1f);
                (sLeaser.sprites[TongueSpriteIndex[self]] as TriangleMesh).verticeColors[j] = Color.Lerp(palette.fogColor, Custom.HSL2RGB(Mathf.Lerp(a, b, num2), sl, Mathf.Lerp(a2, b2, Mathf.Pow(num2, 0.15f))), 0.7f);
            }
        }

        //Necessary rope segment updates for the tongue sprite
        public static void PlayerGraphics_MSCUpdate(On.PlayerGraphics.orig_MSCUpdate orig, PlayerGraphics self)
        {
            orig.Invoke(self);
            if (     && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return;
            }
            //Relevant saint portion copied from the game's code
            self.lastStretch = self.stretch;
            self.stretch = self.RopeStretchFac;
            List<Vector2> list = new List<Vector2>();
            for (int j = self.player.tongue.rope.TotalPositions - 1; j > 0; j--)
            {
                list.Add(self.player.tongue.rope.GetPosition(j));
            }
            list.Add(self.player.mainBodyChunk.pos);
            float num = 0f;
            for (int k = 1; k < list.Count; k++)
            {
                num += Vector2.Distance(list[k - 1], list[k]);
            }
            float num2 = 0f;
            for (int l = 0; l < list.Count; l++)
            {
                if (l > 0)
                {
                    num2 += Vector2.Distance(list[l - 1], list[l]);
                }
                self.AlignRope(num2 / num, list[l]);
            }
            for (int m = 0; m < self.ropeSegments.Length; m++)
            {
                self.ropeSegments[m].Update();
            }
            for (int n = 1; n < self.ropeSegments.Length; n++)
            {
               self.ConnectRopeSegments(n, n - 1);
            }
            for (int num3 = 0; num3 < self.ropeSegments.Length; num3++)
            {
                self.ropeSegments[num3].claimedForBend = false;
            }
        }
        #endregion*/
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