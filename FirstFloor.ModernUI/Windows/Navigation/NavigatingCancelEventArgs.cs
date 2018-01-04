// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

namespace FirstFloor.ModernUI.Windows.Navigation
{
    public class NavigatingCancelEventArgs : NavigationBaseEventArgs
    {
        public bool IsParentFrameNavigating { get; internal set; }

        public NavigationType NavigationType { get; internal set; }

        public bool Cancel { get; set; }
    }
}