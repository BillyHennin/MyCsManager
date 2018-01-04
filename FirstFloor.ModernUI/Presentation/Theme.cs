// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

namespace FirstFloor.ModernUI.Presentation
{
    public class Theme : NotifyPropertyChanged
    {
        private string themeName;
        private string themeUrl;

        public string ThemeUrl
        {
            get { return themeUrl; }
            set
            {
                if(themeUrl != value)
                {
                    themeUrl = value;
                    OnPropertyChanged("ThemeUrl");
                }
            }
        }

        public string ThemeName
        {
            get { return themeName; }
            set
            {
                if(themeName != value)
                {
                    themeName = value;
                    OnPropertyChanged("ThemeName");
                }
            }
        }
    }
}