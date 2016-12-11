using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Devices.Display
{
    /// <summary>
    /// e-Paper exception code
    /// </summary>
    public enum ePaperExceptionCode
    {
        /// <summary>
        /// Invalid Command
        /// </summary>
        InvalidCommand = 0,

        /// <summary>
        /// Failed to initialize TF Card
        /// </summary>
        FailedToInitializeTFCard = 1,

        /// <summary>
        /// Invalid Command Arguments
        /// </summary>
        InvalidArgument = 2,

        /// <summary>
        /// Cand not find the TF Card
        /// </summary>
        NonTFCard = 3,

        /// <summary>
        /// The specified file could not be opened.
        /// </summary>
        CannotFindFile = 4,

        /// <summary>
        /// Failed parity check
        /// </summary>
        ParityCheckError = 20,

        /// <summary>
        /// Invalid frame format
        /// </summary>
        InvalidFrame = 21,
        /// <summary>
        /// Unknow error
        /// </summary>
        Unknow = 250
    }
}
