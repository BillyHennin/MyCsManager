// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using MANAGER.Classes;
using MANAGER.Properties;

namespace MANAGER.Pages
{
    /// <summary>
    ///   Logique d'interaction pour Parametre.xaml
    /// </summary>
    public partial class Parametre
    {
        private void ComboBoxLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newLang = ComboBoxLang.SelectedItem.ToString();
            switch(newLang)
            {
                case "French":
                    Transharp.SetCurrentLanguage(Transharp.LangsEnum.French);
                    break;
                case "English":
                    Transharp.SetCurrentLanguage(Transharp.LangsEnum.English);
                    break;
                default:
                    Transharp.SetCurrentLanguage(Transharp.LangsEnum.English);
                    newLang = "English";
                    break;
            }
            SelectionTextBlock.Text = Transharp.GetTranslation("PM_SelectionLanguage");

            Settings.Default.Language = newLang;
        }

        private void ComboBoxLang_Initialized(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Language", "*.lang");
            foreach(var file in files)
            {
                ComboBoxLang.Items.Add(file.Split('.')[0].Split('\\').Last());
            }
        }

        private void MenuParametre_Loaded(object sender, RoutedEventArgs e)
        {
            SelectionTextBlock.Text = Transharp.GetTranslation("PM_SelectionLanguage");
        }
    }
}