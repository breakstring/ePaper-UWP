using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Devices.Display
{
    /// <summary>
    /// e-Paper exception
    /// </summary>
    public class ePaperException: System.Exception
    {
        ePaperExceptionCode _exceptionCode;
        public ePaperExceptionCode ePaperExceptionCode
        {
            get
            {
                return _exceptionCode;
            }
        }

        string _commandResult;
        public string CommandResult
        {
            get
            {
                return _commandResult;
            }
        }

        public ePaperException():this(ePaperExceptionCode.Unknow,null)
        {
               
        }
        public ePaperException(string commandResult) : this(ePaperExceptionCode.Unknow, commandResult)
        {

        }


        public ePaperException(ePaperExceptionCode exceptionCode) : this(exceptionCode, null)
        {

        }

        public ePaperException(ePaperExceptionCode exceptionCode, string commandResult) : base()
        {
            _commandResult = commandResult;
            _exceptionCode = exceptionCode;
        }

    }
}
