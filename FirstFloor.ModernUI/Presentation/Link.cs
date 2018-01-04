// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;

namespace FirstFloor.ModernUI.Presentation
{
    public class Link : Displayable
    {
        private bool flash;
        private string name;
        private Uri source;

        public Uri Source
        {
            get { return source; }
            set
            {
                if(source != value)
                {
                    source = value;
                    OnPropertyChanged("Source");
                }
            }
        }

        public bool Flash
        {
            get { return flash; }
            set
            {
                if(flash != value)
                {
                    flash = value;
                    OnPropertyChanged("Flash");
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if(name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
    }
}