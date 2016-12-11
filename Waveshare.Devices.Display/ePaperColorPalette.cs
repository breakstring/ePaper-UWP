using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Devices.Display
{
    /// <summary>
    /// e-Paper color palette
    /// </summary>
    public class ePaperColorPalette
    {
        /// <summary>
        /// Foreground color
        /// </summary>
        public ePaperColor ForegroundColor { get; set; } = ePaperColor.Black;
        /// <summary>
        /// Background color
        /// </summary>
        public ePaperColor BackgroundColor { get; set; } = ePaperColor.White;
    }
}
