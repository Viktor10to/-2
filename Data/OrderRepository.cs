using System;
using System.Collections.Generic;
using Flexi2.Models;

namespace Flexi2.Data
{
    public sealed class OrderRepository
    {
        private readonly FlexiDb _db;
        public OrderRepository(FlexiDb db) => _db = db;

        public int OpenOrder(int tableId, int ownerUserId)
        {
            var now = DateTime.UtcNow;
            return _db.Scalar<int>(@"
INSERT INTO Orders(TableId,OwnerUserId,OpenedAtUtc,ClosedAtUtc,DiscountPercent,PaidMethod)
VALUES($t,$u,$o,NULL,0,'');
SELECT last_insert_rowid();",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$t", tableId);
                cmd.Parameters.AddWithValue("$u", ownerUserId);
                cmd.Parameters.AddWithValue("$o", now.ToString("O"));
            });
        }

        public int? GetOpenOrderIdForTable(int tableId)
        {
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id FROM Orders WHERE TableId=$t AND ClosedAtUtc IS NULL ORDER BY Id DESC LIMIT 1;";
            cmd.Parameters.AddWithValue("$t", tableId);
            var v = cmd.ExecuteScalar();
            if (v == null || v is DBNull) return null;
            return Convert.ToInt32(v);
        }

        public List<OrderItem> GetItems(int orderId)
        {
            var list = new List<OrderItem>();
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT Id, OrderId, ProductId, NameSnapshot, PriceSnapshot, Qty, IsLocked, CreatedAtUtc
                                FROM OrderItems WHERE OrderId=$o ORDER BY Id;";
            cmd.Parameters.AddWithValue("$o", orderId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new OrderItem
                {
                    Id = r.GetInt32(0),
                    OrderId = r.GetInt32(1),
                    ProductId = r.GetInt32(2),
                    NameSnapshot = r.GetString(3),
                    PriceSnapshot = (decimal)r.GetDouble(4),
                    Qty = r.GetInt32(5),
                    IsLocked = r.GetInt32(6) == 1,
                    CreatedAtUtc = DateTime.Parse(r.GetString(7))
                });
            }
            return list;
        }

        public void InsertLockedItems(int orderId, IEnumerable<(Product p, int qty)> items)
        {
            foreach (var (p, qty) in items)
            {
                _db.Execute(@"
INSERT INTO OrderItems(OrderId,ProductId,NameSnapshot,PriceSnapshot,Qty,IsLocked,CreatedAtUtc)
VALUES($o,$pid,$n,$pr,$q,1,$t);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$o", orderId);
                    cmd.Parameters.AddWithValue("$pid", p.Id);
                    cmd.Parameters.AddWithValue("$n", p.Name);
                    cmd.Parameters.AddWithValue("$pr", (double)p.Price);
                    cmd.Parameters.AddWithValue("$q", qty);
                    cmd.Parameters.AddWithValue("$t", DateTime.UtcNow.ToString("O"));
                });
            }
        }

        public void CloseOrder(int orderId, decimal discountPercent, string paidMethod)
        {
            _db.Execute(@"UPDATE Orders
                          SET ClosedAtUtc=$c, DiscountPercent=$d, PaidMethod=$m
                          WHERE Id=$id;",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$id", orderId);
                cmd.Parameters.AddWithValue("$c", DateTime.UtcNow.ToString("O"));
                cmd.Parameters.AddWithValue("$d", (double)discountPercent);
                cmd.Parameters.AddWithValue("$m", paidMethod ?? "");
            });
        }

        public decimal CalcTotal(int orderId)
        {
            using var con = _db.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(PriceSnapshot*Qty),0) FROM OrderItems WHERE OrderId=$o;";
            cmd.Parameters.AddWithValue("$o", orderId);
            return (decimal)Convert.ToDouble(cmd.ExecuteScalar() ?? 0d);
        }

        public void InsertTurnover(int userId, int tableId, decimal totalAfterDiscount)
        {
            _db.Execute(@"INSERT INTO Turnover(AtUtc,UserId,TableId,TotalAfterDiscount)
                          VALUES($t,$u,$tb,$tot);",
            cmd =>
            {
                cmd.Parameters.AddWithValue("$t", DateTime.UtcNow.ToString("O"));
                cmd.Parameters.AddWithValue("$u", userId);
                cmd.Parameters.AddWithValue("$tb", tableId);
                cmd.Parameters.AddWithValue("$tot", (double)totalAfterDiscount);
            });
        }
    }
}
