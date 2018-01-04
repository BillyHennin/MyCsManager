// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

using System;

namespace MANAGER.Table
{
    public class Merchandise
    {
        static Merchandise()
        {
            TableName = "MARCHANDISE";
            ID = $"ID_{TableName}";
            Name = "NOM";
            OnSale = "ENVENTE";
            Price = "PRIX";
            Quantity = "QUANTITE";
        }

        public static string ID { get; private set; }
        public static string TableName { get; private set; }
        public static string Price { get; private set; }
        public static string Name { get; private set; }
        public static string Quantity { get; private set; }
        public static string OnSale { get; private set; }

        public void Construction(string tableName, string name, string onSale, string price, string quantity)
        {
            TableName = tableName;
            ID = $"ID_{Merchandise.TableName}";
            Name = name;
            OnSale = onSale;
            Price = price;
            Quantity = quantity;
        }
    }
}