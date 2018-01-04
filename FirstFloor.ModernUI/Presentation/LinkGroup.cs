// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

namespace FirstFloor.ModernUI.Presentation
{
    public class LinkGroup : Displayable
    {
        private readonly LinkCollection links = new LinkCollection();
        private string groupName;
        private Link selectedLink;

        public string GroupName
        {
            get { return groupName; }
            set
            {
                if(groupName != value)
                {
                    groupName = value;
                    OnPropertyChanged("GroupName");
                }
            }
        }

        internal Link SelectedLink
        {
            get { return selectedLink; }
            set
            {
                if(selectedLink != value)
                {
                    selectedLink = value;
                    OnPropertyChanged("SelectedLink");
                }
            }
        }

        public LinkCollection Links { get { return links; } }
    }
}