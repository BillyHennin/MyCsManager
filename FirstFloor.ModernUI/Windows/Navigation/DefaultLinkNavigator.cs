// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using FirstFloor.ModernUI.Presentation;

namespace FirstFloor.ModernUI.Windows.Navigation
{
    public class DefaultLinkNavigator : ILinkNavigator
    {
        private CommandDictionary commands = new CommandDictionary();
        private string[] externalSchemes = {Uri.UriSchemeHttp, Uri.UriSchemeHttps, Uri.UriSchemeMailto};

        public DefaultLinkNavigator()
        {
            Commands.Add(new Uri("cmd://accentcolor"), AppearanceManager.Current.AccentColorCommand);
            Commands.Add(new Uri("cmd://darktheme"), AppearanceManager.Current.DarkThemeCommand);
            Commands.Add(new Uri("cmd://largefontsize"), AppearanceManager.Current.LargeFontSizeCommand);
            Commands.Add(new Uri("cmd://lighttheme"), AppearanceManager.Current.LightThemeCommand);
            Commands.Add(new Uri("cmd://settheme"), AppearanceManager.Current.SetThemeCommand);
            Commands.Add(new Uri("cmd://smallfontsize"), AppearanceManager.Current.SmallFontSizeCommand);

            commands.Add(new Uri("cmd://browseback"), NavigationCommands.BrowseBack);
            commands.Add(new Uri("cmd://refresh"), NavigationCommands.Refresh);

            commands.Add(new Uri("cmd://copy"), ApplicationCommands.Copy);
        }

        public string[] ExternalSchemes { get { return externalSchemes; } set { externalSchemes = value; } }

        public CommandDictionary Commands { get { return commands; } set { commands = value; } }

        public virtual void Navigate(Uri uri, FrameworkElement source = null, string parameter = null)
        {
            if(uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            ICommand command;
            if(commands != null && commands.TryGetValue(uri, out command))
            {
                if(command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
            else if(uri.IsAbsoluteUri && externalSchemes != null && externalSchemes.Any(s => uri.Scheme.Equals(s, StringComparison.OrdinalIgnoreCase)))
            {
                Process.Start(uri.AbsoluteUri);
            }
            else
            {
                if(source == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Resources.NavigationFailedSourceNotSpecified, uri));
                }

                var frame = NavigationHelper.FindFrame(parameter, source);
                if(frame == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Resources.NavigationFailedFrameNotFound, uri, parameter));
                }

                frame.Source = uri;
            }
        }
    }
}