using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Devices.SerialCommunication;
using Windows.Foundation.Metadata;
using Windows.Devices.Enumeration;
using System.IO;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Net;

namespace Waveshare.Devices.Display
{
    /// <summary>
    /// Waveshare 4.3 E-ink Display
    /// </summary>
    /// <see cref="http://www.waveshare.com/4.3inch-e-paper.htm"/>
    /// <seealso cref="http://www.waveshare.net/wiki/4.3inch_e-Paper"/>
    /// <seealso cref="http://www.waveshare.com/wiki/4.3inch_e-Paper"/>
    public class ePaper
    {
        /// <summary>
        /// Freame head
        /// </summary>
        public static readonly byte[] Frame_Header = new byte[] { 0xA5 };
        /// <summary>
        /// Length of frame head segment
        /// </summary>
        public static readonly short FrameHeaderSegment_Length = 1;
        /// <summary>
        /// Length of frame length segment
        /// </summary>
        public static readonly short FrameLengthSegment_Length = 2;
        /// <summary>
        /// Length of command type segment
        /// </summary>
        public static readonly short CommandTypeSegment_Length = 1;
        /// <summary>
        /// Frame end
        /// </summary>
        public static readonly byte[] Frame_End = new byte[] { 0xCC, 0x33, 0xC3, 0x3C };
        /// <summary>
        /// Length of frame end segment
        /// </summary>
        public static readonly short FrameEndSegment_Length = 4;
        /// <summary>
        /// Length of frame parity segment
        /// </summary>
        public static readonly short FrameParitySegment_Length = 1;
        /// <summary>
        /// Fixed string end
        /// </summary>
        public static readonly byte String_End = 0x00;
        /// <summary>
        /// Serial port write timeout
        /// </summary>
        public int SerialWriteTimeout { get; set; } = 1000;
        /// <summary>
        /// Serial port read timeout
        /// </summary>
        public int SerialReadTimeout { get; set; } = 1000;

        SerialDevice serial = null;

        /// <summary>
        /// constructor
        /// </summary>
        private ePaper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Close the serial port
        /// </summary>
        public void Close()
        {
            if(serial != null)
                serial.Dispose();
            serial = null;
        }


        /// <summary>
        /// Get a e-Paper from serial port name
        /// </summary>
        /// <param name="serialPortName">Serial port name</param>
        /// <returns>e-Paper object</returns>
        public static async Task<ePaper> FromNameAsync(string serialPortName)
        {
            ePaper epaper = new ePaper();

            string aqs = SerialDevice.GetDeviceSelector(serialPortName);
            
            var dis = await DeviceInformation.FindAllAsync(aqs);

            epaper.serial = await SerialDevice.FromIdAsync(dis[0].Id);

            epaper.serial.BaudRate = 115200;
            epaper.serial.DataBits = 8;
            epaper.serial.StopBits = SerialStopBitCount.One;
            epaper.serial.Parity = SerialParity.None;
            epaper.serial.WriteTimeout = TimeSpan.FromMilliseconds(epaper.SerialWriteTimeout);
            epaper.serial.ReadTimeout = TimeSpan.FromMilliseconds(epaper.SerialReadTimeout);
            
            return epaper;
        }

        /// <summary>
        /// Generate the command frame
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameters">parameters</param>
        /// <returns>commmand frame byte array</returns>
        private byte[] GenerateCommandFrame(ePaperCommand command, byte[] parameters)
        {
            int frameLength = ePaper.FrameHeaderSegment_Length + ePaper.FrameLengthSegment_Length + ePaper.CommandTypeSegment_Length + parameters.Length + ePaper.FrameEndSegment_Length + ePaper.FrameParitySegment_Length;

            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {

                writer.Write(ePaper.Frame_Header);

                byte[] frameLengthBytes = BitConverter.GetBytes((short)frameLength);
                writer.Write(frameLengthBytes[1]);
                writer.Write(frameLengthBytes[0]);

                writer.Write((byte)command);

                if (parameters.Length > 0)
                    writer.Write(parameters);
                writer.Write(ePaper.Frame_End);

                byte[] frame = stream.ToArray();
                byte parity = GenerateParity(frame);
                writer.Write(parity);
            }

            return stream.ToArray();
        }
        /// <summary>
        /// Generate the parity
        /// </summary>
        /// <param name="frame">frame bytes</param>
        /// <returns></returns>
        private byte GenerateParity(byte[] frame)
        {
            byte parityByte = (byte)0x00;

            
            for (int i = 0; i < frame.Length; i++)
            {

                parityByte ^= frame[i];

            }

            return parityByte;
        }

