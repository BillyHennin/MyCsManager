// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Windows;

namespace FirstFloor.ModernUI.Shell
{
    public class ThumbButtonInfoCollection : FreezableCollection<ThumbButtonInfo>
    {
        private static ThumbButtonInfoCollection s_empty;

        internal static ThumbButtonInfoCollection Empty
        {
            get
            {
                if(s_empty == null)
                {
                    var collection = new ThumbButtonInfoCollection();
                    collection.Freeze();
                    s_empty = collection;
                }
                return s_empty;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ThumbButtonInfoCollection();
        }
    }
}