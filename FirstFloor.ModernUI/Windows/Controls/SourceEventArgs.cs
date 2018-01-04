// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class SourceEventArgs : EventArgs
    {
        public SourceEventArgs(Uri source)
        {
            Source = source;
        }

        public Uri Source { get; private set; }
    }
}