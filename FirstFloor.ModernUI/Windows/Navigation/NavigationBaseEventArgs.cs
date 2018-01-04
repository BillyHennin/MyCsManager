// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;

using FirstFloor.ModernUI.Windows.Controls;

namespace FirstFloor.ModernUI.Windows.Navigation
{
    public abstract class NavigationBaseEventArgs : EventArgs
    {
        public ModernFrame Frame { get; internal set; }

        public Uri Source { get; internal set; }
    }
}