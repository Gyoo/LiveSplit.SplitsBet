using LiveSplit.SplitsBet;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: ComponentFactory(typeof(SplitsBetFactory))]

namespace LiveSplit.SplitsBet
{
    public class SplitsBetFactory : IComponentFactory
    {
        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public string ComponentName
        {
            get { return "Splits Bet Bot"; }
        }

        public IComponent Create(Model.LiveSplitState state)
        {
            return new SplitsBetComponent(state);
        }

        public string Description
        {
            get { return "Twitch Bot that takes bets on what time will the next split be"; }
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string UpdateURL
        {
            get { return "http://livesplit.org/update/"; }
        }

        public Version Version
        {
            get { return Version.Parse("0.2"); }
        }

        public string XMLURL
        {
            get { return "http://livesplit.org/update/Components/LiveSplit.Remote.xml"; }
        }
    }
}
