// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

using FirstFloor.ModernUI.Windows.Controls.BBCode;
using FirstFloor.ModernUI.Windows.Navigation;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class BbCodeBlock : TextBlock
    {
        public static DependencyProperty BbCodeProperty = DependencyProperty.Register("BbCode", typeof(string), typeof(BbCodeBlock),
            new PropertyMetadata(OnBBCodeChanged));

        public static DependencyProperty LinkNavigatorProperty = DependencyProperty.Register("LinkNavigator", typeof(ILinkNavigator), typeof(BbCodeBlock),
            new PropertyMetadata(new DefaultLinkNavigator(), OnLinkNavigatorChanged));

        private bool dirty;

        public BbCodeBlock()
        {
            DefaultStyleKey = typeof(BbCodeBlock);

            AddHandler(FrameworkContentElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
            AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnRequestNavigate));
        }

        public string BbCode { get { return (string) GetValue(BbCodeProperty); } set { SetValue(BbCodeProperty, value); } }

        public ILinkNavigator LinkNavigator { get { return (ILinkNavigator) GetValue(LinkNavigatorProperty); } set { SetValue(LinkNavigatorProperty, value); } }

        private static void OnBBCodeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((BbCodeBlock) o).UpdateDirty();
        }

        private static void OnLinkNavigatorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == null)
            {
                throw new ArgumentNullException("e", @"LinkNavigator");
            }

            ((BbCodeBlock) o).UpdateDirty();
        }

        private void OnLoaded(object o, EventArgs e)
        {
            Update();
        }

        private void UpdateDirty()
        {
            dirty = true;
            Update();
        }

        private void Update()
        {
            if(!IsLoaded || !dirty)
            {
                return;
            }

            var bbcode = BbCode;

            Inlines.Clear();

            if(!string.IsNullOrWhiteSpace(bbcode))
            {
                Inline inline;
                try
                {
                    var parser = new BbCodeParser(bbcode, this) {Commands = LinkNavigator.Commands};
                    inline = parser.Parse();
                }
                catch(Exception)
                {
                    inline = new Run {Text = bbcode};
                }
                Inlines.Add(inline);
            }
            dirty = false;
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                LinkNavigator.Navigate(e.Uri, this, e.Target);
            }
            catch(Exception error)
            {
                ModernDialog.ShowMessage(error.Message, ModernUI.Resources.NavigationFailed, MessageBoxButton.OK);
            }
        }
    }
}