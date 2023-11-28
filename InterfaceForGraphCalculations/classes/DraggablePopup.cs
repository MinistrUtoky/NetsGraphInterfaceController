using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace InterfaceForGraphCalculations.classes
{
    internal partial class DraggablePopup : Popup
    {
        public DraggablePopup()
        {
            var thumb = new Thumb
            {
                Width = 0,
                Height = 0,
            };

            MouseDown += (sender, e) =>
            {
                thumb.RaiseEvent(e);
            };

            thumb.DragDelta += (sender, e) =>
            {
                HorizontalOffset += e.HorizontalChange;
                VerticalOffset += e.VerticalChange;
            };
        }
    }
}
