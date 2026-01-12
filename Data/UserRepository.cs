using System;
using System.Collections.Generic;
using Flexi2.Core.Security;
using Flexi2.Models;

namespace Flexi2.Data
{
    public sealed class UserRepository
    {
        private readonly FlexiDb _db;
        public UserRepository(FlexiDb db) => _db = db;

        public List<User> GetAll()
        {
            var list = new List<User>();
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Role, PinHash, PinSalt, IsActive, CreatedAtUtc FROM Users ORDER BY Role DESC, Name ASC;";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new User
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1),
                    Role = (UserRole)r.GetInt32(2),
                    PinHash = r.GetString(3),
                    PinSalt = r.GetString(4),
                    IsActive = r.GetInt32(5) == 1,
                    CreatedAtUtc = DateTime.Parse(r.GetString(6))
                });
            }
            return list;
        }

        public User? TryLoginByPin(string pin)
        {
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Role, PinHash, PinSalt, IsActive, CreatedAtUtc FROM Users WHERE IsActive=1;";
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                var hash = r.GetString(3);
                var salt = r.GetString(4);

                if (PinHasher.Verify(pin, salt, hash))
                {
                    return new User
                    {
                        Id = r.GetInt32(0),
                        Name = r.GetString(1),
                        Role = (UserRole)r.GetInt32(2),
                        PinHash = hash,
                        PinSalt = salt,
                        IsActive = true,
                        CreatedAtUtc = DateTime.Parse(r.GetString(6))
                    };
                }
            }
            return null;
        }

        public int Create(string name, UserRole role, string pin)
        {
            var salt = PinHasher.NewSalt();
            var hash = PinHasher.Hash(pin, salt);

            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Users(Name, Role, PinHash, PinSalt, IsActive, CreatedAtUtc)
VALUES ($name, $role, $hash, $salt, 1, $created);
SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$role", (int)role);
            cmd.Parameters.AddWithValue("$hash", hash);
            cmd.Parameters.AddWithValue("$salt", salt);
            cmd.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void UpdateNameRole(int id, string name, UserRole role, bool isActive)
        {
            _db.Execute(@"
UPDATE Users SET Name=$name, Role=$role, IsActive=$active
WHERE Id=$id;",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$id", id);
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$role", (int)role);
                cmd.Parameters.AddWithValue("$active", isActive ? 1 : 0);
            });
        }

        public void ChangePin(int id, string newPin)
        {
            var salt = PinHasher.NewSalt();
            var hash = PinHasher.Hash(newPin, salt);

            _db.Execute("UPDATE Users SET PinHash=$h, PinSalt=$s WHERE Id=$id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$id", id);
                    cmd.Parameters.AddWithValue("$h", hash);
                    cmd.Parameters.AddWithValue("$s", salt);
                });
        }

        public void Delete(int id)
        {
            _db.Execute("DELETE FROM Users WHERE Id=$id;", cmd => cmd.Parameters.AddWithValue("$id", id));
        }
    }
}
