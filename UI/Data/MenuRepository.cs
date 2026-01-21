using Flexi2.Models;

namespace Flexi2.Data
{
    public sealed class MenuRepository
    {
        private readonly FlexiDb _db;
        public MenuRepository(FlexiDb db) => _db = db;

        public System.Collections.Generic.List<Category> GetCategories(bool includeInactive = false)
        {
            var sql = includeInactive
                ? @"SELECT Id, Name, IsActive FROM Categories ORDER BY Name;"
                : @"SELECT Id, Name, IsActive FROM Categories WHERE IsActive = 1 ORDER BY Name;";

            return _db.Query(sql,
            r => new Category
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                IsActive = r.GetInt32(2) == 1
            });
        }

        public System.Collections.Generic.List<Product> GetProducts(int categoryId, bool includeInactive = false)
        {
            var sql = includeInactive
                ? @"SELECT Id, CategoryId, Name, Price, IsActive, HasModifiers FROM Products WHERE CategoryId = $c ORDER BY Name;"
                : @"SELECT Id, CategoryId, Name, Price, IsActive, HasModifiers FROM Products WHERE CategoryId = $c AND IsActive = 1 ORDER BY Name;";

            return _db.Query(sql,
            r => new Product
            {
                Id = r.GetInt32(0),
                CategoryId = r.GetInt32(1),
                Name = r.GetString(2),
                Price = (decimal)r.GetDouble(3),
                IsActive = r.GetInt32(4) == 1,
                HasModifiers = r.GetInt32(5) == 1
            },
            c => c.Parameters.AddWithValue("$c", categoryId));
        }

        public int AddCategory(string name)
        {
            _db.Execute("INSERT INTO Categories(Name,IsActive) VALUES ($n,1);", c => c.Parameters.AddWithValue("$n", name.Trim()));
            return (int)_db.Scalar<long>("SELECT last_insert_rowid();");
        }

        public void SetCategoryActive(int id, bool isActive)
        {
            _db.Execute("UPDATE Categories SET IsActive=$a WHERE Id=$id;", c =>
            {
                c.Parameters.AddWithValue("$a", isActive ? 1 : 0);
                c.Parameters.AddWithValue("$id", id);
            });

            // Keep POS consistent with category visibility:
            // - when category is disabled -> hide all its products
            // - when category is enabled  -> show all its products (admin can still toggle per product afterwards)
            _db.Execute("UPDATE Products SET IsActive=$a WHERE CategoryId=$c;", c =>
            {
                c.Parameters.AddWithValue("$a", isActive ? 1 : 0);
                c.Parameters.AddWithValue("$c", id);
            });
        }

        public int AddProduct(int categoryId, string name, decimal price, bool hasModifiers)
        {
            _db.Execute(@"INSERT INTO Products(CategoryId,Name,Price,IsActive,HasModifiers)
VALUES ($c,$n,$p,1,$m);", c =>
            {
                c.Parameters.AddWithValue("$c", categoryId);
                c.Parameters.AddWithValue("$n", name.Trim());
                c.Parameters.AddWithValue("$p", (double)price);
                c.Parameters.AddWithValue("$m", hasModifiers ? 1 : 0);
            });
            return (int)_db.Scalar<long>("SELECT last_insert_rowid();");
        }

        public void SetProductActive(int productId, bool isActive)
        {
            _db.Execute("UPDATE Products SET IsActive=$a WHERE Id=$id;", c =>
            {
                c.Parameters.AddWithValue("$a", isActive ? 1 : 0);
                c.Parameters.AddWithValue("$id", productId);
            });
        }

        /// <summary>
        /// Hard delete product. Order history remains intact because OrderItems stores a snapshot (name/price)
        /// and does not enforce a FK to Products in this build.
        /// </summary>
        public void DeleteProduct(int productId)
        {
            _db.Execute("DELETE FROM Products WHERE Id=$id;", c => c.Parameters.AddWithValue("$id", productId));
        }

        /// <summary>
        /// Hard delete category + all its products.
        /// </summary>
        public void DeleteCategory(int categoryId)
        {
            // delete products first (no ON DELETE CASCADE in schema)
            _db.Execute("DELETE FROM Products WHERE CategoryId=$c;", c => c.Parameters.AddWithValue("$c", categoryId));
            _db.Execute("DELETE FROM Categories WHERE Id=$id;", c => c.Parameters.AddWithValue("$id", categoryId));
        }
    }
}
