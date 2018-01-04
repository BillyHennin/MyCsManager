// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using FirstFloor.ModernUI.Windows;

namespace FirstFloor.ModernUI
{
    public class ViewModelService
    {
        private static readonly ViewModelService current = new ViewModelService();
        private readonly Dictionary<object, Type> vModels = new Dictionary<object, Type>();

        public static ViewModelService Current { get { return current; } }

        public void RemoveViewModel(object model, Type type)
        {
            if(vModels.ContainsKey(model))
            {
                vModels.Remove(model);
            }
        }

        public T GetViewModel<T>(Type type)
        {
            var vm = vModels.FirstOrDefault(m => m.Value == type).Key;
            return (T) vm;
        }

        public List<object> GetAllViewModel(Type type)
        {
            var list = new List<object>();
            foreach(var val in vModels)
            {
                if(val.Value == type)
                {
                    list.Add(val.Key);
                }
            }
            return list;
        }

        public void AddViewModel(object vm, Type type)
        {
            if(!vModels.ContainsKey(vm))
            {
                vModels.Add(vm, type);
            }
        }

        public void AddViewModel(object tempvm, Type type, string name)
        {
            var canAdd = true;
            var vms = vModels.Where(m => m.Value == type).Select(m => m.Key);

            foreach(var vm in vms.OfType<IViewModel>().Select(vmTemp => vmTemp as IViewModel).Where(vm => vm.VmName == name)) {
                canAdd = false;
            }

            if(canAdd)
            {
                vModels.Add(tempvm, type);
            }
        }

        public T GetViewModel<T>()
        {
            var vm = vModels.FirstOrDefault(m => m.Value == typeof(T)).Key;
            return (T) vm;
        }

        public T GetViewModel<T>(string vmName)
        {
            var vms = vModels.Where(m => m.Value == typeof(T)).Select(m => m.Key);

            foreach(var vmTemp in vms)
            {
                if(vmTemp is IViewModel)
                {
                    var vm = vmTemp as IViewModel;
                    if(vm.VmName == vmName)
                    {
                        return (T) vmTemp;
                    }
                }
            }

            return default(T);
        }
    }
}