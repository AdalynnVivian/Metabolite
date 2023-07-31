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

public static class Gourmand
{
    public static void ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear) {
        if (ModManager.MSC && self.slugcatStats.name.value == "Metabolite" && self.FoodInStomach >= 1 && (self.input[0].x == 1 || self.input[0].x == -1))
        {
            self.SubtractFood(1);
            spear.spearDamageBonus = 2f;
            if (self.canJump != 0)
            {
                self.animation = Player.AnimationIndex.Roll;
            }
            else
            {
                self.animation = Player.AnimationIndex.Flip;
            }
            if ((self.room != null && self.room.gravity == 0f) || Mathf.Abs(spear.firstChunk.vel.x) < 1f)
            {
                self.firstChunk.vel += spear.firstChunk.vel.normalized * 9f;
            }
            else
            {
                self.rollDirection = (int)Mathf.Sign(spear.firstChunk.vel.x);
                self.rollCounter = 0;
                BodyChunk firstChunk3 = self.firstChunk;
                firstChunk3.vel.x = firstChunk3.vel.x + Mathf.Sign(spear.firstChunk.vel.x) * 9f;
            }
            BodyChunk firstChunk4 = spear.firstChunk;
            firstChunk4.vel.x = firstChunk4.vel.x * 1.2f;
        }
        else
        {
            orig.Invoke(self, spear);
        }
    }
}
