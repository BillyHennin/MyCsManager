// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region

using MANAGER.Classes;

#endregion

namespace MANAGER.ComboBox
{
    /// <summary>
    ///   Create it when you want to make a combobox that contains estimates.
    /// </summary>
    internal class ComboboxItemEstimate
    {
        public string Text { private get; set; }
        public Estimate Value { get; set; }

        public override string ToString() => Text;
    }
}