using System;
using System.Collections.Generic;
using System.Linq;
using FlexiPOS.Models;

namespace FlexiPOS.Services
{
    public sealed class InMemoryDataStore
    {
        private readonly Dictionary<int, TableOrderState> _orders = new();

        public List<CategoryModel> Categories { get; } = new()
        {
            new CategoryModel{ Id=1, Name="Кафе" },
            new CategoryModel{ Id=2, Name="Безалкохолни" },
            new CategoryModel{ Id=3, Name="Алкохол" },
            new CategoryModel{ Id=4, Name="Храна" }
        };

        public List<ProductModel> Products { get; } = new()
        {
            new ProductModel{ Id=101, CategoryId=1, Name="Еспресо", Price=2.50m },
            new ProductModel{ Id=102, CategoryId=1, Name="Капучино", Price=3.80m },
            new ProductModel{ Id=103, CategoryId=1, Name="Фрапе", Price=5.50m, HasModifiers=true },

            new ProductModel{ Id=201, CategoryId=2, Name="Кола", Price=3.50m },
            new ProductModel{ Id=202, CategoryId=2, Name="Вода", Price=2.00m },

            new ProductModel{ Id=401, CategoryId=4, Name="Бургер", Price=9.90m, HasModifiers=true },
            new ProductModel{ Id=402, CategoryId=4, Name="Пържени картофи", Price=4.90m },
        };

        public TableOrderState GetOrCreateOrder(int tableId, int ownerUserId)
        {
            if (!_orders.TryGetValue(tableId, out var state))
            {
                state = new TableOrderState
                {
                    TableId = tableId,
                    OwnerUserId = ownerUserId,
                    OpenedAt = DateTime.Now
                };
                _orders[tableId] = state;
            }
            return state;
        }

        public TableOrderState? TryGetOrder(int tableId)
            => _orders.TryGetValue(tableId, out var s) ? s : null;

        public void CloseOrder(int tableId)
        {
            if (_orders.ContainsKey(tableId))
                _orders.Remove(tableId);
        }

        public void LockDraftLines(int tableId)
        {
            if (!_orders.TryGetValue(tableId, out var state)) return;

            foreach (var l in state.Lines.Where(x => !x.IsLocked))
                l.IsLocked = true;
        }

        public decimal GetTableTotal(int tableId)
        {
            if (!_orders.TryGetValue(tableId, out var state)) return 0m;
            var subtotal = state.Lines.Sum(l => l.Total);
            var discount = subtotal * (state.DiscountPercent / 100m);
            return subtotal - discount;
        }
    }
}
