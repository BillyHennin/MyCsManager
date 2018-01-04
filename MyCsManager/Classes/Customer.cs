// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

using System.Collections.Generic;
using System.Windows.Controls;

namespace MANAGER.Classes
{
    public class Customer
    {
        public Customer(int id, string name, string phone, string email)
        {
            Id = id;
            Name = name;
            Phone = phone;
            Email = email;
        }

        public Border Border { get; set; }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public List<Estimate> listEstimate { get; set; }
    }
}