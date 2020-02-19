using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace RaptorLibrary
{
    class Main : Script
    {
        readonly string pageName = "Main";
        readonly string ModName = "RaptorLibrary";
        readonly string Developer = "CptnRaptor";
        readonly string Version = "0.1";

        public Main()
        {
            Tick += ModLoaded;
            Interval = 1;
        }

        private void ModLoaded(object sender, EventArgs e)
        {
            if (!Game.IsLoading && Game.Player != null && Game.Player.CanControlCharacter)
            {
                Tick -= ModLoaded;
                //LoggingTools.TextPlayerMessage(ModName, pageName, "onTick", $"{ModName} {Version} by {Developer} Loaded");
                //LoggingTools.WriteMessageToDisk(ModName, $"{ModName} {Version} by {Developer} Loaded");
            }
        }
    }
}
