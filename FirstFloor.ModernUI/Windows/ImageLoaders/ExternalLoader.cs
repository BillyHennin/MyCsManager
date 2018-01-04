// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.IO;
using System.Net;

namespace FirstFloor.ModernUI.Windows.ImageLoaders
{
    internal class ExternalLoader : ILoader
    {
        public Stream Load(string source)
        {
            var webClient = new WebClient();
            var html = webClient.DownloadData(source);
            return new MemoryStream(html);
        }
    }
}