        /// <summary>
        /// Send command
        /// </summary>
        /// <param name="command">command</param>
        /// <returns>return from e-Paper</returns>
        private async Task<string> SendCommand(ePaperCommand command)
        {
            
            return await SendCommand(command,new byte[0]);
        }

        /// <summary>
        /// Send command to e-Paper
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result from e-Paper</returns>
        private async Task<string> SendCommand(ePaperCommand command, byte[] parameters)
        {
            string rxBuffer;

            byte[] commandFrame = GenerateCommandFrame(command, parameters);
            DataWriter dw = new DataWriter(this.serial.OutputStream);
            try
            {
                
                dw.WriteBytes(commandFrame);
                await dw.StoreAsync();
            }
            catch(Exception e)
            {
                throw new ePaperException(ePaperExceptionCode.Unknow, e.Message);
            }
            finally
            {
                dw.DetachStream();
                dw = null;
            }
      

            const uint maxReadLength = 32;
            DataReader dr = new DataReader(this.serial.InputStream);
            try
            {
                uint bytesToRead = await dr.LoadAsync(maxReadLength);
                rxBuffer = dr.ReadString(bytesToRead);
            }
            catch (Exception e)
            {
                throw new ePaperException(ePaperExceptionCode.Unknow, e.Message);
            }
            finally
            {
                dr.DetachStream();
                dr = null;
            }
            System.Diagnostics.Debug.WriteLine(command.ToString() +  " return:" + rxBuffer);
            return rxBuffer;

        }
        /// <summary>
        /// Throw the e-Paper exception
        /// </summary>
        /// <param name="rxBuffer">return from e-Paper</param>
        private static void ThrowException(string rxBuffer)
        {
            int errorCode;
            if(rxBuffer.StartsWith("Error:"))
            {
                if (!int.TryParse(rxBuffer.Substring(6), out errorCode))
                {
                    errorCode = 250;
                }
            }
            else
            {
                errorCode = 250;
            }

            throw new ePaperException((ePaperExceptionCode)errorCode, rxBuffer);
        }


        #region System Settings
        /// <summary>
        /// Hand shake with e-Paper
        /// </summary>
        /// <returns>void</returns>
        public async Task HandShakeAsync()
        {

            string result = await SendCommand(ePaperCommand.HandShake);

            if (result != "OK")
            {
                ThrowException(result);
            }

        }

