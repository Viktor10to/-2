using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Flexi2.Models;
using Flexi2.ViewModels;

namespace Flexi2.Views
{
    public partial class AdminFloorView : UserControl
    {
        public AdminFloorView() => InitializeComponent();

        private void TableThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is not Thumb thumb) return;
            if (thumb.DataContext is not TableModel table) return;

            table.PosX = Math.Max(0, table.PosX + e.HorizontalChange);
            table.PosY = Math.Max(0, table.PosY + e.VerticalChange);
        }

        private void TableThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is not Thumb thumb) return;
            if (thumb.DataContext is not TableModel table) return;

            // Persist layout immediately when the drag ends
            if (DataContext is AdminFloorViewModel vm)
            {
                vm.SaveTableLayout(table);
            }
        }
    }
}
