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
        public static IEnumerable<CodeInstruction> ShipDrawTopLayerTranspilerRenderVanillaOrContinue(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originalMethod)
        {
            return new SequenceBlockMatcher<CodeInstruction>(instructions)
                .Find(
                   ILMatches.LdcR8(1),
                   ILMatches.Stloc(11).Anchor(out var anchor)
                )
                .Find(
                    ILMatches.Ldloc(3).CreateLabel(generator, out Label loopContinue),
                    ILMatches.LdcI4(1),
                    ILMatches.Instruction(OpCodes.Add),
                    ILMatches.Stloc(3)
                    )
                .Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.ExcludingInsertion,
                [
                   new CodeInstruction(OpCodes.Ldarg, 0),
                   new CodeInstruction(OpCodes.Ldarg, 1),
                   new CodeInstruction(OpCodes.Ldarg, 2),
                   new CodeInstruction(OpCodes.Ldarg, 3),
                   new CodeInstruction(OpCodes.Ldloc, 3),
                   new CodeInstruction(OpCodes.Call, typeof(RenderPatches).GetMethod(nameof(ShipDrawTopLayerTranspiler_RenderPartCustom), BindingFlags.NonPublic | BindingFlags.Static)),
                ])
                .Anchors().PointerMatcher(anchor)
                .Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.ExcludingInsertion,
                [
                   new CodeInstruction(OpCodes.Ldarg, 0),
                   new CodeInstruction(OpCodes.Ldarg, 1),
                   new CodeInstruction(OpCodes.Ldarg, 2),
                   new CodeInstruction(OpCodes.Ldarg, 3),
                   new CodeInstruction(OpCodes.Ldloc, 3),
                   new CodeInstruction(OpCodes.Call, typeof(RenderPatches).GetMethod(nameof(ShipDrawTopLayerTranspiler_DoVanillaRendering), BindingFlags.NonPublic | BindingFlags.Static)),
                   new CodeInstruction(OpCodes.Brfalse, loopContinue),
                ])
                .AllElements();
        }

        private static void Debug(int i)
        {
            Console.WriteLine(i);
        }

        private static void ShipDrawTopLayerTranspiler_RenderPartCustom(Ship ship, G g, Vec v, Vec worldPos, int i)
        {
            PMod.Api.TryProxy<ICustomPart>(ship.parts[i-1], out var proxy);
            if (proxy == null) return;
            proxy.Render(ship, i-1, g, v, worldPos);
        }
        private static bool ShipDrawTopLayerTranspiler_DoVanillaRendering(Ship ship, G g, Vec v, Vec worldPos, int i)
        {
            PMod.Api.TryProxy<ICustomPart>(ship.parts[i], out var proxy);
            if (proxy == null) return true;
            return proxy.DoVanillaRender(ship, i, g);
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.RenderPartUI)), HarmonyPostfix]
        public static void DrawPartUI(Ship __instance, G g, Combat? combat, Part part, int localX, string keyPrefix, bool isPreview)
        {
            if (PMod.Api.TryProxy<ICustomPart>(part, out var customPart))
            {
                UIKey key = new UIKey(SUK.part, localX, keyPrefix);
                Box box = g.boxes.Find(b => b.key == key) ?? new Box();
                Vec v = g.Peek().xy + new Vec((part.xLerped ?? ((double)localX)) * 16.0, -32.0 + (__instance.isPlayerShip ? part.offset.y : (1.0 + (0.0 - part.offset.y))));
                Vec ttPos = box.rect.xy + new Vec(16.0);

                var tooltips = customPart.GetTooltips(g.state);
                if (tooltips != null && box.IsHover())
                {
                    foreach (var item in tooltips)
                    {
                        g.tooltips.Add(ttPos, item);
                    }
                }
                if (customPart.IsTemporary)
                {
                    if (box.IsHover())
                    {
                        g.tooltips.Add(ttPos, PMod.glossaries["Temp"]);
                    }
                    Color color = new Color(1.0, 1.0, 1.0, 0.8 + Math.Sin(g.state.time * 4.0) * 0.3);
                    Draw.Sprite(SSpr.icons_temporary, v.x + 7, v.y + 14, flipX: false, flipY: false, 0.0, null, null, null, null, color);
                }
                customPart.RenderUI(__instance, g, combat, localX, keyPrefix, isPreview, v);

                
            }
        }
    }
}