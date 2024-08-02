using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Nanoray.Pintail;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APurpleApple.ShipPartExpansion
{
    public sealed class ApiImplementation : IShipPartExpansionAPI
    {
        private static readonly Dictionary<Type, ConditionalWeakTable<object, object?>> ProxyCache = [];
        public void RenderPart(Part part, int localX, Ship ship, G g, Vec v, Vec worldPos)
        {
            if (TryProxy<ICustomPart>(part, out var customPart))
            {
                if (customPart.DoVanillaRender(ship, localX, g))
                {
                    DrawPartVanilla(ship, part, localX, g, v, worldPos);
                }

                customPart.Render(ship, localX, g, v, worldPos);
            }
            else
            {
                DrawPartVanilla(ship, part, localX, g, v, worldPos);
            }
        }

        public bool TryProxy<T>(object @object, [MaybeNullWhen(false)] out T proxy) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                proxy = null;
                return false;
            }
            if (!ProxyCache.TryGetValue(typeof(T), out var table))
            {
                table = [];
                ProxyCache[typeof(T)] = table;
            }
            if (table.TryGetValue(@object, out var rawProxy))
            {
                proxy = rawProxy is null ? default : (T)rawProxy;
                return rawProxy is not null;
            }

            var newNullableProxy = PMod.Instance.Helper.Utilities.ProxyManager.TryProxy<string, T>(@object, "Unknown", PMod.Name, out var newProxy) ? newProxy : null;
            table.AddOrUpdate(@object, newNullableProxy);
            proxy = newNullableProxy is null ? default : newNullableProxy;
            return newNullableProxy is not null;
        }

        public void DrawPartVanilla(Ship ship, Part part, int i, G g, Vec v, Vec worldPos)
        {
            Vec vec = worldPos + new Vec(ship.parts.Count * 16 / 2);
            int num = 10;
            if (ship.ai is RailCannon)
            {
                num = 60;
            }

            part.xLerped = Mutil.MoveTowards(part.xLerped ?? ((double)i), i, g.dt * (double)num);
            Vec vec2 = worldPos + new Vec((part.xLerped ?? ((double)i)) * 16.0, -32.0 + (ship.isPlayerShip ? part.offset.y : (1.0 + (0.0 - part.offset.y))));
            Vec vec3 = v + vec2;
            string? skin = part.skin;
            if (skin != null && skin.Contains("crystal"))
            {
                vec3.y += (int)Math.Round(1.45 * Math.Sin(g.state.time * 8.0 + (double)i));
            }

            string? skin2 = part.skin;
            if (skin2 != null && skin2.Contains("dragon"))
            {
                vec3.y += (int)Math.Round(1.45 * Math.Sin(g.state.time * 8.0 + (double)i));
            }

            string? skin3 = part.skin;
            if (skin3 != null && skin3.Contains("tentacle"))
            {
                vec3.y += (int)Math.Round(1.45 * Math.Sin(g.state.time * 8.0 + (double)i));
            }

            if (part.skin == "octoMouth" || part.skin == "octoMouthChargingOff")
            {
                vec3.y += (int)Math.Round(1.45 * Math.Sin(g.state.time * 8.0 + (double)i));
            }

            if (part.skin == "octoMouthCharging")
            {
                vec3.y += (int)Math.Round(1.45 * Math.Sin(g.state.time * 48.0 + (double)i));
            }

            if (part.hilight && g.state.route is Combat combat && combat.PlayerCanAct(g.state))
            {
                Spr? id = Spr.parts_hilight;
                double num2 = (int)vec3.x - 1;
                double y = (int)vec3.y - 1;
                bool flag = !ship.isPlayerShip;
                bool flip = part.flip;
                bool flipY = flag;
                BlendState screen = BlendMode.Screen;
                Draw.Sprite(id, num2, y, flip, flipY, 0.0, null, null, null, null, null, screen);
            }

            if (part.hilightToggle && g.state.route is Combat combat2 && combat2.PlayerCanAct(g.state))
            {
                Spr? id2 = (part.active ? Spr.parts_hilight_toggle_off : Spr.parts_hilight_toggle_on);
                double num3 = (int)vec3.x - 1;
                double y2 = (int)vec3.y - 1;
                bool flag = !ship.isPlayerShip;
                bool flip2 = part.flip;
                bool flipY2 = flag;
                BlendState screen = BlendMode.Screen;
                Draw.Sprite(id2, num3, y2, flip2, flipY2, 0.0, null, null, null, null, null, screen);
            }

            Spr? spr = (part.active ? DB.parts : DB.partsOff).GetOrNull(part.skin ?? part.type.Key());
            ship.ai?.DrawUnderPart(g, ship, i, vec3);
            double num4 = 1.0;
            if (part.skin == "scaffolding_ancient")
            {
                num4 = Mutil.RemapClamped(-1.0, 1.0, 0.6, 1.0, Math.Sin(g.state.time * 3.0));
            }

            Vec vec4 = vec3 + new Vec(-1.0, -1.0 + (double)(ship.isPlayerShip ? 6 : (-6)) * part.pulse).round();
            if (spr == Spr.parts_cannon_drill)
            {
                spr = Ship.drillSprites.GetModulo((int)(g.state.time * 12.0));
            }

            if (ship.ai is FinaleFrienemy finaleFrienemy)
            {
                double num5 = g.state.time * 0.5 + (double)i * 1.2;
                double num6 = Mutil.RemapClamped(0.5, 1.0, 1.0, 0.0, Mutil.Mod(num5, 1.0));
                double num7 = Mutil.RemapClamped(0.0, 0.5, 0.0, 1.0, Mutil.Mod(num5, 1.0));
                Color color = new Color(0.2 + 0.2 * Math.Sin(g.state.time + 0.0 + num5 * 0.5), 0.5, 0.75 + 0.25 * Math.Sin(g.state.time + Math.PI / 2.0 + num5 * 0.5));
                Spr? part2 = finaleFrienemy.GetPart(part.type, (int)num5);
                double num8 = vec4.x;
                double y3 = vec4.y;
                bool flag = !ship.isPlayerShip;
                bool flip3 = part.flip;
                bool flipY3 = flag;
                Color? color2 = color.fadeAlpha(num4 * num6);
                Draw.Sprite(part2, num8, y3, flip3, flipY3, 0.0, null, null, null, null, color2);
                Spr? part3 = finaleFrienemy.GetPart(part.type, (int)num5);
                double num9 = vec4.x;
                double y4 = vec4.y;
                flag = !ship.isPlayerShip;
                bool flip4 = part.flip;
                bool flipY4 = flag;
                color2 = color.gain(num6 * 2.0);
                BlendState screen = BlendMode.Screen;
                Draw.Sprite(part3, num9, y4, flip4, flipY4, 0.0, null, null, null, null, color2, screen);
                Spr? part4 = finaleFrienemy.GetPart(part.type, (int)(num5 + 1.0));
                double num10 = vec4.x;
                double y5 = vec4.y;
                flag = !ship.isPlayerShip;
                bool flip5 = part.flip;
                bool flipY5 = flag;
                color2 = color.fadeAlpha(num4 * num7);
                Draw.Sprite(part4, num10, y5, flip5, flipY5, 0.0, null, null, null, null, color2);
                Spr? part5 = finaleFrienemy.GetPart(part.type, (int)(num5 + 1.0));
                double num11 = vec4.x;
                double y6 = vec4.y;
                flag = !ship.isPlayerShip;
                bool flip6 = part.flip;
                bool flipY6 = flag;
                color2 = color.gain(num7 * 2.0);
                screen = BlendMode.Screen;
                Draw.Sprite(part5, num11, y6, flip6, flipY6, 0.0, null, null, null, null, color2, screen);
            }
            else
            {
                Spr? id3 = spr;
                double num12 = vec4.x;
                double y7 = vec4.y;
                bool flag = !ship.isPlayerShip;
                bool flip7 = part.flip;
                bool flipY7 = flag;
                Color? color2 = new Color(1.0, 1.0, 1.0, num4);
                Draw.Sprite(id3, num12, y7, flip7, flipY7, 0.0, null, null, null, null, color2);
            }

            if (part.intent != null)
            {
                string? skin4 = part.skin;
                if (skin4 != null && skin4.Contains("octoMouthCharging"))
                {
                    for (int num13 = 1; num13 >= 1; num13--)
                    {
                        Vec vec5 = Mutil.RandVel().normalized() * 25.0 * Mutil.NextRand();
                        PFX.combatAdd.Add(new Particle
                        {
                            color = new Color(0.0, 1.0, 0.3),
                            pos = vec2 + new Vec(7.5, 45.0) + vec5,
                            vel = vec5 * -5.0,
                            lifetime = 0.5,
                            size = 2.0 + Mutil.NextRand() * 1.0
                        });
                    }
                }
            }

            Vec vec6 = vec3 + new Vec(8.0, 32 + 12 * ((!ship.isPlayerShip) ? 1 : (-1)));
            if (part.skin == "cockpit_cicada")
            {
                Glow.Draw(vec6, 50.0, new Color("551900"));
            }

            if (part.skin == "cockpit_cicada2")
            {
                Glow.Draw(vec6, 50.0, new Color("085400"));
            }

            if (part.skin == "cockpit_cicada3")
            {
                Glow.Draw(vec6, 50.0, new Color("68002b"));
            }

            if (part.skin == "cockpit_lawless" || part.skin == "cockpit_lawlessGiant")
            {
                Glow.Draw(vec6, 50.0, new Color("550000"));
            }

            if (part.skin == "cockpit_knight" || (part.type == PType.cockpit && part.skin == null))
            {
                Glow.Draw(vec6, 50.0, new Color("001b39"));
            }

            if (part.skin == "cockpit_bubble" || part.skin == "cockpit_gemini")
            {
                Glow.Draw(vec6, 50.0, new Color("110055"));
            }

            if (part.skin == "cockpit_fish")
            {
                Glow.Draw(vec6 + new Vec(0.0, 16.0), 20.0, new Color("5effe9").gain(0.3));
            }

            if (part.skin == "cannon_possum" || part.skin == "cockpit_possumCenterOpen")
            {
                Glow.Draw(vec6 + new Vec(0.0, 4.0), 40.0, new Color("770022").gain(0.9 + Math.Sin(g.state.time * 8.0) * 0.1));
            }

            if (part.skin == "wing_freezeRingA")
            {
                Glow.Draw(vec6 + new Vec(0.0, -12.0), 40.0, new Color(0.0, 0.5, 1.0).gain(0.5));
            }

            if (part.skin == "wing_freezeRingB")
            {
                Glow.Draw(vec6 + new Vec(0.0, -12.0), 80.0, new Color(0.0, 0.5, 1.0).gain(0.5));
            }

            if (part.skin == "missiles_freeze")
            {
                Glow.Draw(vec6 + new Vec(4.0, -24.0), 80.0, new Color(0.0, 0.5, 1.0).gain(0.9 + Math.Sin(g.state.time * 8.0) * 0.1));
            }

            if (part.skin == "scaffolding_rail")
            {
                Glow.Draw(vec6 + new Vec(0.0, -12.0), 50.0, new Color(1.0, 0.0, 0.0).gain(0.4 + Math.Sin(g.state.time * 3.0) * 0.05));
            }

            if (part.skin == "cockpit_rail")
            {
                Glow.Draw(vec6 + new Vec(0.0, 2.0), 30.0, new Color(0.0, 0.5, 1.0).gain(0.3 + Math.Sin(g.state.time * 3.0) * 0.05));
            }

            if (part.skin == "cockpit_dualDropper")
            {
                Glow.Draw(vec6 + new Vec(0.0, 0.0), 30.0, new Color(0.5, 0.0, 1.0).gain(0.5));
            }

            if (part.skin == "cockpit_cloner")
            {
                Glow.Draw(vec6 + new Vec(0.0, -3.0), 25.0, new Color(1.0, 0.75, 0.0).gain(0.5));
                Glow.Draw(vec6 + new Vec(0.0, -39.0), 30.0, new Color(0.0, 0.5, 1.0).gain(0.7));
            }

        }

        public IEnumerable<ICustomPart> GetCustomParts(Ship ship)
        {
            return ship.parts
            .Select(p => TryProxy<ICustomPart>(p, out var newProxy) ? newProxy : null)
            .Where(p => p is not null).Cast<ICustomPart>();
        }

        public ICustomPart? GetCustomPart(Part part)
        {
            if (TryProxy<ICustomPart>(part, out ICustomPart? customPart))
            {
                return customPart;
            }

            return null;
        }
    }
}
