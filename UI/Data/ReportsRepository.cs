using System;
using System.Collections.Generic;
using System.Globalization;
using Flexi2.Models;

namespace Flexi2.Data
{
    public sealed class ReportsRepository
    {
        private readonly FlexiDb _db;
        public ReportsRepository(FlexiDb db) => _db = db;

        public decimal GetTurnoverSum(DateTime fromUtc, DateTime toUtc)
        {
            var sum = _db.Scalar<double>(@"
SELECT IFNULL(SUM(PaidTotal),0)
FROM Orders
WHERE IsClosed=1
  AND ClosedAtUtc IS NOT NULL
  AND ClosedAtUtc >= $f
  AND ClosedAtUtc <  $t;",
            c =>
            {
                c.Parameters.AddWithValue("$f", fromUtc.ToString("O"));
                c.Parameters.AddWithValue("$t", toUtc.ToString("O"));
            });

            return (decimal)sum;
        }

        public List<TurnoverEntry> GetTurnoverEntries(DateTime fromUtc, DateTime toUtc, int take = 200)
        {
            return _db.Query(@"
SELECT Id, ClosedAtUtc, UserId, TableId, PaidTotal
FROM Orders
WHERE IsClosed=1
  AND ClosedAtUtc IS NOT NULL
  AND ClosedAtUtc >= $f
  AND ClosedAtUtc <  $t
ORDER BY Id DESC
LIMIT $n;",
            r => new TurnoverEntry
            {
                Id = r.GetInt32(0),
                AtUtc = DateTime.Parse(r.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                UserId = r.GetInt32(2),
                TableId = r.GetInt32(3),
                TotalAfterDiscount = (decimal)r.GetDouble(4)
            },
            c =>
            {
                c.Parameters.AddWithValue("$f", fromUtc.ToString("O"));
                c.Parameters.AddWithValue("$t", toUtc.ToString("O"));
                c.Parameters.AddWithValue("$n", take);
            });
        }
    }
}
