using System;
using Microsoft.Data.Sqlite;

namespace Flexi2.Data
{
    public sealed class FlexiDb
    {
        private readonly string _cs;

        public FlexiDb(string dbPath)
        {
            _cs = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            }.ToString();
        }

        public SqliteConnection Open()
        {
            var con = new SqliteConnection(_cs);
            con.Open();
            return con;
        }

        public void Execute(string sql, Action<SqliteCommand>? bind = null)
        {
            using var con = Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            bind?.Invoke(cmd);
            cmd.ExecuteNonQuery();
        }

        public T Scalar<T>(string sql, Action<SqliteCommand>? bind = null)
        {
            using var con = Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            bind?.Invoke(cmd);
            var val = cmd.ExecuteScalar();
            if (val == null || val is DBNull) return default!;
            return (T)Convert.ChangeType(val, typeof(T));
        }
    }
}
