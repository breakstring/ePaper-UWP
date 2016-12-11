using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Devices.Display
{
    /// <summary>
    /// e-Paper command
    /// </summary>
    public enum ePaperCommand:byte
    {
        #region System Control
        HandShake = 0x00,
        SetBaudRate = 0x01,
        GetBaudRate = 0x02,
        GetStorageArea = 0x06,
        SetStorageArea = 0x07,
        Sleep = 0x08,
        Refresh = 0x0A,
        GetDisplayOrientation = 0x0C,
        SetDisplayOrientation = 0x0D,
        LoadFonts =0x0E ,
        LoadImages = 0x0F,
        #endregion

        #region Display Setting
        SetColor = 0x10,
        GetColor = 0x11,
        GetFontSizeEn = 0x1C,
        GetFontSizeCn = 0x1D,
        SetFontSizeEn = 0x1E,
        SetFontSizeCn = 0x1F,
        #endregion


        #region Basic Drawing
        DrawPoint = 0x20,
        DrawLine = 0x22,
        FillRectangle = 0x24,
        DrawRectangle = 0x25,
        DrawCircle = 0x26,
        FillCircle = 0x27,
        DrawTriangle = 0x28,
        FillTriangle = 0x29,
        ClearScreen = 0x2E, 
        #endregion

        WriteText = 0x30,

        ShowImage = 0x70
    }
}
