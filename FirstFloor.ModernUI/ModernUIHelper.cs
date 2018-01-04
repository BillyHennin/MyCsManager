// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.ComponentModel;
using System.Windows;

namespace FirstFloor.ModernUI
{
    public static class ModernUIHelper
    {
        private static bool? isInDesignMode;

        public static bool IsInDesignMode
        {
            get
            {
                if(!isInDesignMode.HasValue)
                {
                    isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
                }
                return isInDesignMode.Value;
            }
        }
    }
}