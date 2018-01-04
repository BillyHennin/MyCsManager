// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FirstFloor.ModernUI.Windows.Media
{
    public static class VisualTreeHelperEx
    {
        public static VisualStateGroup TryGetVisualStateGroup(this DependencyObject dependencyObject, string groupName)
        {
            var root = GetImplementationRoot(dependencyObject);
            if(root == null)
            {
                return null;
            }
            return
                (from @group in VisualStateManager.GetVisualStateGroups(root).OfType<VisualStateGroup>()
                    where string.CompareOrdinal(groupName, @group.Name) == 0
                    select @group).FirstOrDefault<VisualStateGroup>();
        }

        public static FrameworkElement GetImplementationRoot(this DependencyObject dependencyObject)
        {
            if(1 != VisualTreeHelper.GetChildrenCount(dependencyObject))
            {
                return null;
            }
            return (VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement);
        }

        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject dependencyObject)
        {
            var parent = dependencyObject;
            while(true)
            {
                parent = GetParent(parent);
                if(parent != null)
                {
                    yield return parent;
                }
                else
                {
                    break;
                }
            }
        }

        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject dependencyObject)
        {
            if(dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }

            var parent = dependencyObject;
            while(true)
            {
                if(parent != null)
                {
                    yield return parent;
                }
                else
                {
                    break;
                }
                parent = GetParent(parent);
            }
        }

        public static DependencyObject GetParent(this DependencyObject dependencyObject)
        {
            if(dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }

            var ce = dependencyObject as ContentElement;
            if(ce != null)
            {
                var parent = ContentOperations.GetParent(ce);
                if(parent != null)
                {
                    return parent;
                }

                var fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            return VisualTreeHelper.GetParent(dependencyObject);
        }
    }
}