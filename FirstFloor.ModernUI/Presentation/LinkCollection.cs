// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FirstFloor.ModernUI.Presentation
{
    public class LinkCollection : ObservableCollection<Link>
    {
        public LinkCollection() {}

        public LinkCollection(IEnumerable<Link> links)
        {
            if(links == null)
            {
                throw new ArgumentNullException("links");
            }
            foreach(var link in links)
            {
                Add(link);
            }
        }
    }
}