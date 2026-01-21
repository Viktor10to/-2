using Flexi2.Models;
using System;
using System.Globalization;
using System.Collections.Generic;

namespace Flexi2.Data
{
    public sealed class FloorRepository
    {
        private readonly FlexiDb _db;
        public FloorRepository(FlexiDb db) => _db = db;

        public int AddZone(string name)
        {
            _db.Execute("INSERT INTO Zones(Name) VALUES($n);", c => c.Parameters.AddWithValue("$n", name));
            return (int)_db.Scalar<long>("SELECT last_insert_rowid();");
        }

        public void DeleteZone(int zoneId)
        {
            // Safety: do not delete a zone that still has tables.
            var t = _db.Scalar<long>("SELECT COUNT(1) FROM Tables WHERE ZoneId=$z;", c => c.Parameters.AddWithValue("$z", zoneId));
            if (t > 0)
                throw new InvalidOperationException("Зоната има маси. Първо изтрий масите.");

            _db.Execute("DELETE FROM Zones WHERE Id=$z;", c => c.Parameters.AddWithValue("$z", zoneId));
        }

        public int AddTable(int zoneId, string name)
        {
            // Default layout: place tables in a simple grid. Admin can later drag & drop.
            var idx = (int)_db.Scalar<long>("SELECT COUNT(1) FROM Tables WHERE ZoneId=$z;", c => c.Parameters.AddWithValue("$z", zoneId));
            var x = 20 + (idx % 4) * 190;
            var y = 20 + (idx / 4) * 140;

            _db.Execute("INSERT INTO Tables(ZoneId,Name,PosX,PosY,Width,Height,Status) VALUES($z,$n,$x,$y,170,120,0);", c =>
            {
                c.Parameters.AddWithValue("$z", zoneId);
                c.Parameters.AddWithValue("$n", name);
                c.Parameters.AddWithValue("$x", x);
                c.Parameters.AddWithValue("$y", y);
            });
            return (int)_db.Scalar<long>("SELECT last_insert_rowid();");
        }

        public void UpdateTableLayout(int tableId, double x, double y, double? width = null, double? height = null)
        {
            _db.Execute(@"
UPDATE Tables
SET PosX = $x,
    PosY = $y,
    Width = COALESCE($w, Width),
    Height = COALESCE($h, Height)
WHERE Id = $id;",
            c =>
            {
                c.Parameters.AddWithValue("$x", x);
                c.Parameters.AddWithValue("$y", y);
                c.Parameters.AddWithValue("$w", (object?)width ?? DBNull.Value);
                c.Parameters.AddWithValue("$h", (object?)height ?? DBNull.Value);
                c.Parameters.AddWithValue("$id", tableId);
            });
        }

        public void DeleteTable(int tableId)
        {
            // Safety: do not delete table if it has any orders.
            var o = _db.Scalar<long>("SELECT COUNT(1) FROM Orders WHERE TableId=$t;", c => c.Parameters.AddWithValue("$t", tableId));
            if (o > 0)
                throw new InvalidOperationException("Масата има сметки/поръчки. Не може да се изтрие.");

            _db.Execute("DELETE FROM Tables WHERE Id=$t;", c => c.Parameters.AddWithValue("$t", tableId));
        }

        public List<ZoneModel> GetZones()
        {
            var zones = _db.Query(@"
SELECT Id, Name
FROM Zones
ORDER BY Id;",
            r => new ZoneModel
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
            });

            var tables = _db.Query(@"
SELECT Id, ZoneId, Name, PosX, PosY, Width, Height, Status, OwnerUserId, OpenedAtUtc, CurrentTotal
FROM Tables
ORDER BY ZoneId, Id;",
            r => new TableModel
            {
                Id = r.GetInt32(0),
                ZoneId = r.GetInt32(1),
                Name = r.GetString(2),
                PosX = r.IsDBNull(3) ? 20 : r.GetDouble(3),
                PosY = r.IsDBNull(4) ? 20 : r.GetDouble(4),
                Width = r.IsDBNull(5) ? 170 : r.GetDouble(5),
                Height = r.IsDBNull(6) ? 120 : r.GetDouble(6),
                Status = (TableStatus)r.GetInt32(7),
                OwnerUserId = r.IsDBNull(8) ? null : r.GetInt32(8),
                OpenedAtUtc = r.IsDBNull(9) ? null : DateTime.Parse(r.GetString(9), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CurrentTotal = (decimal)r.GetDouble(10)
            });

            foreach (var z in zones)
                z.Tables = tables.FindAll(t => t.ZoneId == z.Id);

            return zones;
        }

        public void SetTableOccupied(int tableId, int ownerUserId)
        {
            _db.Execute(@"
UPDATE Tables
SET Status = 1,
    OwnerUserId = $u,
    OpenedAtUtc = $dt
WHERE Id = $id;",
            c =>
            {
                c.Parameters.AddWithValue("$u", ownerUserId);
                c.Parameters.AddWithValue("$dt", DateTime.UtcNow.ToString("O"));
                c.Parameters.AddWithValue("$id", tableId);
            });
        }

        public void SetTableFree(int tableId)
        {
            _db.Execute(@"
UPDATE Tables
SET Status = 0,
    OwnerUserId = NULL,
    OpenedAtUtc = NULL,
    CurrentTotal = 0
WHERE Id = $id;",
            c => c.Parameters.AddWithValue("$id", tableId));
        }

        public void UpdateTableTotal(int tableId, decimal total)
        {
            _db.Execute("UPDATE Tables SET CurrentTotal = $t WHERE Id = $id;",
                c =>
                {
                    c.Parameters.AddWithValue("$t", (double)total);
                    c.Parameters.AddWithValue("$id", tableId);
                });
        }
    }
}
