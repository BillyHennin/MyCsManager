// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Linq;
using System.Windows;

using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Media;

namespace FirstFloor.ModernUI.Windows.Navigation
{
    public static class NavigationHelper
    {
        public const string FrameSelf = "_self";
        public const string FrameTop = "_top";
        public const string FrameParent = "_parent";

        public static ModernFrame FindFrame(string name, FrameworkElement context)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context");
            }

            var frames = context.Ancestors().OfType<ModernFrame>().ToArray();

            if(name == null || name == "_self")
            {
                return frames.FirstOrDefault();
            }
            if(name == "_parent")
            {
                return frames.Skip(1).FirstOrDefault();
            }
            if(name == "_top")
            {
                return frames.LastOrDefault();
            }

            var frame = frames.FirstOrDefault(f => f.Name == name);

            if(frame == null)
            {
                frame = context.FindName(name) as ModernFrame;

                if(frame == null)
                {
                    var parent = frames.FirstOrDefault();
                    if(parent != null && parent.Content != null)
                    {
                        var content = parent.Content as FrameworkElement;
                        if(content != null)
                        {
                            frame = content.FindName(name) as ModernFrame;
                        }
                    }
                }
            }

            return frame;
        }

        public static Uri RemoveFragment(Uri uri)
        {
            string fragment;
            return RemoveFragment(uri, out fragment);
        }

        public static Uri RemoveFragment(Uri uri, out string fragment)
        {
            fragment = null;

            if(uri != null)
            {
                var value = uri.OriginalString;

                var i = value.IndexOf('#');
                if(i != -1)
                {
                    fragment = value.Substring(i + 1);
                    uri = new Uri(value.Substring(0, i), uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
                }
            }

            return uri;
        }
    }
}