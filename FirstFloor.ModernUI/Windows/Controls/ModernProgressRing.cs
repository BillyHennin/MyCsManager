// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace FirstFloor.ModernUI.Windows.Controls
{
    [TemplateVisualState(GroupName = GroupActiveStates, Name = StateInactive)]
    [TemplateVisualState(GroupName = GroupActiveStates, Name = StateActive)]
    public class ModernProgressRing : Control
    {
        private const string GroupActiveStates = "ActiveStates";
        private const string StateInactive = "Inactive";
        private const string StateActive = "Active";

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ModernProgressRing),
            new PropertyMetadata(false, OnIsActiveChanged));

        public ModernProgressRing()
        {
            DefaultStyleKey = typeof(ModernProgressRing);
        }

        public bool IsActive { get { return (bool) GetValue(IsActiveProperty); } set { SetValue(IsActiveProperty, value); } }

        private void GotoCurrentState(bool animate)
        {
            var state = IsActive ? StateActive : StateInactive;

            VisualStateManager.GoToState(this, state, animate);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            GotoCurrentState(false);
        }

        private static void OnIsActiveChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernProgressRing) o).GotoCurrentState(true);
        }
    }
}