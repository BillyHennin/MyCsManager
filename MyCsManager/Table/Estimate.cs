// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

namespace MANAGER.Table
{
    public class Estimate
    {
        static Estimate()
        {
            TableName = "DEVIS";
            Day = "JOUR";
            NumberDevis = "NUMERODEVIS";
            PriceMerchandise = "PRIXMARCHANDISE";
            Quantity = "QUANTITE";
        }

        public static string TableName { get; private set; }
        public static string Quantity { get; private set; }
        public static string Day { get; private set; }
        public static string PriceMerchandise { get; private set; }
        public static string NumberDevis { get; private set; }

        public void Construction(string tableName, string day, string numberDevis, string priceMerchandise, string quantity)
        {
            TableName = tableName;
            Day = day;
            NumberDevis = numberDevis;
            PriceMerchandise = priceMerchandise;
            Quantity = quantity;
        }
    }
}