using Dapper;

namespace Flexi2.Data
{
    public class DbInit
    {
        private readonly FlexiDb _db;
        public DbInit(FlexiDb db) => _db = db;

        public void Init()
        {
            using var cn = _db.Open();

            cn.Execute(@"
CREATE TABLE IF NOT EXISTS Categories(
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Products(
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Price REAL NOT NULL,
    CategoryId INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Orders(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TableNumber INTEGER NOT NULL,
    OpenedAt TEXT NOT NULL,
    ClosedAt TEXT
);

CREATE TABLE IF NOT EXISTS OrderItems(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId INTEGER NOT NULL,
    ProductName TEXT NOT NULL,
    Qty INTEGER NOT NULL,
    Price REAL NOT NULL
);
");

            // seed ако е празно
            var count = cn.ExecuteScalar<int>("SELECT COUNT(*) FROM Categories;");
            if (count == 0)
            {
                cn.Execute(@"
INSERT INTO Categories VALUES (1,'Топли'),(2,'Студени'),(3,'Кухня');

INSERT INTO Products VALUES
(1,'Еспресо',2.50,1),
(2,'Капучино',3.00,1),
(3,'Вода',1.50,2),
(4,'Кола',2.80,2),
(5,'Бургер',8.50,3);
");
            }
        }
    }
}
