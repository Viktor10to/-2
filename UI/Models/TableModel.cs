using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Flexi2.Models
{
    // Needs change notifications because the admin can drag tables around on the floor plan.
    public sealed class TableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        public int Id { get; set; }
        public int ZoneId { get; set; }
        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        public TableStatus Status { get; set; }

        // Layout (admin can position tables on the floor plan)
        private double _posX = 20;
        public double PosX
        {
            get => _posX;
            set => SetField(ref _posX, value);
        }

        private double _posY = 20;
        public double PosY
        {
            get => _posY;
            set => SetField(ref _posY, value);
        }

        private double _width = 170;
        public double Width
        {
            get => _width;
            set => SetField(ref _width, value);
        }

        private double _height = 120;
        public double Height
        {
            get => _height;
            set => SetField(ref _height, value);
        }

        public int? OwnerUserId { get; set; }
        public DateTime? OpenedAtUtc { get; set; }
        public decimal CurrentTotal { get; set; }
    }
}
