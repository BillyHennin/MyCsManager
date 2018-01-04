// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using FirstFloor.ModernUI.Windows.Controls;

using MANAGER.Classes;

using Newtonsoft.Json.Linq;

#endregion

namespace MANAGER.Pages
{
    public partial class About
    {
        private void Text_Loaded(object sender, RoutedEventArgs e)
        {
            PanelMOTD.Children.Clear();
            string jsonMotd;
            try
            {
                //JsonMOTD = new WebClient().DownloadString(String.Format("http://billyhennin.github.io/Devis-Manager/MOTD{0}.json", Transharp.getCurrentLanguage()));

                var json = JObject.Parse(new WebClient().DownloadString("http://billyhennin.github.io/Devis-Manager/MOTD.json"));
                jsonMotd = $"{json["date"]} - {json["title"]}\r\n\r\n\t\t{json["tags"]}";
                //JsonMOTD = String.Format("{0} - {1}\r\n\r\n\t\t{2}\r\n\r\n[img]{3}[/img]", json["date"], json["title"], json["tags"], json["image"]);
            }
            catch
            {
                jsonMotd = Transharp.GetTranslation("Curl_Fail_MOTD");
            }

            var motd =
                $"\r\n{Transharp.GetTranslation("AB_MOTD1")}\r\n\r\n{Transharp.GetTranslation("AB_MOTD2")}\r\n\r\n\t{Transharp.GetTranslation("AB_MOTD3")}\r\n\t{Transharp.GetTranslation("AB_MOTD4")}\r\n\t{Transharp.GetTranslation("AB_MOTD5", "[url='https://github.com/BillyHennin/Devis-Manager']GitHub[/url]")}\r\n\r\n{Transharp.GetTranslation("AB_MOTD6")}\r\n\r\n\t{jsonMotd}";

            var panelMessage = new StackPanel();
            var border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2, 2, 1, 0),
                BorderThickness = new Thickness(1),
                Child = panelMessage
            };
            panelMessage.Children.Add(new BbCodeBlock {Margin = new Thickness(5, 2, 0, 0), BbCode = motd});
            PanelMOTD.Children.Add(border);
        }
    }
}