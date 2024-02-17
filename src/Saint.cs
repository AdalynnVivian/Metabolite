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

public static class Saint
{
    public static void Player_ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
    {
        orig.Invoke(player, abstractCreature, world);
        player.tongue = new Player.Tongue(player, 0);
    }
    //A method used in various(2) places to check if the tongue can be shot
    public static bool Player_SaintTongueCheck(On.Player.orig_SaintTongueCheck orig, Player player)
    {
        if (player.slugcatStats.name.value != "metabolite" && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
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
        if (player.slugcatStats.name.value != "metabolite" && player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
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
        if (self.player.slugcatStats.name.value != "metabolite" )//&& self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
        {
            return;
        }
        self.ropeSegments = new PlayerGraphics.RopeSegment[20];
        for (int k = 0; k < self.ropeSegments.Length; k++)
        {
            self.ropeSegments[k] = new PlayerGraphics.RopeSegment(k, self);
        }
        self.tailSpecks = new PlayerGraphics.TailSpeckles(self, 12);
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
        if (self.player.slugcatStats.name.value != "metabolite")// && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
        {
            return;
        }
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

        //Set the value of this dictionary to the last sprite
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
        if (self.player.slugcatStats.name.value != "metabolite")// && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
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
        if (self.player.slugcatStats.name.value != "metabolite")// && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
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
        if (self.player.slugcatStats.name.value != "metabolite")// && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
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

}
