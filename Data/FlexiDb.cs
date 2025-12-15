using Microsoft.Data.Sqlite;
using System.IO;

namespace Flexi2.Data
{
    public class FlexiDb
    {
        public string DbPath { get; }

        public FlexiDb()
        {
            var dir = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "Flexi2");
            Directory.CreateDirectory(dir);

            DbPath = Path.Combine(dir, "flexi2.db");
        }

        public SqliteConnection Open()
        {
            var cn = new SqliteConnection($"Data Source={DbPath}");
            cn.Open();
            return cn;
        }
    }
}
