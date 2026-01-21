using System;
using Flexi2.Core.Security;

namespace Flexi2.Data
{
    public static class DbInit
    {
        public static void EnsureCreated(FlexiDb db)
        {
            // --- Core tables ---
            db.Execute(@"
CREATE TABLE IF NOT EXISTS Users(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    PinHash TEXT NOT NULL,
    PinSalt TEXT NOT NULL DEFAULT '',
    Role INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);");

            db.Execute(@"
CREATE TABLE IF NOT EXISTS Zones(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);");

            db.Execute(@"
CREATE TABLE IF NOT EXISTS Tables(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ZoneId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    PosX REAL NOT NULL DEFAULT 20,
    PosY REAL NOT NULL DEFAULT 20,
    Width REAL NOT NULL DEFAULT 170,
    Height REAL NOT NULL DEFAULT 120,
    Status INTEGER NOT NULL DEFAULT 0,
    OwnerUserId INTEGER NULL,
    OpenedAtUtc TEXT NULL,
    CurrentTotal REAL NOT NULL DEFAULT 0,
    FOREIGN KEY(ZoneId) REFERENCES Zones(Id)
);");

            // Menu / products
            db.Execute(@"
CREATE TABLE IF NOT EXISTS Categories(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);");

            db.Execute(@"
CREATE TABLE IF NOT EXISTS Products(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Price REAL NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    HasModifiers INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(CategoryId) REFERENCES Categories(Id)
);");

            // Orders
            db.Execute(@"
CREATE TABLE IF NOT EXISTS Orders(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TableId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    ClosedAtUtc TEXT NULL,
    IsClosed INTEGER NOT NULL DEFAULT 0,
    DiscountPercent REAL NOT NULL DEFAULT 0,
    DiscountAmount REAL NOT NULL DEFAULT 0,
    TipAmount REAL NOT NULL DEFAULT 0,
    Subtotal REAL NOT NULL DEFAULT 0,
    FinalTotal REAL NOT NULL DEFAULT 0,
    PaymentMethod TEXT NULL,
    PaidTotal REAL NOT NULL DEFAULT 0,
    FOREIGN KEY(TableId) REFERENCES Tables(Id),
    FOREIGN KEY(UserId) REFERENCES Users(Id)
);");

            db.Execute(@"
CREATE TABLE IF NOT EXISTS OrderItems(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    ProductName TEXT NOT NULL,
    UnitPrice REAL NOT NULL,
    Qty INTEGER NOT NULL,
    IsLocked INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY(OrderId) REFERENCES Orders(Id)
);");

            // Audit
            db.Execute(@"
CREATE TABLE IF NOT EXISTS AuditLog(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AtUtc TEXT NOT NULL,
    UserId INTEGER NOT NULL,
    Action TEXT NOT NULL,
    Entity TEXT NOT NULL,
    EntityId INTEGER NOT NULL,
    Details TEXT NOT NULL
);");

            // --- Simple migrations for older DBs ---
            // Users.PinSalt might be missing from old builds.
            try
            {
                var hasSalt = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Users') WHERE name='PinSalt';");
                if (hasSalt == 0)
                {
                    db.Execute("ALTER TABLE Users ADD COLUMN PinSalt TEXT NOT NULL DEFAULT '';" );
                }
            }
            catch
            {
                // Ignore migration errors; fresh DB will still work.
            }

            // Tables layout columns might be missing from old builds.
            try
            {
                var hasPosX = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Tables') WHERE name='PosX';");
                if (hasPosX == 0) db.Execute("ALTER TABLE Tables ADD COLUMN PosX REAL NOT NULL DEFAULT 20;");

                var hasPosY = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Tables') WHERE name='PosY';");
                if (hasPosY == 0) db.Execute("ALTER TABLE Tables ADD COLUMN PosY REAL NOT NULL DEFAULT 20;");

                var hasW = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Tables') WHERE name='Width';");
                if (hasW == 0) db.Execute("ALTER TABLE Tables ADD COLUMN Width REAL NOT NULL DEFAULT 170;");

                var hasH = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Tables') WHERE name='Height';");
                if (hasH == 0) db.Execute("ALTER TABLE Tables ADD COLUMN Height REAL NOT NULL DEFAULT 120;");
            }
            catch
            {
                // ignore
            }

            // Orders payment / totals columns might be missing from older DBs.
            try
            {
                var hasDiscountAmount = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Orders') WHERE name='DiscountAmount';");
                if (hasDiscountAmount == 0) db.Execute("ALTER TABLE Orders ADD COLUMN DiscountAmount REAL NOT NULL DEFAULT 0;");

                var hasTip = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Orders') WHERE name='TipAmount';");
                if (hasTip == 0) db.Execute("ALTER TABLE Orders ADD COLUMN TipAmount REAL NOT NULL DEFAULT 0;");

                var hasSubtotal = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Orders') WHERE name='Subtotal';");
                if (hasSubtotal == 0) db.Execute("ALTER TABLE Orders ADD COLUMN Subtotal REAL NOT NULL DEFAULT 0;");

                var hasFinal = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Orders') WHERE name='FinalTotal';");
                if (hasFinal == 0) db.Execute("ALTER TABLE Orders ADD COLUMN FinalTotal REAL NOT NULL DEFAULT 0;");
            }
            catch
            {
                // ignore
            }

            // Menu columns might be missing from older DBs.
            // SQLite supports ADD COLUMN, so we can do lightweight migrations.
            try
            {
                var catHasActive = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Categories') WHERE name='IsActive';");
                if (catHasActive == 0) db.Execute("ALTER TABLE Categories ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;");

                var prodHasActive = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Products') WHERE name='IsActive';");
                if (prodHasActive == 0) db.Execute("ALTER TABLE Products ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;");

                var prodHasMods = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Products') WHERE name='HasModifiers';");
                if (prodHasMods == 0) db.Execute("ALTER TABLE Products ADD COLUMN HasModifiers INTEGER NOT NULL DEFAULT 0;");

                var prodHasPrice = db.Scalar<long>("SELECT COUNT(1) FROM pragma_table_info('Products') WHERE name='Price';");
                // Extremely old builds might not have Price as a column name.
                // If missing, the safest route is to keep DB usable by re-creating (user can delete flexi.db).
                // We don't attempt destructive migrations here.
                _ = prodHasPrice;
            }
            catch
            {
                // ignore
            }

            // If we have legacy users with empty salt, migrate them by treating PinHash as plain PIN.
            try
            {
                var legacy = db.Scalar<long>("SELECT COUNT(1) FROM Users WHERE IFNULL(PinSalt,'') = '';");
                if (legacy > 0)
                {
                    using var con = db.Open();
                    using var cmd = con.CreateCommand();
                    cmd.CommandText = "SELECT Id, PinHash FROM Users WHERE IFNULL(PinSalt,'') = '';";
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        var id = r.GetInt32(0);
                        var plain = r.GetString(1);
                        var salt = PinHasher.NewSaltHex();
                        var hash = PinHasher.Hash(plain, salt);
                        db.Execute("UPDATE Users SET PinHash=$h, PinSalt=$s WHERE Id=$id;", c =>
                        {
                            c.Parameters.AddWithValue("$h", hash);
                            c.Parameters.AddWithValue("$s", salt);
                            c.Parameters.AddWithValue("$id", id);
                        });
                    }
                }
            }
            catch
            {
                // ignore
            }

            // --- Seeds ---
            var uCount = db.Scalar<long>("SELECT COUNT(1) FROM Users;");
            if (uCount == 0)
            {
                // POS 1111
                var s1 = PinHasher.NewSaltHex();
                var h1 = PinHasher.Hash("1111", s1);
                db.Execute("INSERT INTO Users(Name,PinHash,PinSalt,Role,IsActive) VALUES('POS USER',$h,$s,0,1);",
                    c => { c.Parameters.AddWithValue("$h", h1); c.Parameters.AddWithValue("$s", s1); });

                // ADMIN 9999
                var s2 = PinHasher.NewSaltHex();
                var h2 = PinHasher.Hash("9999", s2);
                db.Execute("INSERT INTO Users(Name,PinHash,PinSalt,Role,IsActive) VALUES('ADMIN',$h,$s,1,1);",
                    c => { c.Parameters.AddWithValue("$h", h2); c.Parameters.AddWithValue("$s", s2); });
            }

            var zCount = db.Scalar<long>("SELECT COUNT(1) FROM Zones;");
            if (zCount == 0)
            {
                db.Execute("INSERT INTO Zones(Name) VALUES ('Salon');");
                db.Execute("INSERT INTO Zones(Name) VALUES ('Garden');");

                var salonId = db.Scalar<long>("SELECT Id FROM Zones WHERE Name='Salon' LIMIT 1;");
                var gardenId = db.Scalar<long>("SELECT Id FROM Zones WHERE Name='Garden' LIMIT 1;");

                db.Execute("INSERT INTO Tables(ZoneId,Name,Status) VALUES ($z,'T1',0);", c => c.Parameters.AddWithValue("$z", (int)salonId));
                db.Execute("INSERT INTO Tables(ZoneId,Name,Status) VALUES ($z,'T2',0);", c => c.Parameters.AddWithValue("$z", (int)salonId));
                db.Execute("INSERT INTO Tables(ZoneId,Name,Status) VALUES ($z,'G1',0);", c => c.Parameters.AddWithValue("$z", (int)gardenId));
            }

            var cCount = db.Scalar<long>("SELECT COUNT(1) FROM Categories;");
            if (cCount == 0)
            {
                db.Execute("INSERT INTO Categories(Name,IsActive) VALUES ('Напитки',1);");
                db.Execute("INSERT INTO Categories(Name,IsActive) VALUES ('Храна',1);");
                db.Execute("INSERT INTO Categories(Name,IsActive) VALUES ('Наргиле',1);");

                var drinks = db.Scalar<long>("SELECT Id FROM Categories WHERE Name='Напитки' LIMIT 1;");
                var food = db.Scalar<long>("SELECT Id FROM Categories WHERE Name='Храна' LIMIT 1;");
                var hookah = db.Scalar<long>("SELECT Id FROM Categories WHERE Name='Наргиле' LIMIT 1;");

                // Example items (edit/delete from Admin later)
                db.Execute("INSERT INTO Products(CategoryId,Name,Price,IsActive,HasModifiers) VALUES ($c,'Кола',3.50,1,0);", c => c.Parameters.AddWithValue("$c", (int)drinks));
                db.Execute("INSERT INTO Products(CategoryId,Name,Price,IsActive,HasModifiers) VALUES ($c,'Вода',2.00,1,0);", c => c.Parameters.AddWithValue("$c", (int)drinks));
                db.Execute("INSERT INTO Products(CategoryId,Name,Price,IsActive,HasModifiers) VALUES ($c,'Кафе',2.50,1,0);", c => c.Parameters.AddWithValue("$c", (int)drinks));
                db.Execute("INSERT INTO Products(CategoryId,Name,Price,IsActive,HasModifiers) VALUES ($c,'Пържени картофи',5.50,1,0);", c => c.Parameters.AddWithValue("$c", (int)food));
                db.Execute("INSERT INTO Products(CategoryId,Name,Price,IsActive,HasModifiers) VALUES ($c,'Дюнер',7.50,1,0);", c => c.Parameters.AddWithValue("$c", (int)food));
                db.Execute("INSERT INTO Products(CategoryId,Name,Price,IsActive,HasModifiers) VALUES ($c,'Наргиле (класик)',20.00,1,0);", c => c.Parameters.AddWithValue("$c", (int)hookah));
            }
        }
    }
}
