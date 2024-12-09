using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace TOR_Core.CampaignMechanics.Crafting
{
    public class EnchantingVM : ViewModel
    {
        private Action _closeAction;

        public EnchantingVM(Action closeScreen)
        {
            _closeAction = closeScreen;
        }

        private void ExecuteClose()
        {
            _closeAction();
        }
    }
}
