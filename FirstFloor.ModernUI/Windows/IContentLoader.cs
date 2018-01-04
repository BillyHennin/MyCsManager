// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstFloor.ModernUI.Windows
{
    public interface IContentLoader
    {
        Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken);
    }
}