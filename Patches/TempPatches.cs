using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.ShipPartExpansion.Patches
{
    [HarmonyPatch]
    internal class TempPatches
    {
        [HarmonyPatch(typeof(Combat), nameof(Combat.PlayerWon)), HarmonyPostfix]
        public static void CombatPlayerWon_Postfix(G g)
        {
            List<ICustomPart> parts = PMod.Api.GetCustomParts(g.state.ship).ToList();
            for (int i = parts.Count-1; i >= 0; i--)
            {
                if (parts[i].IsTemporary)
                {
                    g.state.ship.parts.RemoveAt(i);
                }
            }
        }

        [HarmonyPatch(typeof(Combat), nameof(Combat.EndCombat_CHEAT)), HarmonyPostfix]
        public static void CombatEndCHEAT_Postfix(State s)
        {
            List<ICustomPart> parts = PMod.Api.GetCustomParts(s.ship).ToList();
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i].IsTemporary)
                {
                    s.ship.parts.RemoveAt(i);
                }
            }
        }
    }
}
