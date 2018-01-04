// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;

namespace FirstFloor.ModernUI.Presentation
{
    public class RelayCommand : CommandBase
    {
        private readonly Func<object, bool> canExecute;
        private readonly Action<object> execute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if(execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            if(canExecute == null)
            {
                canExecute = o => true;
            }
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        protected override void OnExecute(object parameter)
        {
            execute(parameter);
        }
    }
}