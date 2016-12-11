using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Devices.Display
{
    /// <summary>
    /// e-Paper storage area
    /// </summary>
    public enum ePaperStorageArea:Byte
    {
        /// <summary>
        /// Nand Flash
        /// </summary>
        NandFlash = 0,
        /// <summary>
        /// TF card
        /// </summary>
        MicroSD = 1
    }
}
