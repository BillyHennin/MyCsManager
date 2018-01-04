// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Windows;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class DataGridTextColumn : System.Windows.Controls.DataGridTextColumn
    {
        public DataGridTextColumn()
        {
            ElementStyle = Application.Current.Resources["DataGridTextStyle"] as Style;
            EditingElementStyle = Application.Current.Resources["DataGridEditingTextStyle"] as Style;
        }
    }
}