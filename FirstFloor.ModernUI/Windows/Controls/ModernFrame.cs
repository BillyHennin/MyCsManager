// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using FirstFloor.ModernUI.Windows.Media;
using FirstFloor.ModernUI.Windows.Navigation;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class ModernFrame : ContentControl
    {
        public static readonly DependencyProperty KeepAliveProperty = DependencyProperty.RegisterAttached("KeepAlive", typeof(bool?), typeof(ModernFrame),
            new PropertyMetadata(null));

        public static readonly DependencyProperty KeepContentAliveProperty = DependencyProperty.Register("KeepContentAlive", typeof(bool), typeof(ModernFrame),
            new PropertyMetadata(true, OnKeepContentAliveChanged));

        public static readonly DependencyProperty ContentLoaderProperty = DependencyProperty.Register("ContentLoader", typeof(IContentLoader),
            typeof(ModernFrame), new PropertyMetadata(new DefaultContentLoader(), OnContentLoaderChanged));

        private static readonly DependencyPropertyKey IsLoadingContentPropertyKey = DependencyProperty.RegisterReadOnly("IsLoadingContent", typeof(bool),
            typeof(ModernFrame), new PropertyMetadata(false));

        public static readonly DependencyProperty IsLoadingContentProperty = IsLoadingContentPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(ModernFrame),
            new PropertyMetadata(OnSourceChanged));

        private readonly List<WeakReference> childFrames = new List<WeakReference>();
        private readonly Dictionary<Uri, object> contentCache = new Dictionary<Uri, object>();
        private readonly Stack<Uri> historyBack = new Stack<Uri>();
        private readonly Stack<Uri> historyForward = new Stack<Uri>();
        private bool isNavigatingHistory;
        private bool isResetSource;
        private CancellationTokenSource tokenSource;

        public ModernFrame()
        {
            DefaultStyleKey = typeof(ModernFrame);

            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnBrowseBack, OnCanBrowseBack));
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, OnGoToPage, OnCanGoToPage));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseHome, OnBrowseHome, OnCanBrowseHome));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, OnBrowseForward, OnCanBrowseForward));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, OnRefresh, OnCanRefresh));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopy, OnCanCopy));

            Loaded += OnLoaded;
        }

        public bool KeepContentAlive { get { return (bool) GetValue(KeepContentAliveProperty); } set { SetValue(KeepContentAliveProperty, value); } }

        public IContentLoader ContentLoader { get { return (IContentLoader) GetValue(ContentLoaderProperty); } set { SetValue(ContentLoaderProperty, value); } }

        public bool IsLoadingContent { get { return (bool) GetValue(IsLoadingContentProperty); } }

        public Uri Source { get { return (Uri) GetValue(SourceProperty); } set { SetValue(SourceProperty, value); } }

        public event EventHandler<FragmentNavigationEventArgs> FragmentNavigation;

        public event EventHandler<NavigatingCancelEventArgs> Navigating;

        public event EventHandler<NavigationEventArgs> Navigated;

        public event EventHandler<NavigationFailedEventArgs> NavigationFailed;

        private static void OnKeepContentAliveChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernFrame) o).OnKeepContentAliveChanged((bool) e.NewValue);
        }

        private void OnKeepContentAliveChanged(bool keepAlive)
        {
            contentCache.Clear();
        }

        private static void OnContentLoaderChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == null)
            {
                throw new ArgumentNullException("e", @"ContentLoader");
            }
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernFrame) o).OnSourceChanged((Uri) e.OldValue, (Uri) e.NewValue);
        }

        private void OnSourceChanged(Uri oldValue, Uri newValue)
        {
            if(isResetSource || newValue != null && newValue.Equals(oldValue))
            {
                return;
            }

            string newFragment = null;
            var oldValueNoFragment = NavigationHelper.RemoveFragment(oldValue);
            var newValueNoFragment = NavigationHelper.RemoveFragment(newValue, out newFragment);

            if(newValueNoFragment != null && newValueNoFragment.Equals(oldValueNoFragment))
            {
                var args = new FragmentNavigationEventArgs {Fragment = newFragment};

                OnFragmentNavigation(Content as IContent, args);
            }
            else
            {
                var navType = isNavigatingHistory ? NavigationType.Back : NavigationType.New;

                if(!isNavigatingHistory && !CanNavigate(oldValue, newValue, navType))
                {
                    return;
                }

                Navigate(oldValue, newValue, navType);
            }
        }

        private bool CanNavigate(Uri oldValue, Uri newValue, NavigationType navigationType)
        {
            var cancelArgs = new NavigatingCancelEventArgs
            {
                Frame = this,
                Source = newValue,
                IsParentFrameNavigating = true,
                NavigationType = navigationType,
                Cancel = false,
            };
            OnNavigating(Content as IContent, cancelArgs);

            if(cancelArgs.Cancel)
            {
                Debug.WriteLine("Cancelled navigation from '{0}' to '{1}'", oldValue, newValue);

                if(Source != oldValue)
                {
                    Dispatcher.BeginInvoke((Action) (() =>
                    {
                        isResetSource = true;
                        SetCurrentValue(SourceProperty, oldValue);
                        isResetSource = false;
                    }));
                }
                return false;
            }

            return true;
        }

        private void Navigate(Uri oldValue, Uri newValue, NavigationType navigationType)
        {
            Debug.WriteLine("Navigating from '{0}' to '{1}'", oldValue, newValue);

            SetValue(IsLoadingContentPropertyKey, true);

            if(tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource = null;
            }

            if(oldValue != null && navigationType == NavigationType.New)
            {
                historyBack.Push(oldValue);
                historyForward.Clear();
            }

            object newContent = null;

            if(newValue != null)
            {
                var newValueNoFragment = NavigationHelper.RemoveFragment(newValue);

                if(navigationType == NavigationType.Refresh || !contentCache.TryGetValue(newValueNoFragment, out newContent))
                {
                    var localTokenSource = new CancellationTokenSource();
                    tokenSource = localTokenSource;

                    var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    var task = ContentLoader.LoadContentAsync(newValue, tokenSource.Token);

                    task.ContinueWith(t =>
                    {
                        try
                        {
                            if(t.IsCanceled || localTokenSource.IsCancellationRequested)
                            {
                                Debug.WriteLine("Cancelled navigation to '{0}'", newValue);
                            }
                            else if(t.IsFaulted)
                            {
                                var failedArgs = new NavigationFailedEventArgs
                                {
                                    Frame = this,
                                    Source = newValue,
                                    Error = t.Exception.InnerException,
                                    Handled = false
                                };

                                OnNavigationFailed(failedArgs);

                                newContent = failedArgs.Handled ? null : failedArgs.Error;

                                SetContent(newValue, navigationType, newContent, true);
                            }
                            else
                            {
                                newContent = t.Result;
                                if(ShouldKeepContentAlive(newContent))
                                {
                                    contentCache[newValueNoFragment] = newContent;
                                }

                                SetContent(newValue, navigationType, newContent, false);
                            }
                        }
                        finally
                        {
                            if(tokenSource == localTokenSource)
                            {
                                tokenSource = null;
                            }

                            localTokenSource.Dispose();
                        }
                    }, scheduler);
                    return;
                }
            }

            SetContent(newValue, navigationType, newContent, false);
        }

        private void SetContent(Uri newSource, NavigationType navigationType, object newContent, bool contentIsError)
        {
            var oldContent = Content as IContent;

            Content = newContent;

            if(!contentIsError)
            {
                var args = new NavigationEventArgs {Frame = this, Source = newSource, Content = newContent, NavigationType = navigationType};

                OnNavigated(oldContent, newContent as IContent, args);
            }

            SetValue(IsLoadingContentPropertyKey, false);

            if(!contentIsError)
            {
                string fragment;
                NavigationHelper.RemoveFragment(newSource, out fragment);
                if(fragment != null)
                {
                    var fragmentArgs = new FragmentNavigationEventArgs {Fragment = fragment};

                    OnFragmentNavigation(newContent as IContent, fragmentArgs);
                }
            }

            var data = newContent as IData;
            if(data != null)
            {
                data.OnSetData(Application.Current.MainWindow as ModernWindow);
            }
            else
            {
                var main = Application.Current.MainWindow as ModernWindow;
                main.LogoData = Geometry.Parse(ModernUI.Resources.DefaultData);
            }
        }

        private IEnumerable<ModernFrame> GetChildFrames()
        {
            var refs = childFrames.ToArray();
            foreach(var r in refs)
            {
                var valid = false;

                if(r.IsAlive)
                {
                    var frame = (ModernFrame) r.Target;

                    if(NavigationHelper.FindFrame(null, frame) == this)
                    {
                        valid = true;
                        yield return frame;
                    }
                }

                if(!valid)
                {
                    childFrames.Remove(r);
                }
            }
        }

        private void OnFragmentNavigation(IContent content, FragmentNavigationEventArgs e)
        {
            if(content != null)
            {
                content.OnFragmentNavigation(e);
            }

            if(FragmentNavigation != null)
            {
                FragmentNavigation(this, e);
            }
        }

        private void OnNavigating(IContent content, NavigatingCancelEventArgs e)
        {
            foreach(var f in GetChildFrames())
            {
                f.OnNavigating(f.Content as IContent, e);
            }

            e.IsParentFrameNavigating = e.Frame != this;

            if(content != null)
            {
                content.OnNavigatingFrom(e);
            }

            if(Navigating != null)
            {
                Navigating(this, e);
            }
        }

        private void OnNavigated(IContent oldContent, IContent newContent, NavigationEventArgs e)
        {
            if(oldContent != null)
            {
                oldContent.OnNavigatedFrom(e);
            }
            if(newContent != null)
            {
                newContent.OnNavigatedTo(e);
            }

            if(Navigated != null)
            {
                Navigated(this, e);
            }
        }

        private void OnNavigationFailed(NavigationFailedEventArgs e)
        {
            if(NavigationFailed != null)
            {
                NavigationFailed(this, e);
            }
        }

        private bool HandleRoutedEvent(CanExecuteRoutedEventArgs args)
        {
            var originalSource = args.OriginalSource as DependencyObject;

            if(originalSource == null)
            {
                return false;
            }
            return originalSource.AncestorsAndSelf().OfType<ModernFrame>().FirstOrDefault() == this;
        }

        private void OnCanBrowseBack(object sender, CanExecuteRoutedEventArgs e)
        {
            if(HandleRoutedEvent(e))
            {
                e.CanExecute = historyBack.Count > 0;
            }
        }

        private void OnCanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            if(HandleRoutedEvent(e))
            {
                e.CanExecute = Content != null;
            }
        }

        private void OnCanGoToPage(object sender, CanExecuteRoutedEventArgs e)
        {
            if(HandleRoutedEvent(e))
            {
                e.CanExecute = e.Parameter is String || e.Parameter is Uri;
            }
        }

        private void OnCanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            if(HandleRoutedEvent(e))
            {
                e.CanExecute = Source != null;
            }
        }

        private void OnBrowseBack(object target, ExecutedRoutedEventArgs e)
        {
            if(historyBack.Count > 0)
            {
                var oldValue = Source;
                var newValue = historyBack.Peek();

                historyForward.Push(oldValue);

                if(CanNavigate(oldValue, newValue, NavigationType.Back))
                {
                    isNavigatingHistory = true;
                    SetCurrentValue(SourceProperty, historyBack.Pop());
                    isNavigatingHistory = false;
                }
            }
        }

        private void OnGoToPage(object target, ExecutedRoutedEventArgs e)
        {
            var newValue = e.Parameter as Uri;

            if(newValue == null)
            {
                var newValueStr = e.Parameter as string;
                if(newValueStr != null)
                {
                    newValue = new Uri(newValueStr, UriKind.RelativeOrAbsolute);
                }
                else
                {
                    return;
                }
            }
            SetCurrentValue(SourceProperty, newValue);
        }

        private void OnRefresh(object target, ExecutedRoutedEventArgs e)
        {
            if(CanNavigate(Source, Source, NavigationType.Refresh))
            {
                Navigate(Source, Source, NavigationType.Refresh);
            }
        }

        private void OnCopy(object target, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(Content.ToString());
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var parent = NavigationHelper.FindFrame(null, this);
            if(parent != null)
            {
                parent.RegisterChildFrame(this);
            }
        }

        private void RegisterChildFrame(ModernFrame frame)
        {
            if(!GetChildFrames().Contains(frame))
            {
                var r = new WeakReference(frame);

                childFrames.Add(r);
            }
        }

        private bool ShouldKeepContentAlive(object content)
        {
            var o = content as DependencyObject;
            if(o != null)
            {
                var result = GetKeepAlive(o);

                if(result.HasValue)
                {
                    return result.Value;
                }
            }

            return KeepContentAlive;
        }

        public static bool? GetKeepAlive(DependencyObject o)
        {
            if(o == null)
            {
                throw new ArgumentNullException("o");
            }
            return (bool?) o.GetValue(KeepAliveProperty);
        }

        public static void SetKeepAlive(DependencyObject o, bool? value)
        {
            if(o == null)
            {
                throw new ArgumentNullException("o");
            }
            o.SetValue(KeepAliveProperty, value);
        }

        private void OnCanBrowseForward(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = historyForward.Count > 0;
        }

        private void OnCanBrowseHome(object sender, CanExecuteRoutedEventArgs e)
        {
            var MainWindow = Application.Current.MainWindow as ModernWindow;
            e.CanExecute = historyBack.Count > 0 && MainWindow.HomeSource != Source && MainWindow.HomeSource != null;
        }

        private void OnBrowseForward(object sender, ExecutedRoutedEventArgs e)
        {
            if(historyForward.Count > 0)
            {
                var oldValue = Source;
                var newValue = historyForward.Peek();

                historyBack.Push(oldValue);

                if(CanNavigate(oldValue, newValue, NavigationType.Forward))
                {
                    isNavigatingHistory = true;
                    SetCurrentValue(SourceProperty, historyForward.Pop());
                    isNavigatingHistory = false;
                }
            }
        }

        private void OnBrowseHome(object sender, ExecutedRoutedEventArgs e)
        {
            var MainWindow = Application.Current.MainWindow as ModernWindow;
            SetCurrentValue(SourceProperty, MainWindow.HomeSource);
        }

        public void ShowModal() {}
    }
}