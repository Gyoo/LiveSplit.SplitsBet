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
        public readonly static string VersionString = "0.6";
        #if DEBUG
        public readonly static string VersionPostfix = "-dev";
        #else
        public readonly static string VersionPostfix = "";
        #endif

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
            get { return "http://fezmod.tk/files/travis/splitsbet/update/"; }//TODO move away from FEZMod..?
        }

        public Version Version
        {
            get { return Version.Parse(VersionString); }
        }

        public string XMLURL
        {
            get { return UpdateURL + "Components/LiveSplit.SplitsBet.xml"; }
        }
    }
}
