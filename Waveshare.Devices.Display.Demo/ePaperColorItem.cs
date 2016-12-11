using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Devices.Display;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Waveshare.Devices.Display.Demo
{
    public class ePaperColors:List<ePaperColorItem>
    {
        public ePaperColors():base()
        {
            this.Add(new ePaperColorItem() { ColorName = ePaperColor.Black, ByteValue= 0 });
            this.Add(new ePaperColorItem() { ColorName = ePaperColor.DarkGray, ByteValue = 1 });
            this.Add(new ePaperColorItem() { ColorName = ePaperColor.LightGray, ByteValue = 2 });
            this.Add(new ePaperColorItem() { ColorName = ePaperColor.White, ByteValue = 3 });
        } 
    }

    public class ePaperColorItem
    {
        public ePaperColor ColorName { get; set; }

        public Byte ByteValue { get; set; }

        public Color ColorValue
        {
            get
            {
                Color r = new Color();
                switch(ColorName)
                {
                    case ePaperColor.Black:
                        r = Windows.UI.Colors.Black;
                        break;
                    case ePaperColor.DarkGray:
                        r = Windows.UI.Colors.DarkGray;
                        break;
                    case ePaperColor.LightGray:
                        r = Windows.UI.Colors.LightGray;
                        break;
                    default:
                        r = Windows.UI.Colors.White;
                        break;
                }
                return r;
            }
        }

        public SolidColorBrush Brush
        {
            get
            {
                return new SolidColorBrush(ColorValue);
            }
        }
    }
}
