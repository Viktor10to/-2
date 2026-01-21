using System.Collections.Generic;

namespace Flexi2.Models
{
    public sealed class ZoneModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<TableModel> Tables { get; set; } = new();
    }
}
