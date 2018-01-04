// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Windows.Input;

namespace FirstFloor.ModernUI.Presentation
{
    public abstract class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(!CanExecute(parameter))
            {
                return;
            }
            OnExecute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        protected abstract void OnExecute(object parameter);
    }
}