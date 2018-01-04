// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

using MANAGER.Classes;

namespace MANAGER.ComboBox
{
    internal class ComboboxItemCategory
    {
        public string Text { private get; set; }
        public Category Value { get; set; }

        public override string ToString() => Text;
    }
}