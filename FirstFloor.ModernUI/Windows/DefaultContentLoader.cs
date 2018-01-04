// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FirstFloor.ModernUI.Windows
{
    public class DefaultContentLoader : IContentLoader
    {
        public Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken)
        {
            if(!Application.Current.Dispatcher.CheckAccess())
            {
                throw new InvalidOperationException(Resources.UIThreadRequired);
            }

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return Task.Factory.StartNew(() => LoadContent(uri), cancellationToken, TaskCreationOptions.None, scheduler);
        }

        protected virtual object LoadContent(Uri uri)
        {
            if(ModernUIHelper.IsInDesignMode)
            {
                return null;
            }
            return Application.LoadComponent(uri);
        }
    }
}