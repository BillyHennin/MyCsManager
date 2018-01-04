// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FirstFloor.ModernUI.Presentation
{
    public class AppearanceManager : NotifyPropertyChanged
    {
        public const string KeyAccentColor = "AccentColor";

        public const string KeyAccent = "Accent";

        public const string KeyDefaultFontSize = "DefaultFontSize";

        public const string KeyFixedFontSize = "FixedFontSize";

        public static readonly Uri DarkThemeSource = new Uri("/FirstFloor.ModernUI;component/Assets/ModernUI.Dark.xaml", UriKind.Relative);

        public static readonly Uri LightThemeSource = new Uri("/FirstFloor.ModernUI;component/Assets/ModernUI.Light.xaml", UriKind.Relative);

        private static readonly AppearanceManager current = new AppearanceManager();

        private AppearanceManager()
        {
            DarkThemeCommand = new RelayCommand(o => ThemeSource = DarkThemeSource, o => !DarkThemeSource.Equals(ThemeSource));
            LightThemeCommand = new RelayCommand(o => ThemeSource = LightThemeSource, o => !LightThemeSource.Equals(ThemeSource));
            SetThemeCommand = new RelayCommand(o =>
            {
                var uri = o as Uri;
                if(uri != null)
                {
                    ThemeSource = uri;
                }
                else
                {
                    var str = o as string;
                    if(str != null)
                    {
                        Uri source;
                        if(Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out source))
                        {
                            ThemeSource = source;
                        }
                    }
                }
            }, o => o is Uri || o is string);
            LargeFontSizeCommand = new RelayCommand(o => FontSize = FontSize.Large);
            SmallFontSizeCommand = new RelayCommand(o => FontSize = FontSize.Small);
            AccentColorCommand = new RelayCommand(o =>
            {
                if(o is Color)
                {
                    AccentColor = (Color) o;
                }
                else
                {
                    var str = o as string;
                    if(str != null)
                    {
                        AccentColor = (Color) ColorConverter.ConvertFromString(str);
                    }
                }
            }, o => o is Color || o is string);

            ExternalTheme = new List<Theme>();
        }

        public List<Theme> ExternalTheme { get; private set; }

        public static AppearanceManager Current { get { return current; } }

        public ICommand DarkThemeCommand { get; private set; }

        public ICommand LightThemeCommand { get; private set; }

        public ICommand SetThemeCommand { get; private set; }

        public ICommand LargeFontSizeCommand { get; private set; }

        public ICommand SmallFontSizeCommand { get; private set; }

        public ICommand AccentColorCommand { get; private set; }

        public Uri ThemeSource { get { return GetThemeSource(); } set { SetThemeSource(value, true); } }

        public FontSize FontSize { get { return GetFontSize(); } set { SetFontSize(value); } }

        public Color AccentColor { get { return GetAccentColor(); } set { SetAccentColor(value); } }

        private ResourceDictionary GetThemeDictionary()
        {
            return (from dict in Application.Current.Resources.MergedDictionaries where dict.Contains("WindowBackground") select dict).FirstOrDefault();
        }

        private Uri GetThemeSource()
        {
            var dict = GetThemeDictionary();
            if(dict != null)
            {
                return dict.Source;
            }

            return null;
        }

        private void SetThemeSource(Uri source, bool useThemeAccentColor)
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
            }

            var oldThemeDict = GetThemeDictionary();
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var themeDict = new ResourceDictionary {Source = source};

            var accentColor = themeDict[KeyAccentColor] as Color?;
            if(accentColor.HasValue)
            {
                themeDict.Remove(KeyAccentColor);

                if(useThemeAccentColor)
                {
                    ApplyAccentColor(accentColor.Value);
                }
            }

            dictionaries.Add(themeDict);

            if(oldThemeDict != null)
            {
                dictionaries.Remove(oldThemeDict);
            }

            OnPropertyChanged("ThemeSource");
        }

        private void ApplyAccentColor(Color accentColor)
        {
            Application.Current.Resources[KeyAccentColor] = accentColor;
            Application.Current.Resources[KeyAccent] = new SolidColorBrush(accentColor);
        }

        private FontSize GetFontSize()
        {
            var defaultFontSize = Application.Current.Resources[KeyDefaultFontSize] as double?;

            if(defaultFontSize.HasValue)
            {
                return defaultFontSize.Value == 12D ? FontSize.Small : FontSize.Large;
            }

            return FontSize.Large;
        }

        private void SetFontSize(FontSize fontSize)
        {
            if(GetFontSize() == fontSize)
            {
                return;
            }

            Application.Current.Resources[KeyDefaultFontSize] = fontSize == FontSize.Small ? 12D : 13D;
            Application.Current.Resources[KeyFixedFontSize] = fontSize == FontSize.Small ? 10.667D : 13.333D;

            OnPropertyChanged("FontSize");
        }

        private Color GetAccentColor()
        {
            var accentColor = Application.Current.Resources[KeyAccentColor] as Color?;

            if(accentColor.HasValue)
            {
                return accentColor.Value;
            }

            return Color.FromArgb(0xff, 0x1b, 0xa1, 0xe2);
        }

        private void SetAccentColor(Color value)
        {
            ApplyAccentColor(value);

            var themeSource = GetThemeSource();
            if(themeSource != null)
            {
                SetThemeSource(themeSource, false);
            }

            OnPropertyChanged("AccentColor");
        }
    }
}