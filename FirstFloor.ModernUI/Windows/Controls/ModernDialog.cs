// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using FirstFloor.ModernUI.Presentation;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class ModernDialog : Window
    {
        public static readonly DependencyProperty BackgroundContentProperty = DependencyProperty.Register("BackgroundContent", typeof(object),
            typeof(ModernDialog));

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register("Buttons", typeof(IEnumerable<Button>), typeof(ModernDialog));
        private readonly ICommand closeCommand;

        private Button cancelButton;
        private Button closeButton;

        private MessageBoxResult dialogResult = MessageBoxResult.None;
        private Button noButton;
        private Button okButton;
        private Button yesButton;

        public ModernDialog()
        {
            DefaultStyleKey = typeof(ModernDialog);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            closeCommand = new RelayCommand(o =>
            {
                var result = o as MessageBoxResult?;
                if(result.HasValue)
                {
                    dialogResult = result.Value;
                }
                Close();
            });

            Buttons = new[] {CloseButton};

            if(Application.Current != null && Application.Current.MainWindow != this)
            {
                Owner = Application.Current.MainWindow;
            }
        }

        public ICommand CloseCommand { get { return closeCommand; } }

        public Button OkButton { get { return okButton ?? (okButton = CreateCloseDialogButton(ModernUI.Resources.Ok, true, false, MessageBoxResult.OK)); } }

        public Button CancelButton
        {
            get { return cancelButton ?? (cancelButton = CreateCloseDialogButton(ModernUI.Resources.Cancel, false, true, MessageBoxResult.Cancel)); }
        }

        public Button YesButton
        {
            get { return yesButton ?? (yesButton = CreateCloseDialogButton(ModernUI.Resources.Yes, true, false, MessageBoxResult.Yes)); }
        }

        public Button NoButton { get { return noButton ?? (noButton = CreateCloseDialogButton(ModernUI.Resources.No, false, true, MessageBoxResult.No)); } }

        public Button CloseButton
        {
            get { return closeButton ?? (closeButton = CreateCloseDialogButton(ModernUI.Resources.Close, true, false, MessageBoxResult.None)); }
        }

        public object BackgroundContent { get { return GetValue(BackgroundContentProperty); } set { SetValue(BackgroundContentProperty, value); } }

        public IEnumerable<Button> Buttons { get { return (IEnumerable<Button>) GetValue(ButtonsProperty); } set { SetValue(ButtonsProperty, value); } }

        private Button CreateCloseDialogButton(string content, bool isDefault, bool isCancel, MessageBoxResult result)
        {
            return new Button
            {
                Content = content,
                Command = CloseCommand,
                CommandParameter = result,
                IsDefault = isDefault,
                IsCancel = isCancel,
                MinHeight = 21,
                MinWidth = 135,
                Margin = new Thickness(4, 0, 0, 0)
            };
        }

        public static MessageBoxResult ShowMessage(string text, string title, MessageBoxButton button)
        {
            var dlg = new ModernDialog
            {
                Title = title,
                Content = new BbCodeBlock {BbCode = text, Margin = new Thickness(0, 0, 0, 8)},
                MinHeight = 0,
                MinWidth = 0,
                MaxHeight = 480,
                MaxWidth = 640,
            };

            dlg.Buttons = GetButtons(dlg, button);
            dlg.ShowDialog();
            return dlg.dialogResult;
        }

        private static IEnumerable<Button> GetButtons(ModernDialog owner, MessageBoxButton button)
        {
            if(button == MessageBoxButton.OK)
            {
                yield return owner.OkButton;
            }
            else if(button == MessageBoxButton.OKCancel)
            {
                yield return owner.OkButton;
                yield return owner.CancelButton;
            }
            else if(button == MessageBoxButton.YesNo)
            {
                yield return owner.YesButton;
                yield return owner.NoButton;
            }
            else if(button == MessageBoxButton.YesNoCancel)
            {
                yield return owner.YesButton;
                yield return owner.NoButton;
                yield return owner.CancelButton;
            }
        }
    }
}