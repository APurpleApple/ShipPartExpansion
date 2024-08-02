using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.ShipPartExpansion.Patches
{
    [HarmonyPatch]
    internal static class RenderPatches
    {
        [HarmonyPatch(typeof(Ship), nameof(Ship.DrawTopLayer)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ShipDrawTopLayerTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originalMethod)
        {
            return new SequenceBlockMatcher<CodeInstruction>(instructions)
                .Find(
                   ILMatches.LdcI4(0).Anchor(out var pointer),
                   ILMatches.Stloc(3),
                   ILMatches.Br
                )
                .Find(
                   ILMatches.Ldloc(3),
                   ILMatches.Ldarg(0),
                   ILMatches.Ldfld(typeof(Ship).GetField(nameof(Ship.parts))!),
                   ILMatches.AnyCall,
                   ILMatches.Blt,
                   ILMatches.Ldloc(0).CreateLabel(generator, out Label loopExit)
                )
                .Anchors()
                .PointerMatcher(pointer)
                .Insert(SequenceMatcherPastBoundsDirection.Before, SequenceMatcherInsertionResultingBounds.JustInsertion,
                [
                   new CodeInstruction(OpCodes.Ldarg, 0),
                   new CodeInstruction(OpCodes.Ldarg, 1),
                   new CodeInstruction(OpCodes.Ldarg, 2),
                   new CodeInstruction(OpCodes.Ldarg, 3),
                   new CodeInstruction(OpCodes.Call, typeof(RenderPatches).GetMethod(nameof(ShipDrawTopLayerTranspiler_RenderParts), BindingFlags.NonPublic | BindingFlags.Static)),
                   new CodeInstruction(OpCodes.Br, loopExit),
                ])
                .AllElements();
        }

        private static void ShipDrawTopLayerTranspiler_RenderParts(Ship ship, G g, Vec v, Vec worldPos)
        {
            var parts = ship.parts.Select(p=>(Pa: p, Depth: PMod.Api.TryProxy<ICustomPart>(p, out var proxy) ? proxy.RenderDepth : 0, Index: ship.parts.IndexOf(p))).OrderByDescending(p=>p.Depth);
            int i = 0;
            foreach(var part in parts)
            {
                PMod.Api.RenderPart(part.Pa, part.Index, ship, g, v, worldPos);
                i++;
            }
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.RenderPartUI)), HarmonyPostfix]
        public static void DrawPartUI(Ship __instance, G g, Combat? combat, Part part, int localX, string keyPrefix, bool isPreview)
        {
            if (PMod.Api.TryProxy<ICustomPart>(part, out var customPart))
            {
                Vec v = g.Peek().xy + new Vec((part.xLerped ?? ((double)localX)) * 16.0, -32.0 + (__instance.isPlayerShip ? part.offset.y : (1.0 + (0.0 - part.offset.y))));
                if (customPart.IsTemporary)
                {
                    Color color = new Color(1.0, 1.0, 1.0, 0.8 + Math.Sin(g.state.time * 4.0) * 0.3);
                    Draw.Sprite(SSpr.icons_temporary, v.x + 7, v.y + 14, flipX: false, flipY: false, 0.0, null, null, null, null, color);
                }
                customPart.RenderUI(__instance, g, combat, localX, keyPrefix, isPreview, v);

            }
        }
    }
}