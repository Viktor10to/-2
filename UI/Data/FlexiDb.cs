using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace Flexi2.Data
{
    public sealed class FlexiDb
    {
        private readonly string _dbPath;

        public FlexiDb(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new ArgumentException("dbPath is empty.", nameof(dbPath));

            _dbPath = dbPath;

            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public SqliteConnection Open()
        {
            var cn = new SqliteConnection($"Data Source={_dbPath};Cache=Shared;");
            cn.Open();
            return cn;
        }

        public int Execute(string sql, Action<SqliteCommand>? bind = null)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            bind?.Invoke(cmd);
            return cmd.ExecuteNonQuery();
        }

        public T Scalar<T>(string sql, Action<SqliteCommand>? bind = null)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            bind?.Invoke(cmd);

            var obj = cmd.ExecuteScalar();
            if (obj == null || obj is DBNull) return default!;
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public List<T> Query<T>(string sql, Func<SqliteDataReader, T> map, Action<SqliteCommand>? bind = null)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            bind?.Invoke(cmd);

            using var r = cmd.ExecuteReader();
            var list = new List<T>();
            while (r.Read())
                list.Add(map(r));
            return list;
        }
    }
}
