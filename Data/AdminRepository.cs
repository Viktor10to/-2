using System.Collections.Generic;
using Flexi2.Models;

namespace Flexi2.Data
{
    public sealed class AdminRepository
    {
        private readonly FlexiDb _db;
        public AdminRepository(FlexiDb db) => _db = db;

        public IEnumerable<Category> GetCategories()
        {
            var list = new List<Category>();
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Name FROM Categories ORDER BY Name ASC;";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Category { Id = r.GetInt32(0), Name = r.GetString(1) });
            return list;
        }

        public IEnumerable<Product> GetProductsByCategory(int categoryId)
        {
            var list = new List<Product>();
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, CategoryId, Name, Price FROM Products WHERE CategoryId=$cid ORDER BY Name ASC;";
            cmd.Parameters.AddWithValue("$cid", categoryId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Product
                {
                    Id = r.GetInt32(0),
                    CategoryId = r.GetInt32(1),
                    Name = r.GetString(2),
                    Price = (decimal)r.GetDouble(3)
                });
            }
            return list;
        }
    }
}
