// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region

//using System.Windows;
using System;
using System.Windows;

using MANAGER.Classes;

#endregion

namespace MANAGER
{
    /// <summary>
    ///   Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private void ModernWindow_Initialized(object sender, EventArgs e)
        {
            //Connection string
            Properties.Connection.Default.DatabaseConnectionString = "user id=SLAM3;password=pw;data source=localhost:1521/xe";
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Load every traduction for links
            Link_About.DisplayName = Transharp.GetTranslation("Link_About");
            Link_Apparence.DisplayName = Transharp.GetTranslation("Link_Apparence");
            Link_Param.DisplayName = Transharp.GetTranslation("Link_Param");
            Link_CE.DisplayName = Transharp.GetTranslation("Link_CE");
            Link_CreateCE.DisplayName = Transharp.GetTranslation("Link_CreateCE");
            Link_Customer.DisplayName = Transharp.GetTranslation("Link_Customer");
            Link_DisplayCustomer.DisplayName = Transharp.GetTranslation("Link_DisplayCustomer");
            Link_CreateCustomer.DisplayName = Transharp.GetTranslation("Link_CreateCustomer");
            Link_Merchandise.DisplayName = Transharp.GetTranslation("Link_Merchandise");
            Link_DisplayMerchandise.DisplayName = Transharp.GetTranslation("Link_DisplayMerchandise");
        }
    }
}