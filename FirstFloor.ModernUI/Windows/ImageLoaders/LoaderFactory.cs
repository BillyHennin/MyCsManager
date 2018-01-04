// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;

namespace FirstFloor.ModernUI.Windows.ImageLoaders
{
    internal static class LoaderFactory
    {
        public static ILoader CreateLoader(SourceType sourceType)
        {
            switch(sourceType)
            {
                case SourceType.LocalDisk:
                    return new LocalDiskLoader();
                case SourceType.ExternalResource:
                    return new ExternalLoader();
                default:
                    throw new ApplicationException("Unexpected exception");
            }
        }
    }
}