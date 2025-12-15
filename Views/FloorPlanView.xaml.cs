using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Flexi2.Models;
using Flexi2.ViewModels;

namespace Flexi2.Views
{
    public partial class FloorPlanView : UserControl
    {
        private bool _isDragging;
        private Point _startPoint;
        private TableModel? _table;

        public FloorPlanView()
        {
            InitializeComponent();
        }

        private void Table_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(this);
            _table = (sender as FrameworkElement)?.DataContext as TableModel;
            CaptureMouse();
        }

        private void Table_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _table == null) return;

            var pos = e.GetPosition(this);

            _table.X += pos.X - _startPoint.X;
            _table.Y += pos.Y - _startPoint.Y;

            _startPoint = pos;
        }

        private void Table_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();

            // ако не е местена → считаме за клик
            if (_isDragging && _table != null)
            {
                var vm = DataContext as FloorPlanViewModel;
                vm?.TableClickCommand.Execute(_table);
            }

            _isDragging = false;
            _table = null;
        }
    }
}
