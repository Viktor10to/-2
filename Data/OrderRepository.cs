using Dapper;
using Flexi2.Models;
using System;
using System.Collections.Generic;

namespace Flexi2.Data
{
    public class OrderRepository
    {
        private readonly FlexiDb _db;
        public OrderRepository(FlexiDb db) => _db = db;

        public int CreateOrder(int tableNumber)
        {
            using var cn = _db.Open();
            return cn.ExecuteScalar<int>(
                "INSERT INTO Orders(TableNumber,OpenedAt) VALUES(@t,@d); SELECT last_insert_rowid();",
                new { t = tableNumber, d = DateTime.Now.ToString("s") });
        }

        public void AddItems(int orderId, IEnumerable<OrderItem> items)
        {
            using var cn = _db.Open();
            foreach (var i in items)
            {
                cn.Execute(
                    "INSERT INTO OrderItems(OrderId,ProductName,Qty,Price) VALUES(@o,@n,@q,@p)",
                    new { o = orderId, n = i.Product.Name, q = i.Qty, p = i.Product.Price });
            }
        }

        public void CloseOrder(int orderId)
        {
            using var cn = _db.Open();
            cn.Execute("UPDATE Orders SET ClosedAt=@d WHERE Id=@i",
                new { d = DateTime.Now.ToString("s"), i = orderId });
        }
    }
}
