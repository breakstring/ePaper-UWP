using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Waveshare.Devices.Display.Demo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = new ePaperColors();
        }

        Waveshare.Devices.Display.ePaper epaper;
        bool updateCNFontFlag = false;
        bool updateENFontFlag = false;
        bool updateStorageFlag = false;
        bool updateDisplayOrientation = false;

        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {

            epaper = await ePaper.FromNameAsync(txtComPort.Text);
            SetStatusText("Serial port openned");
            RefreshSettings(true);
            SetButtonsStatus(true);
        }

        private void SetStatusText(string txt)
        {
            this.txtReturn.Text = txt;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            epaper.Close();
            SetStatusText( "Closed");
            SetButtonsStatus(false);

        }

        private async void btnHandShake_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.HandShakeAsync() ;
                SetStatusText("Handshake OK");
                
            }
            catch(ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }

        }

        private void SetButtonsStatus(bool OK)
        {

            this.btnOpen.IsEnabled = !OK;
            this.btnClose.IsEnabled = OK;
            this.btnHandShake.IsEnabled = OK;
            this.btnRefreshDisplay.IsEnabled = OK;
            this.btnClean.IsEnabled = OK;
            this.btnSleep.IsEnabled = OK;
            this.btnLoadFonts.IsEnabled = OK;
            this.btnLoadImages.IsEnabled = OK;

            this.tgsStorageArea.IsEnabled = OK;
            this.tgsDisplayOrientation.IsEnabled = OK;
            this.foreColorList.IsEnabled = OK;
            this.backColorList.IsEnabled = OK;
            this.rbtCNFontSize32.IsEnabled = OK;
            this.rbtCNFontSize48.IsEnabled = OK;
            this.rbtCNFontSize64.IsEnabled = OK;
            this.rbtENFontSize32.IsEnabled = OK;
            this.rbtENFontSize48.IsEnabled = OK;
            this.rbtENFontSize64.IsEnabled = OK;

            this.btnDrawPoint.IsEnabled = OK;
            this.btnDrawLine.IsEnabled = OK;
            this.btnDrawRectangle.IsEnabled = OK;
            this.btnFillRectangle.IsEnabled = OK;
            this.btnDrawTriangle.IsEnabled = OK;
            this.btnFillTriangle.IsEnabled = OK;
            this.btnDrawCircle.IsEnabled = OK;
            this.btnFillCircle.IsEnabled = OK;
            this.btnWriteText.IsEnabled = OK;
            this.btnShowImage.IsEnabled = OK;

        }

        private async void btnGetBaudRate_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                uint b = await epaper.GetBaudRateAsync();
                this.txtReturn.Text = "Baudrate:" + b.ToString();
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void tgsStorageArea_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if(toggleSwitch!=null)
            {
                if(updateStorageFlag)
                {
                    Byte sa = 0;
                    if (toggleSwitch.IsOn)
                    {
                        sa = 1;
                    }

                    try
                    {
                        ePaperStorageArea storageArea = (ePaperStorageArea)sa;
                        await epaper.SetStorageAreaAsync(storageArea);
                        SetStatusText("Storage Area change to:" + storageArea.ToString());
                    }
                    catch (ePaperException ex)
                    {
                        SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                        RefreshSettings(false);
                    }
                }
                updateStorageFlag = true;
            }
        }

        private async void RefreshSettings(bool updateStatusText)
        {
            await GetStorageArea();

            await GetDisplayOrientation();

            await GetColorPalette();

            await GetCNFontSize();

            await GetENFontSize();

            if (updateStatusText)
            {
                SetStatusText("All settings refreshed");
            }
        }

        private async Task GetDisplayOrientation()
        {
            ePaperDisplayOrientation o = await epaper.GetDisplayOrientationAsync();
            if (o == ePaperDisplayOrientation.Normal)
            {
                tgsDisplayOrientation.IsOn = false;
            }
            else
            {
                tgsDisplayOrientation.IsOn = true;
            }
        }

        private async Task GetColorPalette()
        {
            ePaperColorPalette c = await epaper.GetColorAsync();
            foreColorList.SelectedIndex = (int)c.ForegroundColor;
            backColorList.SelectedIndex = (int)c.BackgroundColor;
        }

        private async Task GetStorageArea()
        {
            ePaperStorageArea sa = await epaper.GetStorageAreaAsync();
            if (sa == ePaperStorageArea.NandFlash)
            {
                tgsStorageArea.IsOn = false;
            }
            else
            {
                tgsStorageArea.IsOn = true;
            }
        }

        private async Task GetCNFontSize()
        {
            ePaperFontSize fs = await epaper.GetFontSizeAsync(ePaperLanguage.Chinese);
            switch (fs)
            {
                case ePaperFontSize.Small:
                    this.rbtCNFontSize32.IsChecked = true;
                    break;
                case ePaperFontSize.Middle:
                    this.rbtCNFontSize48.IsChecked = true;
                    break;
                case ePaperFontSize.Large:
                    this.rbtCNFontSize64.IsChecked = true;
                    break;
            }
        }

        private async Task GetENFontSize()
        {
            ePaperFontSize fs = await epaper.GetFontSizeAsync(ePaperLanguage.English);
            switch (fs)
            {
                case ePaperFontSize.Small:
                    this.rbtENFontSize32.IsChecked = true;
                    break;
                case ePaperFontSize.Middle:
                    this.rbtENFontSize48.IsChecked = true;
                    break;
                case ePaperFontSize.Large:
                    this.rbtENFontSize64.IsChecked = true;
                    break;
            }
        }

        private async void btnRefreshDisplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                await epaper.RefreshDisplayAsync();
                SetStatusText("Display refreshed.");
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                RefreshSettings(false);
            }
        }

        private async void btnSleep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.SleepAsync();
                SetStatusText("(～﹃～)~zZ......");
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                RefreshSettings(false);
            }
        }

        private async void tgsDisplayOrientation_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            
            if (toggleSwitch != null)
            {
                if(updateDisplayOrientation)
                {
                    Byte o = 0;
                    if (toggleSwitch.IsOn)
                    {
                        o = 1;
                    }

                    try
                    {
                        ePaperDisplayOrientation orientation = (ePaperDisplayOrientation)o;
                        await epaper.SetDisplayOrientationAsync(orientation);
                        SetStatusText("Display Orientation change to:" + orientation.ToString());
                    }
                    catch (ePaperException ex)
                    {
                        SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                        RefreshSettings(false);
                    }
                }
                updateDisplayOrientation = true;
            }
        }

        private async void btnLoadImages_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.LoadImagesAsync();
                SetStatusText("Images loaded.");
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                RefreshSettings(false);
            }
        }

        private  async void btnLoadFonts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.LoadFontsAsync();
                SetStatusText("Fonts loaded.");
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                RefreshSettings(false);
            }
        }

        private async void foreColorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if(e.RemovedItems.Count>0)
            {
                await SetColor();
            }
        }



        private async Task SetColor()
        {
            try
            {
                ePaperColorPalette pcp = new ePaperColorPalette();
                pcp.ForegroundColor = (ePaperColor)foreColorList.SelectedIndex;
                pcp.BackgroundColor = (ePaperColor)backColorList.SelectedIndex;
                await epaper.SetColorAsync(pcp);
                SetStatusText("Color Palette saved.");
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                RefreshSettings(false);
            }
        }

        private async void backColorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                await SetColor();
            }
        }

        private async void rbtCNFontSize_Checked(object sender, RoutedEventArgs e)
        {
            
            RadioButton rb = sender as RadioButton;
            if(updateCNFontFlag)
            {
                try
                {
                    await epaper.SetFontSizeAsync(ePaperLanguage.Chinese, (ePaperFontSize)Byte.Parse(rb.Tag.ToString()));
                    SetStatusText("Chinese font size set to " + rb.Content);
                }
                catch (ePaperException ex)
                {
                    SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                    RefreshSettings(false);
                }
            }
            updateCNFontFlag = true;
        }

        private async void rbtENFontSize_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (updateENFontFlag)
            {
                try
                {
                    await epaper.SetFontSizeAsync(ePaperLanguage.English, (ePaperFontSize)Byte.Parse(rb.Tag.ToString()));
                    SetStatusText("English font size set to " + rb.Content);
                }
                catch (ePaperException ex)
                {
                    SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
                    RefreshSettings(false);
                }
            }
            updateENFontFlag = true;
        }

        private async void btnDrawPoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                await epaper.DrawPointAsync(short.Parse(this.X1.Text),short.Parse(this.Y1.Text));
                SetStatusText("Point: " + X1.Text + "," + Y1.Text);

            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnClean_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.CleanScreenAsync();
                SetStatusText("Screen cleaned: " + X1.Text + "," + Y1.Text);

            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnDrawLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                await epaper.DrawLineAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), short.Parse(this.X2.Text), short.Parse(this.Y2.Text));
                SetStatusText("Line: " + X1.Text + "," + Y1.Text + "->" + X2.Text + "," + Y2.Text);

            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnDrawRectangle_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                await epaper.DrawRectangleAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), short.Parse(this.X2.Text), short.Parse(this.Y2.Text));
                SetStatusText("Rectangle: " + X1.Text + "," + Y1.Text + "->" + X2.Text + "," + Y2.Text);

            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
            catch(Exception ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
            }
        }

        private async void btnFillRectangle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.FillRectangleAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), short.Parse(this.X2.Text), short.Parse(this.Y2.Text));
                SetStatusText("Filled rectangle: " + X1.Text + "," + Y1.Text + "->" + X2.Text + "," + Y2.Text);
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnDrawTriangle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.DrawTriangleAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), short.Parse(this.X2.Text), short.Parse(this.Y2.Text), short.Parse(this.X3.Text), short.Parse(this.Y3.Text));
                SetStatusText("Triangle: " + X1.Text + "," + Y1.Text + "->" + X2.Text + "," + Y2.Text + "->" + X3.Text + "," + Y3.Text);
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnFillTriangle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.FillTriangleAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), short.Parse(this.X2.Text), short.Parse(this.Y2.Text), short.Parse(this.X3.Text), short.Parse(this.Y3.Text));
                SetStatusText("Filled triangle: " + X1.Text + "," + Y1.Text + "->" + X2.Text + "," + Y2.Text + "->" + X3.Text + "," + Y3.Text);
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnDrawCircle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.DrawCircleAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), short.Parse(this.R.Text));
                SetStatusText("Circle at: " + X1.Text + "," + Y1.Text + " with R->" + R.Text);
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnFillCircle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.FillCircleAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), short.Parse(this.R.Text));
                SetStatusText("Filled circle at: " + X1.Text + "," + Y1.Text + " with R->" + R.Text);
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnWriteText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.WriteTextAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), this.StringOrImage.Text);
                SetStatusText("Write Text: " + X1.Text + "," + Y1.Text);
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }

        private async void btnShowImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await epaper.ShowImageAsync(short.Parse(this.X1.Text), short.Parse(this.Y1.Text), this.StringOrImage.Text);
                SetStatusText("Show Image: " + X1.Text + "," + Y1.Text);
            }
            catch (ePaperException ex)
            {
                SetStatusText("Error:" + ex.ePaperExceptionCode.ToString() + "   |  Return->" + ex.CommandResult);
            }
        }
    }
}
