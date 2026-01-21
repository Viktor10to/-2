using Flexi2.Core.Security;
using Flexi2.Models;
using System.Collections.Generic;

namespace Flexi2.Data
{
    public sealed class UserRepository
    {
        private readonly FlexiDb _db;
        public UserRepository(FlexiDb db) => _db = db;

        public User? TryLoginByPin(string pin, UserRole role)
        {
            pin = (pin ?? "").Trim();
            if (pin.Length == 0) return null;

            var users = _db.Query(@"
SELECT Id, Name, PinHash, PinSalt, Role, IsActive
FROM Users
WHERE Role = $r AND IsActive = 1;",
            r => new User
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                PinHash = r.GetString(2),
                PinSalt = r.GetString(3),
                Role = (UserRole)r.GetInt32(4),
                IsActive = r.GetInt32(5) == 1
            },
            c => c.Parameters.AddWithValue("$r", (int)role));

            foreach (var u in users)
                if (PinHasher.Verify(pin, u.PinSalt, u.PinHash))
                    return u;

            return null;
        }

        public List<User> GetAll()
        {
            return _db.Query(@"
SELECT Id, Name, PinHash, PinSalt, Role, IsActive
FROM Users
ORDER BY Role DESC, Name ASC;",
            r => new User
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                PinHash = r.GetString(2),
                PinSalt = r.GetString(3),
                Role = (UserRole)r.GetInt32(4),
                IsActive = r.GetInt32(5) == 1
            });
        }

        public void Create(string name, UserRole role, string pin)
        {
            name = (name ?? "").Trim();
            pin = (pin ?? "").Trim();
            if (name.Length == 0 || pin.Length == 0) return;

            var salt = PinHasher.NewSaltHex();
            var hash = PinHasher.Hash(pin, salt);

            _db.Execute(@"
INSERT INTO Users(Name, PinHash, PinSalt, Role, IsActive)
VALUES ($n, $h, $s, $r, 1);",
            c =>
            {
                c.Parameters.AddWithValue("$n", name);
                c.Parameters.AddWithValue("$h", hash);
                c.Parameters.AddWithValue("$s", salt);
                c.Parameters.AddWithValue("$r", (int)role);
            });
        }

        public void UpdateNameRole(int id, string name, UserRole role, bool isActive)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return;

            _db.Execute(@"
UPDATE Users
SET Name=$n, Role=$r, IsActive=$a
WHERE Id=$id;",
            c =>
            {
                c.Parameters.AddWithValue("$n", name);
                c.Parameters.AddWithValue("$r", (int)role);
                c.Parameters.AddWithValue("$a", isActive ? 1 : 0);
                c.Parameters.AddWithValue("$id", id);
            });
        }

        public void ChangePin(int id, string newPin)
        {
            newPin = (newPin ?? "").Trim();
            if (newPin.Length == 0) return;

            var salt = PinHasher.NewSaltHex();
            var hash = PinHasher.Hash(newPin, salt);

            _db.Execute(@"
UPDATE Users
SET PinHash=$h, PinSalt=$s
WHERE Id=$id;",
            c =>
            {
                c.Parameters.AddWithValue("$h", hash);
                c.Parameters.AddWithValue("$s", salt);
                c.Parameters.AddWithValue("$id", id);
            });
        }

        public void Delete(int id)
        {
            _db.Execute("DELETE FROM Users WHERE Id=$id;", c => c.Parameters.AddWithValue("$id", id));
        }
    }
}
