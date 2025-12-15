using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace Flexi2.Data
{
    public class AdminRepository
    {
        private readonly FlexiDb _db;
        public AdminRepository(FlexiDb db) => _db = db;

        public decimal GetTotalRevenue()
        {
            using var cn = _db.Open();
            return cn.ExecuteScalar<decimal>(
                "SELECT IFNULL(SUM(Qty * Price),0) FROM OrderItems");
        }

        public int GetOrdersCount()
        {
            using var cn = _db.Open();
            return cn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Orders WHERE ClosedAt IS NOT NULL");
        }

        public IEnumerable<OrderRow> GetLastOrders()
        {
            using var cn = _db.Open();
            return cn.Query<OrderRow>(@"
SELECT o.Id, o.TableNumber, o.ClosedAt,
       SUM(i.Qty * i.Price) AS Total
FROM Orders o
JOIN OrderItems i ON i.OrderId = o.Id
WHERE o.ClosedAt IS NOT NULL
GROUP BY o.Id
ORDER BY o.Id DESC
LIMIT 10;
");
        }
    }

    public class OrderRow
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public string ClosedAt { get; set; } = "";
        public decimal Total { get; set; }
    }
}
