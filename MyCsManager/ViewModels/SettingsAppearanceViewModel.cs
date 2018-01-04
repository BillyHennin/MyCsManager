// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region

using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

using FirstFloor.ModernUI.Presentation;

using MANAGER.Classes;
using MANAGER.Properties;

#endregion

namespace MANAGER.ViewModels
{
    public class SettingsAppearanceViewModel : NotifyPropertyChanged
    {
        private readonly string FontLarge = Transharp.GetTranslation("THM_Large");
        private readonly string FontSmall = Transharp.GetTranslation("THM_Small");
        private readonly Color[] accentColors =
        {
            Color.FromRgb(0xff, 0xff, 0xff), Color.FromRgb(0x64, 0x76, 0x87), Color.FromRgb(0x00, 0x00, 0x00),
            Color.FromRgb(0x82, 0x5a, 0x2c), Color.FromRgb(0x87, 0x79, 0x4e), Color.FromRgb(0x6d, 0x87, 0x64), Color.FromRgb(0x00, 0xab, 0xa9),
            Color.FromRgb(0x00, 0x82, 0xAD), Color.FromRgb(0x00, 0xbc, 0xff), Color.FromRgb(0x00, 0x50, 0xef), Color.FromRgb(0x00, 0x50, 0xa9),
            Color.FromRgb(0x00, 0x20, 0xdc), Color.FromRgb(0x50, 0x00, 0xee), Color.FromRgb(0xaa, 0x00, 0xff), Color.FromRgb(0xf4, 0x72, 0xd0),
            Color.FromRgb(0xd8, 0x00, 0x73), Color.FromRgb(0xa2, 0x00, 0x25), Color.FromRgb(0xff, 0x00, 0x00), Color.FromRgb(0xe5, 0x14, 0x00),
            Color.FromRgb(0xfa, 0x68, 0x00), Color.FromRgb(0xf0, 0xa3, 0x0a), Color.FromRgb(0xe3, 0xc8, 0x00), Color.FromRgb(0xa4, 0xc4, 0x00),
            Color.FromRgb(0x60, 0xa9, 0x17), Color.FromRgb(0x00, 0xb5, 0x00), Color.FromRgb(0x00, 0xff, 0x00), Color.FromRgb(0x10, 0x44, 0x10)
        };
        private readonly LinkCollection themes = new LinkCollection();
        private Color selectedAccentColor;
        private string selectedFontSize;
        private Link selectedTheme;

        public SettingsAppearanceViewModel()
        {
            themes.Add(new Link {DisplayName = Transharp.GetTranslation("THM_Dark"), Source = AppearanceManager.DarkThemeSource});
            themes.Add(new Link {DisplayName = Transharp.GetTranslation("THM_Light"), Source = AppearanceManager.LightThemeSource});

            SelectedFontSize = AppearanceManager.Current.FontSize == FontSize.Large ? FontLarge : FontSmall;
            SyncThemeAndColor();

            AppearanceManager.Current.PropertyChanged += OnAppearanceManagerPropertyChanged;
        }

        public LinkCollection Themes { get { return themes; } }
        public string[] FontSizes { get { return new[] {FontSmall, FontLarge}; } }
        public Color[] AccentColors { get { return accentColors; } }
        public Link SelectedTheme
        {
            get { return selectedTheme; }
            set
            {
                if(selectedTheme == value)
                {
                    return;
                }
                selectedTheme = value;
                Settings.Default.Theme = value.Source.ToString();
                OnPropertyChanged("SelectedTheme");
                AppearanceManager.Current.ThemeSource = value.Source;
            }
        }
        public string SelectedFontSize
        {
            get { return selectedFontSize; }
            set
            {
                if(selectedFontSize == value)
                {
                    return;
                }
                selectedFontSize = value;
                Settings.Default.FontSize = value;
                OnPropertyChanged("SelectedFontSize");
                AppearanceManager.Current.FontSize = value == FontLarge ? FontSize.Large : FontSize.Small;
            }
        }
        public Color SelectedAccentColor
        {
            get { return selectedAccentColor; }
            set
            {
                if(selectedAccentColor == value)
                {
                    return;
                }
                selectedAccentColor = value;
                Settings.Default.AccentColor = value.ToString();
                OnPropertyChanged("SelectedAccentColor");
                AppearanceManager.Current.AccentColor = value;
            }
        }

        private void SyncThemeAndColor()
        {
            SelectedTheme = themes.FirstOrDefault(l => l.Source.Equals(AppearanceManager.Current.ThemeSource));

            SelectedAccentColor = AppearanceManager.Current.AccentColor;
        }

        private void OnAppearanceManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
            if(e.PropertyName == "ThemeSource" || e.PropertyName == "AccentColor")
            {
                SyncThemeAndColor();
            }
        }
    }
}