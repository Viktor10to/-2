using System;
using Flexi2.Core.Security;
using Flexi2.Models;

namespace Flexi2.Data
{
    public static class DbInit
    {
        public static void EnsureCreated(FlexiDb db)
        {
            db.Execute(@"
CREATE TABLE IF NOT EXISTS Users(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Role INTEGER NOT NULL,
    PinHash TEXT NOT NULL,
    PinSalt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAtUtc TEXT NOT NULL
);");

            db.Execute(@"
CREATE TABLE IF NOT EXISTS Categories(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);");

            db.Execute(@"
CREATE TABLE IF NOT EXISTS Products(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Price REAL NOT NULL,
    FOREIGN KEY(CategoryId) REFERENCES Categories(Id)
);");

            // seed users
            var usersCount = db.Scalar<long>("SELECT COUNT(1) FROM Users;");
            if (usersCount == 0)
            {
                SeedUser(db, "ADMIN", UserRole.Admin, "9999");
                SeedUser(db, "POS USER", UserRole.Pos, "1111");
            }

            // seed menu
            var catCount = db.Scalar<long>("SELECT COUNT(1) FROM Categories;");
            if (catCount == 0)
            {
                db.Execute("INSERT INTO Categories(Name) VALUES ('Drinks');");
                db.Execute("INSERT INTO Categories(Name) VALUES ('Food');");
                db.Execute("INSERT INTO Categories(Name) VALUES ('Hookah');");

                var drinksId = db.Scalar<long>("SELECT Id FROM Categories WHERE Name='Drinks' LIMIT 1;");
                var foodId = db.Scalar<long>("SELECT Id FROM Categories WHERE Name='Food' LIMIT 1;");
                var hookahId = db.Scalar<long>("SELECT Id FROM Categories WHERE Name='Hookah' LIMIT 1;");

                InsertProduct(db, (int)drinksId, "Coke", 3.50m);
                InsertProduct(db, (int)drinksId, "Frappe", 6.00m);
                InsertProduct(db, (int)foodId, "Burger", 10.00m);
                InsertProduct(db, (int)foodId, "Fries", 5.50m);
                InsertProduct(db, (int)hookahId, "Hookah 28", 28.00m);
            }
        }

        private static void SeedUser(FlexiDb db, string name, UserRole role, string pin)
        {
            var salt = PinHasher.NewSalt();
            var hash = PinHasher.Hash(pin, salt);

            db.Execute(@"
INSERT INTO Users(Name, Role, PinHash, PinSalt, IsActive, CreatedAtUtc)
VALUES ($name, $role, $hash, $salt, 1, $created);",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$role", (int)role);
                cmd.Parameters.AddWithValue("$hash", hash);
                cmd.Parameters.AddWithValue("$salt", salt);
                cmd.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));
            });
        }

        private static void InsertProduct(FlexiDb db, int catId, string name, decimal price)
        {
            db.Execute(@"
INSERT INTO Products(CategoryId, Name, Price)
VALUES ($cid, $name, $price);",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$cid", catId);
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$price", (double)price);
            });
        }
    }
}
