// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region

using System;
using System.Collections.Generic;

#endregion

namespace MANAGER.Classes
{
    public class Estimate
    {
        public Estimate(List<Merchandise> list)
        {
            GetList = list;
            TotalPrice = 0;
            Date = DateTime.Now;
        }

        public double TotalPrice { get; set; }
        public Customer Customer { get; set; }
        public DateTime Date { get; set; }
        public List<Merchandise> GetList { get; }

        public Merchandise this[int i] => GetList[i];
    }
}