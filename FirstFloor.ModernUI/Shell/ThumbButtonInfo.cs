// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    [DefaultEvent("Click")]
    public sealed class ThumbButtonInfo : Freezable, ICommandSource
    {
        private EventHandler _commandEvent;

        protected override Freezable CreateInstanceCore()
        {
            return new ThumbButtonInfo();
        }

        public event EventHandler Click;

        internal void InvokeClick()
        {
            var local = Click;
            if(local != null)
            {
                local(this, EventArgs.Empty);
            }
            _InvokeCommand();
        }

        private void _InvokeCommand()
        {
            var command = Command;
            if(command != null)
            {
                var parameter = CommandParameter;
                var target = CommandTarget;
                var routedCommand = command as RoutedCommand;
                if(routedCommand != null)
                {
                    if(routedCommand.CanExecute(parameter, target))
                    {
                        routedCommand.Execute(parameter, target);
                    }
                }
                else if(command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        }

        private void _UnhookCommand(ICommand command)
        {
            Assert.IsNotNull(command);
            command.CanExecuteChanged -= _commandEvent;
            _commandEvent = null;
            _UpdateCanExecute();
        }

        private void _HookCommand(ICommand command)
        {
            _commandEvent = (sender, e) => _UpdateCanExecute();
            command.CanExecuteChanged += _commandEvent;
            _UpdateCanExecute();
        }

        private void _UpdateCanExecute()
        {
            if(Command != null)
            {
                var parameter = CommandParameter;
                var target = CommandTarget;
                var routed = Command as RoutedCommand;
                _CanExecute = routed != null ? routed.CanExecute(parameter, target) : Command.CanExecute(parameter);
            }
            else
            {
                _CanExecute = true;
            }
        }

        #region ICommandSource Members

        public ICommand Command { get { return (ICommand) GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }
        public object CommandParameter { get { return GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }
        public IInputElement CommandTarget { get { return (IInputElement) GetValue(CommandTargetProperty); } set { SetValue(CommandTargetProperty, value); } }

        #endregion

        #region Dependency Properties and support methods

        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(ThumbButtonInfo),
            new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty DismissWhenClickedProperty = DependencyProperty.Register("DismissWhenClicked", typeof(bool),
            typeof(ThumbButtonInfo), new PropertyMetadata(false));
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ThumbButtonInfo),
            new PropertyMetadata(null));
        public static readonly DependencyProperty IsBackgroundVisibleProperty = DependencyProperty.Register("IsBackgroundVisible", typeof(bool),
            typeof(ThumbButtonInfo), new PropertyMetadata(true));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ThumbButtonInfo),
            new PropertyMetadata(string.Empty, null, _CoerceDescription));
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ThumbButtonInfo),
            new PropertyMetadata(true, null, (d, e) => ((ThumbButtonInfo) d)._CoerceIsEnabledValue(e)));
        public static readonly DependencyProperty IsInteractiveProperty = DependencyProperty.Register("IsInteractive", typeof(bool), typeof(ThumbButtonInfo),
            new PropertyMetadata(true));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ThumbButtonInfo),
            new PropertyMetadata(null, (d, e) => ((ThumbButtonInfo) d)._OnCommandChanged(e)));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object),
            typeof(ThumbButtonInfo), new PropertyMetadata(null, (d, e) => ((ThumbButtonInfo) d)._UpdateCanExecute()));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement),
            typeof(ThumbButtonInfo), new PropertyMetadata(null, (d, e) => ((ThumbButtonInfo) d)._UpdateCanExecute()));

        private static readonly DependencyProperty _CanExecuteProperty = DependencyProperty.Register("_CanExecute", typeof(bool), typeof(ThumbButtonInfo),
            new PropertyMetadata(true, (d, e) => d.CoerceValue(IsEnabledProperty)));

        public Visibility Visibility { get { return (Visibility) GetValue(VisibilityProperty); } set { SetValue(VisibilityProperty, value); } }

        public bool DismissWhenClicked { get { return (bool) GetValue(DismissWhenClickedProperty); } set { SetValue(DismissWhenClickedProperty, value); } }

        public ImageSource ImageSource { get { return (ImageSource) GetValue(ImageSourceProperty); } set { SetValue(ImageSourceProperty, value); } }

        public bool IsBackgroundVisible { get { return (bool) GetValue(IsBackgroundVisibleProperty); } set { SetValue(IsBackgroundVisibleProperty, value); } }

        public string Description { get { return (string) GetValue(DescriptionProperty); } set { SetValue(DescriptionProperty, value); } }

        public bool IsEnabled { get { return (bool) GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value); } }

        public bool IsInteractive { get { return (bool) GetValue(IsInteractiveProperty); } set { SetValue(IsInteractiveProperty, value); } }
        private bool _CanExecute { get { return (bool) GetValue(_CanExecuteProperty); } set { SetValue(_CanExecuteProperty, value); } }

        private static object _CoerceDescription(DependencyObject d, object value)
        {
            var text = (string) value;
            if(text != null && text.Length >= 260)
            {
                text = text.Substring(0, 259);
            }
            return text;
        }

        private object _CoerceIsEnabledValue(object value)
        {
            var enabled = (bool) value;
            return enabled && _CanExecute;
        }

        private void _OnCommandChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldCommand = (ICommand) e.OldValue;
            var newCommand = (ICommand) e.NewValue;
            if(oldCommand == newCommand)
            {
                return;
            }
            if(oldCommand != null)
            {
                _UnhookCommand(oldCommand);
            }
            if(newCommand != null)
            {
                _HookCommand(newCommand);
            }
        }

        #endregion
    }
}