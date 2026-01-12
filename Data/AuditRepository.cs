using System;
using System.Collections.Generic;
using Flexi2.Models;

namespace Flexi2.Data
{
    public sealed class AuditRepository
    {
        private readonly FlexiDb _db;
        public AuditRepository(FlexiDb db) => _db = db;

        public void Log(int userId, string action, string entity, int entityId, string details)
        {
            _db.Execute(@"INSERT INTO AuditLog(AtUtc,UserId,Action,Entity,EntityId,Details)
                          VALUES($t,$u,$a,$e,$id,$d);",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$t", DateTime.UtcNow.ToString("O"));
                cmd.Parameters.AddWithValue("$u", userId);
                cmd.Parameters.AddWithValue("$a", action);
                cmd.Parameters.AddWithValue("$e", entity);
                cmd.Parameters.AddWithValue("$id", entityId);
                cmd.Parameters.AddWithValue("$d", details);
            });
        }

        public List<AuditLog> Latest(int take = 200)
        {
            var list = new List<AuditLog>();
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT Id, AtUtc, UserId, Action, Entity, EntityId, Details
                                FROM AuditLog ORDER BY Id DESC LIMIT $n;";
            cmd.Parameters.AddWithValue("$n", take);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new AuditLog
                {
                    Id = r.GetInt32(0),
                    AtUtc = DateTime.Parse(r.GetString(1)),
                    UserId = r.GetInt32(2),
                    Action = r.GetString(3),
                    Entity = r.GetString(4),
                    EntityId = r.GetInt32(5),
                    Details = r.GetString(6)
                });
            }
            return list;
        }
    }
}
