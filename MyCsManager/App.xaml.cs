// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region

using System;
using System.Windows;
using System.Windows.Media;

using FirstFloor.ModernUI.Presentation;
using static System.Windows.Media.ColorConverter;
using static MANAGER.Properties.Settings;

#endregion

namespace MANAGER
{
    /// <summary>
    ///   Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppearanceManager.Current.AccentColor = (Color) ConvertFromString(Default.AccentColor);
            AppearanceManager.Current.FontSize = Default.FontSize == "Large" ? FontSize.Large : FontSize.Small;
            AppearanceManager.Current.ThemeSource = new Uri(Default.Theme, UriKind.Relative);
        }
    }
}