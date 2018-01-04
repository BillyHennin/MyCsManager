﻿// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using FirstFloor.ModernUI.Windows.Media;

namespace FirstFloor.ModernUI.Windows.Controls
{
    [TemplatePart(Name = PreviousContentPresentationSitePartName, Type = typeof(ContentControl))]
    [TemplatePart(Name = CurrentContentPresentationSitePartName, Type = typeof(ContentControl))]
    public class TransitioningContentControl : ContentControl
    {
        private const string PresentationGroup = "PresentationStates";

        private const string NormalState = "Normal";

        public const string DefaultTransitionState = "DefaultTransition";

        internal const string PreviousContentPresentationSitePartName = "PreviousContentPresentationSite";

        internal const string CurrentContentPresentationSitePartName = "CurrentContentPresentationSite";
        public static readonly DependencyProperty IsTransitioningProperty = DependencyProperty.Register("IsTransitioning", typeof(bool),
            typeof(TransitioningContentControl), new PropertyMetadata(OnIsTransitioningPropertyChanged));
        public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register("Transition", typeof(string),
            typeof(TransitioningContentControl), new PropertyMetadata(DefaultTransitionState, OnTransitionPropertyChanged));
        public static readonly DependencyProperty RestartTransitionOnContentChangeProperty = DependencyProperty.Register("RestartTransitionOnContentChange",
            typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(false, OnRestartTransitionOnContentChangePropertyChanged));
        private bool _allowIsTransitioningWrite;
        private Storyboard _currentTransition;

        static TransitioningContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitioningContentControl), new FrameworkPropertyMetadata(typeof(TransitioningContentControl)));
        }

        private ContentPresenter CurrentContentPresentationSite { get; set; }

        private ContentPresenter PreviousContentPresentationSite { get; set; }

        public bool IsTransitioning
        {
            get { return (bool) GetValue(IsTransitioningProperty); }
            private set
            {
                _allowIsTransitioningWrite = true;
                SetValue(IsTransitioningProperty, value);
                _allowIsTransitioningWrite = false;

                if(IsTransitioningChanged != null)
                {
                    IsTransitioningChanged(this, EventArgs.Empty);
                }
            }
        }

        private Storyboard CurrentTransition
        {
            set
            {
                if(_currentTransition != null)
                {
                    _currentTransition.Completed -= OnTransitionCompleted;
                }

                _currentTransition = value;

                if(_currentTransition != null)
                {
                    _currentTransition.Completed += OnTransitionCompleted;
                }
            }
        }

        public string Transition { get { return GetValue(TransitionProperty) as string; } set { SetValue(TransitionProperty, value); } }

        public bool RestartTransitionOnContentChange
        {
            get { return (bool) GetValue(RestartTransitionOnContentChangeProperty); }
            set { SetValue(RestartTransitionOnContentChangeProperty, value); }
        }

        public event EventHandler IsTransitioningChanged;

        private static void OnIsTransitioningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl) d;

            if(!source._allowIsTransitioningWrite)
            {
                source.IsTransitioning = (bool) e.OldValue;
                throw new InvalidOperationException("IsTransitioning property is read-only.");
            }
        }

        private static void OnTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl) d;
            var oldTransition = e.NewValue as string;
            var newTransition = e.NewValue as string;

            if(source.IsTransitioning)
            {
                source.AbortTransition();
            }

            Storyboard newStoryboard = source.GetStoryboard(newTransition);

            if(newStoryboard == null)
            {
                if(source.TryGetVisualStateGroup(PresentationGroup) == null)
                {
                    source.CurrentTransition = null;
                }
                else
                {
                    source.SetValue(TransitionProperty, oldTransition);

                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Transition '{0}' was not defined.", newTransition));
                }
            }
            else
            {
                source.CurrentTransition = newStoryboard;
            }
        }

        private static void OnRestartTransitionOnContentChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitioningContentControl) d).OnRestartTransitionOnContentChangeChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnRestartTransitionOnContentChangeChanged(bool oldValue, bool newValue) {}

        public event RoutedEventHandler TransitionCompleted;

        public override void OnApplyTemplate()
        {
            if(IsTransitioning)
            {
                AbortTransition();
            }

            base.OnApplyTemplate();

            PreviousContentPresentationSite = GetTemplateChild(PreviousContentPresentationSitePartName) as ContentPresenter;
            CurrentContentPresentationSite = GetTemplateChild(CurrentContentPresentationSitePartName) as ContentPresenter;

            if(CurrentContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Content = Content;
            }

            Storyboard transition = GetStoryboard(Transition);
            CurrentTransition = transition;
            if(transition == null)
            {
                string invalidTransition = Transition;
                Transition = DefaultTransitionState;

                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Transition '{0}' was not defined.", invalidTransition));
            }

            VisualStateManager.GoToState(this, NormalState, false);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            StartTransition(oldContent, newContent);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newContent", Justification = "Should be used in the future.")]
        private void StartTransition(object oldContent, object newContent)
        {
            if(CurrentContentPresentationSite != null && PreviousContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Content = newContent;

                PreviousContentPresentationSite.Content = oldContent;

                if(!IsTransitioning || RestartTransitionOnContentChange)
                {
                    IsTransitioning = true;
                    VisualStateManager.GoToState(this, NormalState, false);
                    VisualStateManager.GoToState(this, Transition, true);
                }
            }
        }

        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            AbortTransition();

            RoutedEventHandler handler = TransitionCompleted;
            if(handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }

        public void AbortTransition()
        {
            VisualStateManager.GoToState(this, NormalState, false);
            IsTransitioning = false;
            if(PreviousContentPresentationSite != null)
            {
                PreviousContentPresentationSite.Content = null;
            }
        }

        private Storyboard GetStoryboard(string newTransition)
        {
            VisualStateGroup presentationGroup = this.TryGetVisualStateGroup(PresentationGroup);
            Storyboard newStoryboard = null;
            if(presentationGroup != null)
            {
                newStoryboard =
                    presentationGroup.States.OfType<VisualState>().Where(state => state.Name == newTransition).Select(state => state.Storyboard).FirstOrDefault();
            }
            return newStoryboard;
        }
    }
}