// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

namespace MANAGER.Table
{
    public class Category
    {
        static Category()
        {
            TableName = "CATEGORIE";
            ID = $"ID_{TableName}";
            Title = "LIBELLE";
        }

        public static string ID { get; private set; }
        public static string TableName { get; private set; }
        public static string Title { get; private set; }

        public void Construction(string id, string tableName, string title)
        {
            ID = id;
            TableName = tableName;
            Title = title;
        }
    }
}