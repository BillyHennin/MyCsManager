// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Windows;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class DataGridComboBoxColumn : System.Windows.Controls.DataGridComboBoxColumn
    {
        public DataGridComboBoxColumn()
        {
            EditingElementStyle = Application.Current.Resources["DataGridEditingComboBoxStyle"] as Style;
        }
    }
}