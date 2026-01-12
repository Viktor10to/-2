using System;
using System.Collections.Generic;
using Flexi2.Models;

namespace Flexi2.Data
{
    public sealed class FloorRepository
    {
        private readonly FlexiDb _db;
        public FloorRepository(FlexiDb db) => _db = db;

        public List<ZoneModel> GetZones()
        {
            var list = new List<ZoneModel>();
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Name FROM Zones ORDER BY Name;";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new ZoneModel { Id = r.GetInt32(0), Name = r.GetString(1) });
            return list;
        }

        public List<TableModel> GetTablesByZone(int zoneId)
        {
            var list = new List<TableModel>();
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT Id, Name, Status, OwnerUserId, OpenedAtUtc, CurrentTotal
                                FROM Tables WHERE ZoneId=$z ORDER BY Name;";
            cmd.Parameters.AddWithValue("$z", zoneId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new TableModel
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1),
                    Status = (TableStatus)r.GetInt32(2),
                    OwnerUserId = r.IsDBNull(3) ? null : r.GetInt32(3),
                    OpenedAtUtc = r.IsDBNull(4) ? null : DateTime.Parse(r.GetString(4)),
                    CurrentTotal = (decimal)r.GetDouble(5)
                });
            }
            return list;
        }

        public void SetTableOccupied(int tableId, int ownerUserId, DateTime openedAtUtc)
        {
            _db.Execute(@"UPDATE Tables
                          SET Status=1, OwnerUserId=$u, OpenedAtUtc=$t
                          WHERE Id=$id;",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$id", tableId);
                cmd.Parameters.AddWithValue("$u", ownerUserId);
                cmd.Parameters.AddWithValue("$t", openedAtUtc.ToString("O"));
            });
        }

        public void SetTableFree(int tableId)
        {
            _db.Execute(@"UPDATE Tables
                          SET Status=0, OwnerUserId=NULL, OpenedAtUtc=NULL, CurrentTotal=0
                          WHERE Id=$id;",
            cmd => cmd.Parameters.AddWithValue("$id", tableId));
        }

        public void UpdateTableTotal(int tableId, decimal total)
        {
            _db.Execute(@"UPDATE Tables SET CurrentTotal=$tot WHERE Id=$id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$id", tableId);
                    cmd.Parameters.AddWithValue("$tot", (double)total);
                });
        }

        // ===== ADMIN CRUD =====

        public int CreateZone(string name)
            => _db.Scalar<int>("INSERT INTO Zones(Name) VALUES ($n); SELECT last_insert_rowid();",
                cmd => cmd.Parameters.AddWithValue("$n", name));

        public int CreateTable(int zoneId, string name)
            => _db.Scalar<int>("INSERT INTO Tables(ZoneId,Name,Status) VALUES ($z,$n,0); SELECT last_insert_rowid();",
                c => { c.Parameters.AddWithValue("$z", zoneId); c.Parameters.AddWithValue("$n", name); });

        public void DeleteTable(int id)
            => _db.Execute("DELETE FROM Tables WHERE Id=$id;", c => c.Parameters.AddWithValue("$id", id));

        // ✅ IMPORTANT: delete zone safely (delete its tables first)
        public void DeleteZoneSafe(int zoneId)
        {
            _db.Execute("DELETE FROM Tables WHERE ZoneId=$z;", c => c.Parameters.AddWithValue("$z", zoneId));
            _db.Execute("DELETE FROM Zones WHERE Id=$z;", c => c.Parameters.AddWithValue("$z", zoneId));
        }
    }
}
