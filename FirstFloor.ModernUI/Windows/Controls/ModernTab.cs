// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using FirstFloor.ModernUI.Presentation;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class ModernTab : Control
    {
        public static readonly DependencyProperty ContentLoaderProperty = DependencyProperty.Register("ContentLoader", typeof(IContentLoader), typeof(ModernTab),
            new PropertyMetadata(new DefaultContentLoader()));

        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register("Layout", typeof(TabLayout), typeof(ModernTab),
            new PropertyMetadata(TabLayout.Tab));

        public static readonly DependencyProperty LinksProperty = DependencyProperty.Register("Links", typeof(LinkCollection), typeof(ModernTab),
            new PropertyMetadata(OnLinksChanged));

        public static readonly DependencyProperty SelectedSourceProperty = DependencyProperty.Register("SelectedSource", typeof(Uri), typeof(ModernTab),
            new PropertyMetadata(OnSelectedSourceChanged));

        private ListBox linkList;

        public ModernTab()
        {
            DefaultStyleKey = typeof(ModernTab);

            SetCurrentValue(LinksProperty, new LinkCollection());
        }

        public IContentLoader ContentLoader { get { return (IContentLoader) GetValue(ContentLoaderProperty); } set { SetValue(ContentLoaderProperty, value); } }

        public TabLayout Layout { get { return (TabLayout) GetValue(LayoutProperty); } set { SetValue(LayoutProperty, value); } }

        public LinkCollection Links { get { return (LinkCollection) GetValue(LinksProperty); } set { SetValue(LinksProperty, value); } }

        public Uri SelectedSource { get { return (Uri) GetValue(SelectedSourceProperty); } set { SetValue(SelectedSourceProperty, value); } }

        public event EventHandler<SourceEventArgs> SelectedSourceChanged;

        private static void OnLinksChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTab) o).UpdateSelection();
        }

        private static void OnSelectedSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTab) o).OnSelectedSourceChanged((Uri) e.OldValue, (Uri) e.NewValue);
        }

        private void OnSelectedSourceChanged(Uri oldValue, Uri newValue)
        {
            UpdateSelection();

            var handler = SelectedSourceChanged;
            if(handler != null)
            {
                handler(this, new SourceEventArgs(newValue));
            }
        }

        private void UpdateSelection()
        {
            if(linkList == null || Links == null)
            {
                return;
            }

            linkList.SelectedItem = Links.FirstOrDefault(l => l.Source == SelectedSource);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(linkList != null)
            {
                linkList.SelectionChanged -= OnLinkListSelectionChanged;
            }

            linkList = GetTemplateChild("LinkList") as ListBox;
            if(linkList != null)
            {
                linkList.SelectionChanged += OnLinkListSelectionChanged;
            }

            UpdateSelection();
        }

        private void OnLinkListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var link = linkList.SelectedItem as Link;
            if(link != null && link.Source != SelectedSource)
            {
                SetCurrentValue(SelectedSourceProperty, link.Source);
                link.Flash = false;
            }
        }
    }
}