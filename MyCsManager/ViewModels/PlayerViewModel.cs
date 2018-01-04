// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows;

#endregion

namespace MANAGER.ViewModels
{
    internal class PlayerViewModel : IViewModel
    {
        private string descSource;
        private string imageSource;
        private string infoSource;
        private string installSource;
        private string titleSource;

        public PlayerViewModel(string fragment)
        {
            VmName = fragment;
            ViewModelService.Current.AddViewModel(this, typeof(PlayerViewModel));
        }

        public string ImageSource
        {
            get { return imageSource; }
            set
            {
                if(imageSource == value)
                {
                    return;
                }
                imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }
        public string TitleSource
        {
            get { return titleSource; }
            set
            {
                if(titleSource == value)
                {
                    return;
                }
                titleSource = value;
                OnPropertyChanged("TitleSource");
            }
        }
        public string DescSource
        {
            get { return descSource; }
            set
            {
                if(descSource == value)
                {
                    return;
                }
                descSource = value;
                OnPropertyChanged("DescSource");
            }
        }
        public string InfoSource
        {
            get { return infoSource; }
            set
            {
                if(infoSource == value)
                {
                    return;
                }
                infoSource = value;
                OnPropertyChanged("InfoSource");
            }
        }
        public string InstallSource
        {
            get { return installSource; }
            set
            {
                if(installSource == value)
                {
                    return;
                }
                installSource = value;
                OnPropertyChanged("InstallSource");
            }
        }
    }
}