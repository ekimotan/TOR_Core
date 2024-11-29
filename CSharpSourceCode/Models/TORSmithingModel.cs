using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;

namespace TOR_Core.Models
{
    public class TORSmithingModel : DefaultSmithingModel
    {
        public override int GetEnergyCostForRefining(ref Crafting.RefiningFormula refineFormula, Hero hero)
        {
            return 0;
        }

        public override int GetEnergyCostForSmelting(ItemObject item, Hero hero)
        {
            return 0;
        }

        public override int GetEnergyCostForSmithing(ItemObject item, Hero hero)
        {
            return 0;
        }
    }
}
