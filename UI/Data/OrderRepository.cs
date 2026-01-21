using Flexi2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Flexi2.Data
{
    public sealed class OrderRepository
    {
        private readonly FlexiDb _db;
        public OrderRepository(FlexiDb db) => _db = db;

        public int EnsureOpenOrder(int tableId, int userId)
        {
            var existing = _db.Scalar<long>(@"
SELECT IFNULL(
 (SELECT Id FROM Orders WHERE TableId=$t AND IsClosed=0 ORDER BY Id DESC LIMIT 1),
 0);",
            c => c.Parameters.AddWithValue("$t", tableId));

            if (existing != 0) return (int)existing;

            _db.Execute(@"
INSERT INTO Orders(TableId,UserId,CreatedAtUtc,IsClosed,DiscountPercent)
VALUES($t,$u,$dt,0,0);",
            c =>
            {
                c.Parameters.AddWithValue("$t", tableId);
                c.Parameters.AddWithValue("$u", userId);
                c.Parameters.AddWithValue("$dt", DateTime.UtcNow.ToString("O"));
            });

            var id = _db.Scalar<long>("SELECT last_insert_rowid();");
            return (int)id;
        }

        public List<OrderItem> GetLockedItems(int tableId)
        {
            var orderId = _db.Scalar<long>(@"
SELECT IFNULL((SELECT Id FROM Orders WHERE TableId=$t AND IsClosed=0 ORDER BY Id DESC LIMIT 1),0);",
            c => c.Parameters.AddWithValue("$t", tableId));

            if (orderId == 0) return new();

            return _db.Query(@"
SELECT Id, OrderId, ProductId, ProductName, UnitPrice, Qty, IsLocked
FROM OrderItems
WHERE OrderId = $o
ORDER BY Id;",
            r => new OrderItem
            {
                Id = r.GetInt32(0),
                OrderId = r.GetInt32(1),
                ProductId = r.GetInt32(2),
                ProductName = r.GetString(3),
                UnitPrice = (decimal)r.GetDouble(4),
                Qty = r.GetInt32(5),
                IsLocked = r.GetInt32(6) == 1
            },
            c => c.Parameters.AddWithValue("$o", (int)orderId));
        }

        public void AddLockedItems(int orderId, IEnumerable<OrderItem> items)
        {
            foreach (var it in items)
            {
                _db.Execute(@"
INSERT INTO OrderItems(OrderId,ProductId,ProductName,UnitPrice,Qty,IsLocked)
VALUES($o,$p,$n,$pr,$q,1);",
                c =>
                {
                    c.Parameters.AddWithValue("$o", orderId);
                    c.Parameters.AddWithValue("$p", it.ProductId);
                    c.Parameters.AddWithValue("$n", it.ProductName);
                    c.Parameters.AddWithValue("$pr", (double)it.UnitPrice);
                    c.Parameters.AddWithValue("$q", it.Qty);
                });
            }
        }

        public decimal GetCurrentTotal(int tableId)
        {
            var orderId = _db.Scalar<long>(@"
SELECT IFNULL((SELECT Id FROM Orders WHERE TableId=$t AND IsClosed=0 ORDER BY Id DESC LIMIT 1),0);",
            c => c.Parameters.AddWithValue("$t", tableId));

            if (orderId == 0) return 0;

            var sum = _db.Scalar<double>(@"
SELECT IFNULL(SUM(UnitPrice * Qty),0)
FROM OrderItems
WHERE OrderId=$o;",
            c => c.Parameters.AddWithValue("$o", (int)orderId));

            return (decimal)sum;
        }

        public void CloseOrder(int tableId, string paymentMethod, decimal discountPercent, decimal tipAmount)
        {
            // calculate final total for the currently open order of this table
            var orderId = _db.Scalar<long>(@"
SELECT IFNULL((SELECT Id FROM Orders WHERE TableId=$t AND IsClosed=0 ORDER BY Id DESC LIMIT 1),0);",
                c => c.Parameters.AddWithValue("$t", tableId));

            if (orderId == 0) return;

            var subtotal = _db.Scalar<double>(@"
SELECT IFNULL(SUM(UnitPrice * Qty),0)
FROM OrderItems
WHERE OrderId=$o;",
                c => c.Parameters.AddWithValue("$o", (int)orderId));

            var discPct = (double)discountPercent;
            if (discPct < 0) discPct = 0;
            if (discPct > 100) discPct = 100;

            var discountAmount = subtotal * (discPct / 100.0);
            var afterDiscount = subtotal - discountAmount;
            var tip = (double)tipAmount;
            if (tip < 0) tip = 0;
            var finalTotal = afterDiscount + tip;

            _db.Execute(@"
UPDATE Orders
SET IsClosed=1,
    ClosedAtUtc=$dt,
    DiscountPercent=$dp,
    DiscountAmount=$da,
    TipAmount=$tip,
    Subtotal=$sub,
    FinalTotal=$fin,
    PaymentMethod=$pm,
    PaidTotal=$paid
WHERE Id=$id;",
            c =>
            {
                c.Parameters.AddWithValue("$dt", DateTime.UtcNow.ToString("O"));
                c.Parameters.AddWithValue("$dp", discPct);
                c.Parameters.AddWithValue("$da", discountAmount);
                c.Parameters.AddWithValue("$tip", tip);
                c.Parameters.AddWithValue("$sub", subtotal);
                c.Parameters.AddWithValue("$fin", finalTotal);
                c.Parameters.AddWithValue("$pm", string.IsNullOrWhiteSpace(paymentMethod) ? "Cash" : paymentMethod);
                c.Parameters.AddWithValue("$paid", finalTotal);
                c.Parameters.AddWithValue("$id", (int)orderId);
            });
        }
    }
}
