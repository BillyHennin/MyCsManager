// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Shell;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class ModernWindow : Window
    {
        public static readonly DependencyProperty BackgroundContentProperty = DependencyProperty.Register("BackgroundContent", typeof(object),
            typeof(ModernWindow));

        public static readonly DependencyProperty MenuLinkGroupsProperty = DependencyProperty.Register("MenuLinkGroups", typeof(LinkGroupCollection),
            typeof(ModernWindow));

        public static readonly DependencyProperty TitleLinksProperty = DependencyProperty.Register("TitleLinks", typeof(LinkCollection), typeof(ModernWindow));

        public static readonly DependencyProperty IsTitleVisibleProperty = DependencyProperty.Register("IsTitleVisible", typeof(bool), typeof(ModernWindow),
            new PropertyMetadata(false));

        public static readonly DependencyProperty LogoDataProperty = DependencyProperty.Register("LogoData", typeof(Geometry), typeof(ModernWindow));

        public static readonly DependencyProperty LogoCommandProperty = DependencyProperty.Register("LogoCommand", typeof(ICommand), typeof(ModernWindow));

        public static readonly DependencyProperty ContentSourceProperty = DependencyProperty.Register("ContentSource", typeof(Uri), typeof(ModernWindow));

        public static readonly DependencyProperty HomeSourceProperty = DependencyProperty.Register("HomeSource", typeof(Uri), typeof(ModernWindow));

        public static readonly DependencyProperty ContentLoaderProperty = DependencyProperty.Register("ContentLoader", typeof(IContentLoader),
            typeof(ModernWindow), new PropertyMetadata(new DefaultContentLoader()));

        public static readonly DependencyProperty ShowHomeButtonProperty = DependencyProperty.Register("ShowHomeButton", typeof(bool), typeof(ModernWindow));

        public static readonly DependencyProperty ShowBackButtonProperty = DependencyProperty.Register("ShowBackButton", typeof(bool), typeof(ModernWindow));

        public static readonly DependencyProperty ShowRefreshButtonProperty = DependencyProperty.Register("ShowRefreshButton", typeof(bool),
            typeof(ModernWindow));

        public static readonly DependencyProperty ShowForwardButtonProperty = DependencyProperty.Register("ShowForwardButton", typeof(bool),
            typeof(ModernWindow));

        private Storyboard backgroundAnimation;

        public ModernWindow()
        {
            DefaultStyleKey = typeof(ModernWindow);
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernWindow), new FrameworkPropertyMetadata(typeof(ModernWindow)));
            SetCurrentValue(MenuLinkGroupsProperty, new LinkGroupCollection());
            SetCurrentValue(TitleLinksProperty, new LinkCollection());

            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));

            AppearanceManager.Current.PropertyChanged += OnAppearanceManagerPropertyChanged;
        }

        public object BackgroundContent { get { return GetValue(BackgroundContentProperty); } set { SetValue(BackgroundContentProperty, value); } }

        public LinkGroupCollection MenuLinkGroups
        {
            get { return (LinkGroupCollection) GetValue(MenuLinkGroupsProperty); }
            set { SetValue(MenuLinkGroupsProperty, value); }
        }

        public LinkCollection TitleLinks { get { return (LinkCollection) GetValue(TitleLinksProperty); } set { SetValue(TitleLinksProperty, value); } }

        public bool IsTitleVisible { get { return (bool) GetValue(IsTitleVisibleProperty); } set { SetValue(IsTitleVisibleProperty, value); } }

        public Geometry LogoData { get { return (Geometry) GetValue(LogoDataProperty); } set { SetValue(LogoDataProperty, value); } }

        public Uri ContentSource { get { return (Uri) GetValue(ContentSourceProperty); } set { SetValue(ContentSourceProperty, value); } }

        public IContentLoader ContentLoader { get { return (IContentLoader) GetValue(ContentLoaderProperty); } set { SetValue(ContentLoaderProperty, value); } }

        public Uri HomeSource { get { return (Uri) GetValue(HomeSourceProperty); } set { SetValue(HomeSourceProperty, value); } }

        public ICommand LogoCommand { get { return (ICommand) GetValue(LogoCommandProperty); } set { SetValue(LogoCommandProperty, value); } }

        public bool ShowHomeButton { get { return (bool) GetValue(ShowHomeButtonProperty); } set { SetValue(ShowHomeButtonProperty, value); } }

        public bool ShowBackButton { get { return (bool) GetValue(ShowBackButtonProperty); } set { SetValue(ShowBackButtonProperty, value); } }

        public bool ShowRefreshButton { get { return (bool) GetValue(ShowRefreshButtonProperty); } set { SetValue(ShowRefreshButtonProperty, value); } }

        public bool ShowForwardButton { get { return (bool) GetValue(ShowForwardButtonProperty); } set { SetValue(ShowForwardButtonProperty, value); } }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            AppearanceManager.Current.PropertyChanged -= OnAppearanceManagerPropertyChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var border = GetTemplateChild("WindowBorder") as Border;
            if(border != null)
            {
                backgroundAnimation = border.Resources["BackgroundAnimation"] as Storyboard;

                if(backgroundAnimation != null)
                {
                    backgroundAnimation.Begin();
                }
            }
        }

        private void OnAppearanceManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "ThemeSource" && backgroundAnimation != null)
            {
                backgroundAnimation.Begin();
            }
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }
    }
}