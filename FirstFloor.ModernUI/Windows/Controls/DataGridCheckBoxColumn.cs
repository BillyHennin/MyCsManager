// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Windows;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class DataGridCheckBoxColumn : System.Windows.Controls.DataGridCheckBoxColumn
    {
        public DataGridCheckBoxColumn()
        {
            ElementStyle = Application.Current.Resources["DataGridCheckBoxStyle"] as Style;
            EditingElementStyle = Application.Current.Resources["DataGridEditingCheckBoxStyle"] as Style;
        }
    }
}