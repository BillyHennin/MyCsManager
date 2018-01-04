// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using FirstFloor.ModernUI.Presentation;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class ModernMenu : Control
    {
        public static readonly DependencyProperty LinkGroupsProperty = DependencyProperty.Register("LinkGroups", typeof(LinkGroupCollection), typeof(ModernMenu),
            new PropertyMetadata(OnLinkGroupsChanged));

        public static readonly DependencyProperty SelectedLinkGroupProperty = DependencyProperty.Register("SelectedLinkGroup", typeof(LinkGroup),
            typeof(ModernMenu), new PropertyMetadata(OnSelectedLinkGroupChanged));

        public static readonly DependencyProperty SelectedLinkProperty = DependencyProperty.Register("SelectedLink", typeof(Link), typeof(ModernMenu),
            new PropertyMetadata(OnSelectedLinkChanged));

        public static readonly DependencyProperty SelectedSourceProperty = DependencyProperty.Register("SelectedSource", typeof(Uri), typeof(ModernMenu),
            new PropertyMetadata(OnSelectedSourceChanged));

        private static readonly DependencyPropertyKey VisibleLinkGroupsPropertyKey = DependencyProperty.RegisterReadOnly("VisibleLinkGroups",
            typeof(ReadOnlyLinkGroupCollection), typeof(ModernMenu), null);

        public static readonly DependencyProperty VisibleLinkGroupsProperty = VisibleLinkGroupsPropertyKey.DependencyProperty;

        private readonly Dictionary<string, ReadOnlyLinkGroupCollection> groupMap = new Dictionary<string, ReadOnlyLinkGroupCollection>();

        private bool isSelecting;

        public ModernMenu()
        {
            DefaultStyleKey = typeof(ModernMenu);

            SetCurrentValue(LinkGroupsProperty, new LinkGroupCollection());
        }

        public LinkGroupCollection LinkGroups
        {
            get { return (LinkGroupCollection) GetValue(LinkGroupsProperty); }
            set { SetValue(LinkGroupsProperty, value); }
        }

        public Link SelectedLink { get { return (Link) GetValue(SelectedLinkProperty); } set { SetValue(SelectedLinkProperty, value); } }

        public Uri SelectedSource { get { return (Uri) GetValue(SelectedSourceProperty); } set { SetValue(SelectedSourceProperty, value); } }

        public LinkGroup SelectedLinkGroup { get { return (LinkGroup) GetValue(SelectedLinkGroupProperty); } }

        public ReadOnlyLinkGroupCollection VisibleLinkGroups { get { return (ReadOnlyLinkGroupCollection) GetValue(VisibleLinkGroupsProperty); } }

        public event EventHandler<SourceEventArgs> SelectedSourceChanged;

        private static void OnLinkGroupsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernMenu) o).OnLinkGroupsChanged((LinkGroupCollection) e.OldValue, (LinkGroupCollection) e.NewValue);
        }

        private void OnLinkGroupsChanged(LinkGroupCollection oldValue, LinkGroupCollection newValue)
        {
            if(oldValue != null)
            {
                oldValue.CollectionChanged -= OnLinkGroupsCollectionChanged;
            }

            if(newValue != null)
            {
                newValue.CollectionChanged += OnLinkGroupsCollectionChanged;
            }

            RebuildMenu(newValue);
        }

        private static void OnSelectedLinkGroupChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var group = (LinkGroup) e.NewValue;
            Link selectedLink = null;
            if(group != null)
            {
                selectedLink = group.SelectedLink;

                if(group.Links != null)
                {
                    if(selectedLink != null && @group.Links.All(l => l != selectedLink))
                    {
                        selectedLink = null;
                    }

                    if(selectedLink == null)
                    {
                        selectedLink = group.Links.FirstOrDefault();
                    }
                }
            }

            o.SetCurrentValue(SelectedLinkProperty, selectedLink);
        }

        private static void OnSelectedLinkChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (Link) e.NewValue;
            Uri selectedSource = null;
            if(newValue != null)
            {
                selectedSource = newValue.Source;
            }
            o.SetCurrentValue(SelectedSourceProperty, selectedSource);
        }

        private void OnLinkGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildMenu((LinkGroupCollection) sender);
        }

        private static void OnSelectedSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernMenu) o).OnSelectedSourceChanged((Uri) e.OldValue, (Uri) e.NewValue);
        }

        private void OnSelectedSourceChanged(Uri oldValue, Uri newValue)
        {
            if(!isSelecting)
            {
                if(newValue != null && newValue.Equals(oldValue))
                {
                    return;
                }

                UpdateSelection();
            }

            var handler = SelectedSourceChanged;
            if(handler != null)
            {
                handler(this, new SourceEventArgs(newValue));
            }
        }

        private static string GetGroupName(LinkGroup group)
        {
            return group.GroupName ?? "<null>";
        }

        private void RebuildMenu(IEnumerable<LinkGroup> groups)
        {
            groupMap.Clear();
            if(groups != null)
            {
                foreach(var group in groups)
                {
                    var groupName = GetGroupName(group);

                    ReadOnlyLinkGroupCollection groupCollection;
                    if(!groupMap.TryGetValue(groupName, out groupCollection))
                    {
                        groupCollection = new ReadOnlyLinkGroupCollection(new LinkGroupCollection());
                        groupMap.Add(groupName, groupCollection);
                    }

                    groupCollection.List.Add(group);
                }
            }

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            LinkGroup selectedGroup = null;
            Link selectedLink = null;

            if(LinkGroups != null)
            {
                var linkInfo = (from g in LinkGroups from l in g.Links where l.Source == SelectedSource select new {Group = g, Link = l}).FirstOrDefault();

                if(linkInfo != null)
                {
                    selectedGroup = linkInfo.Group;
                    selectedLink = linkInfo.Link;
                }
                else
                {
                    selectedGroup = SelectedLinkGroup;

                    if(LinkGroups.All(g => g != selectedGroup))
                    {
                        selectedGroup = LinkGroups.FirstOrDefault();
                    }
                }
            }

            ReadOnlyLinkGroupCollection groups = null;
            if(selectedGroup != null)
            {
                selectedGroup.SelectedLink = selectedLink;

                var groupName = GetGroupName(selectedGroup);
                groupMap.TryGetValue(groupName, out groups);
            }

            isSelecting = true;
            SetValue(VisibleLinkGroupsPropertyKey, groups);
            SetCurrentValue(SelectedLinkGroupProperty, selectedGroup);
            SetCurrentValue(SelectedLinkProperty, selectedLink);
            isSelecting = false;
        }
    }
}