        /// <summary>
        /// Not implement yet
        /// </summary>
        /// <param name="baudRate"></param>
        public void SetBaudRateAsync(uint baudRate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get current baudrate
        /// </summary>
        /// <returns>baudrate</returns>
        public async Task<uint> GetBaudRateAsync()
        {
            string result =await SendCommand(ePaperCommand.GetBaudRate);
            uint baudrate;
            if(!uint.TryParse(result, out baudrate))
            {
                ThrowException(result);
            }
            return baudrate;
        } 

        /// <summary>
        /// Get e-Paper storage area setting
        /// </summary>
        /// <returns>storage area</returns>
        public async Task<ePaperStorageArea> GetStorageAreaAsync()
        {
            string result = await SendCommand(ePaperCommand.GetStorageArea);
            ePaperStorageArea sa;
            if(!Enum.TryParse<ePaperStorageArea>(result, out sa))
            {
                ThrowException(result);
            }
            return sa;
            
        }

        /// <summary>
        /// Set e-Paper storage area setting
        /// </summary>
        /// <param name="storageArea">storage area</param>
        /// <returns>void</returns>
        public async Task SetStorageAreaAsync(ePaperStorageArea storageArea)
        {
            byte[] parameters = new byte[1] { (byte)storageArea };
            string result = await SendCommand(ePaperCommand.SetStorageArea, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }


        /// <summary>
        /// Refresh the screan.
        /// </summary>
        /// <returns>void</returns>
        public async Task RefreshDisplayAsync()
        {
            
            string result = await SendCommand(ePaperCommand.Refresh);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Send the sleep command to e-Paper.
        /// </summary>
        /// <returns>void</returns>
        public async Task SleepAsync()
        {

            string result = await SendCommand(ePaperCommand.Sleep);

            if (result != "O\0")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Get display orientation
        /// </summary>
        /// <returns>display orientation</returns>
        public async Task<ePaperDisplayOrientation> GetDisplayOrientationAsync()
        {
            string result = await SendCommand(ePaperCommand.GetDisplayOrientation);
            ePaperDisplayOrientation o;
            if (!Enum.TryParse<ePaperDisplayOrientation>(result, out o))
            {
                ThrowException(result);
            }
            return o;

        }

        /// <summary>
        /// Set display orientation
        /// </summary>
        /// <param name="o">display orientation</param>
        /// <returns>void</returns>
        public async Task SetDisplayOrientationAsync(ePaperDisplayOrientation o)
        {
            byte[] parameters = new byte[1] { (byte)o };
            string result = await SendCommand(ePaperCommand.SetDisplayOrientation, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Load fonts from TF
        /// </summary>
        /// <returns>void</returns>
        public async Task LoadFontsAsync()
        {

            string result = await SendCommand(ePaperCommand.LoadFonts);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Load images from TF
        /// </summary>
        /// <returns>void</returns>
        public async Task LoadImagesAsync()
        {

            string result = await SendCommand(ePaperCommand.LoadImages);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Set color palette
        /// </summary>
        /// <param name="cp">color palette</param>
        /// <returns>void</returns>
        public async Task SetColorAsync(ePaperColorPalette cp)
        {
            byte[] parameters = new byte[2] { (byte)cp.ForegroundColor,(byte)cp.BackgroundColor};
            string result = await SendCommand(ePaperCommand.SetColor, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }


        /// <summary>
        /// Get color palette
        /// </summary>
        /// <returns>color palette</returns>
        public async Task<ePaperColorPalette> GetColorAsync()
        {
            string result = await SendCommand(ePaperCommand.GetColor);
            ePaperColorPalette pcp=new ePaperColorPalette();
            ePaperColor fColor;
            ePaperColor bColor;
            
            if (!Enum.TryParse<ePaperColor>(result.Substring(0,1), out fColor))
            {
                ThrowException(result);
            }
            if (!Enum.TryParse<ePaperColor>(result.Substring(1, 1), out bColor))
            {
                ThrowException(result);
            }
            pcp.ForegroundColor = fColor;
            pcp.BackgroundColor = bColor;

            return pcp;
        }

        /// <summary>
        /// Set font size
        /// </summary>
        /// <param name="language">language</param>
        /// <param name="fontSize">font size</param>
        /// <returns></returns>
        public async Task SetFontSizeAsync(ePaperLanguage language, ePaperFontSize fontSize)
        {
            byte[] parameters = new byte[1] { (byte)fontSize };
            ePaperCommand cmd;
            if(language == ePaperLanguage.English)
            {
                cmd = ePaperCommand.SetFontSizeEn;
            }
            else
            {
                cmd = ePaperCommand.SetFontSizeCn;
            }
            string result = await SendCommand(cmd, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }
        /// <summary>
        /// Get font size
        /// </summary>
        /// <param name="language">language</param>
        /// <returns>font size</returns>
        public async Task<ePaperFontSize> GetFontSizeAsync(ePaperLanguage language)
        {
            ePaperCommand cmd;
            if (language == ePaperLanguage.English)
            {
                cmd = ePaperCommand.GetFontSizeEn;
            }
            else
            {
                cmd = ePaperCommand.GetFontSizeCn;
            }
            string result = await SendCommand(cmd);

            ePaperFontSize fontSize;
            if (!Enum.TryParse<ePaperFontSize>(result, out fontSize))
            {
                ThrowException(result);
            }
            return fontSize;

        }

        #endregion

        #region Basic Drawing
        /// <summary>
        /// Draw a point
        /// </summary>
        /// <param name="X">coordinate X</param>
        /// <param name="Y">coordinate Y</param>
        /// <returns>void</returns>
        public async Task DrawPointAsync(short X, short Y)
        {

            byte[] xs = BitConverter.GetBytes(X);
            byte[] ys = BitConverter.GetBytes(Y);
            byte[] parameters = new byte[4] {   xs[1], xs[0],
                                                ys[1], ys[0] };

            string result = await SendCommand(ePaperCommand.DrawPoint, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }

        }

        /// <summary>
        /// Draw a line
        /// </summary>
        /// <param name="startX">coordinate X of start</param>
        /// <param name="startY">coordinate Y of start</param>
        /// <param name="endX">coordinate X of end</param>
        /// <param name="endY">coordinate Y of end</param>
        /// <returns>void</returns>
        public async Task DrawLineAsync(short startX, short startY, short endX, short endY)
        {

            byte[] sxs = BitConverter.GetBytes(startX);
            byte[] sys = BitConverter.GetBytes(startY);
            byte[] exs = BitConverter.GetBytes(endX);
            byte[] eys = BitConverter.GetBytes(endY);
            byte[] parameters = new byte[8] {   sxs[1], sxs[0],
                                                sys[1], sys[0],
                                                exs[1], exs[0],
                                                eys[1], eys[0]};

            string result = await SendCommand(ePaperCommand.DrawLine, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }

        }

        /// <summary>
        /// Fill a rectangle
        /// </summary>
        /// <param name="startX">coordinate X of start</param>
        /// <param name="startY">coordinate Y of start</param>
        /// <param name="endX">coordinate X of end</param>
        /// <param name="endY">coordinate Y of end</param>
        /// <returns>void</returns>
        public async Task FillRectangleAsync(short startX, short startY, short endX, short endY)
        {

            byte[] sxs = BitConverter.GetBytes(startX);
            byte[] sys = BitConverter.GetBytes(startY);
            byte[] exs = BitConverter.GetBytes(endX);
            byte[] eys = BitConverter.GetBytes(endY);
            byte[] parameters = new byte[8] {   sxs[1], sxs[0],
                                                sys[1], sys[0],
                                                exs[1], exs[0],
                                                eys[1], eys[0]};

            string result = await SendCommand(ePaperCommand.FillRectangle, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }
        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="startX">coordinate X of start</param>
        /// <param name="startY">coordinate Y of start</param>
        /// <param name="endX">coordinate X of end</param>
        /// <param name="endY">coordinate Y of end</param>
        /// <returns>void</returns>
        public async Task DrawRectangleAsync(short startX, short startY, short endX, short endY)
        {

            byte[] sxs = BitConverter.GetBytes(startX);
            byte[] sys = BitConverter.GetBytes(startY);
            byte[] exs = BitConverter.GetBytes(endX);
            byte[] eys = BitConverter.GetBytes(endY);
            byte[] parameters = new byte[8] {   sxs[1], sxs[0],
                                                sys[1], sys[0],
                                                exs[1], exs[0],
                                                eys[1], eys[0]};

            string result = await SendCommand(ePaperCommand.DrawRectangle, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="X">coordinate X of centre</param>
        /// <param name="Y">coordinate Y of centre</param>
        /// <param name="R">radius</param>
        /// <returns>void</returns>
        public async Task DrawCircleAsync(short X, short Y, short R)
        {

            byte[] xs = BitConverter.GetBytes(X);
            byte[] ys = BitConverter.GetBytes(Y);
            byte[] rs = BitConverter.GetBytes(R);

            byte[] parameters = new byte[6] {   xs[1], xs[0],
                                                ys[1], ys[0],
                                                rs[1], rs[0]};

            string result = await SendCommand(ePaperCommand.DrawCircle, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Fill a circle
        /// </summary>
        /// <param name="X">coordinate X of centre</param>
        /// <param name="Y">coordinate Y of centre</param>
        /// <param name="R">radius</param>
        /// <returns>void</returns>
        public async Task FillCircleAsync(short X, short Y, short R)
        {

            byte[] xs = BitConverter.GetBytes(X);
            byte[] ys = BitConverter.GetBytes(Y);
            byte[] rs = BitConverter.GetBytes(R);

            byte[] parameters = new byte[6] {   xs[1], xs[0],
                                                ys[1], ys[0],
                                                rs[1], rs[0]};

            string result = await SendCommand(ePaperCommand.FillCircle, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

        /// <summary>
        /// Draw a triangle
        /// </summary>
        /// <param name="X1">X of point 1</param>
        /// <param name="Y1">Y of point 1</param>
        /// <param name="X2">X of point 2</param>
        /// <param name="Y2">Y of point 2</param>
        /// <param name="X3">X of point 3</param>
        /// <param name="Y3">Y of point 3</param>
        /// <returns>void</returns>
        public async Task DrawTriangleAsync(short X1, short Y1, short X2, short Y2, short X3, short Y3)
        {

            byte[] xs1 = BitConverter.GetBytes(X1);
            byte[] ys1 = BitConverter.GetBytes(Y1);
            byte[] xs2 = BitConverter.GetBytes(X2);
            byte[] ys2 = BitConverter.GetBytes(Y2);
            byte[] xs3 = BitConverter.GetBytes(X3);
            byte[] ys3 = BitConverter.GetBytes(Y3);
            byte[] parameters = new byte[12] {
                                                xs1[1], xs1[0],
                                                ys1[1], ys1[0],
                                                xs2[1], xs2[0],
                                                ys2[1], ys2[0],
                                                xs3[1], xs3[0],
                                                ys3[1], ys3[0]};

            string result = await SendCommand(ePaperCommand.DrawTriangle, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }

        }

        /// <summary>
        /// Fill a triangle
        /// </summary>
        /// <param name="X1">X of point 1</param>
        /// <param name="Y1">Y of point 1</param>
        /// <param name="X2">X of point 2</param>
        /// <param name="Y2">Y of point 2</param>
        /// <param name="X3">X of point 3</param>
        /// <param name="Y3">Y of point 3</param>
        /// <returns>void</returns>
        public async Task FillTriangleAsync(short X1, short Y1, short X2, short Y2, short X3, short Y3)
        {

            byte[] xs1 = BitConverter.GetBytes(X1);
            byte[] ys1 = BitConverter.GetBytes(Y1);
            byte[] xs2 = BitConverter.GetBytes(X2);
            byte[] ys2 = BitConverter.GetBytes(Y2);
            byte[] xs3 = BitConverter.GetBytes(X3);
            byte[] ys3 = BitConverter.GetBytes(Y3);
            byte[] parameters = new byte[12] {
                                                xs1[1], xs1[0],
                                                ys1[1], ys1[0],
                                                xs2[1], xs2[0],
                                                ys2[1], ys2[0],
                                                xs3[1], xs3[0],
                                                ys3[1], ys3[0]};

            string result = await SendCommand(ePaperCommand.FillTriangle, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }

        }
        /// <summary>
        /// Clean screan
        /// </summary>
        /// <returns>void</returns>
        public async Task CleanScreenAsync()
        {
            string result = await SendCommand(ePaperCommand.ClearScreen);
            if (result != "OK")
            {
                ThrowException(result);
            }
        } 
        #endregion
        /// <summary>
        /// Write text
        /// </summary>
        /// <param name="X">coordinate X</param>
        /// <param name="Y">coordinate Y</param>
        /// <param name="Text">text to write</param>
        /// <returns>void</returns>
        public async Task WriteTextAsync(short X, short Y, string Text)
        {
            byte[] xs = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(X));
            byte[] ys = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Y));
            Encoding gbk = Encoding.GetEncoding("GB18030");
            byte[] t = gbk.GetBytes(Text);
            byte[] parameters = new byte[4 + t.Length + 1];
            xs.CopyTo(parameters, 0);
            ys.CopyTo(parameters, 2);
            t.CopyTo(parameters, 4);


            string result = await SendCommand(ePaperCommand.WriteText, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }
        /// <summary>
        /// paint a image
        /// </summary>
        /// <param name="X">coordinate X</param>
        /// <param name="Y">coordinate Y</param>
        /// <param name="FileName">image file name</param>
        /// <returns>void</returns>
        public async Task ShowImageAsync(short X, short Y, string FileName)
        {
            byte[] xs = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(X));
            byte[] ys = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Y));
            Encoding gbk = Encoding.GetEncoding("GB18030");
            byte[] t = gbk.GetBytes(FileName.ToUpper());

            byte[] parameters = new byte[4 + t.Length + 1];
            xs.CopyTo(parameters, 0);
            ys.CopyTo(parameters, 2);
            t.CopyTo(parameters, 4);


            string result = await SendCommand(ePaperCommand.ShowImage, parameters);

            if (result != "OK")
            {
                ThrowException(result);
            }
        }

    }
}
