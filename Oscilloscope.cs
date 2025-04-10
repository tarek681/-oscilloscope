using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.UI.Xaml;

namespace Alktifan_Tarek
{
    
    internal class Oscilloscope
    {
        public delegate void RceivedValueBuf(double [] Values);
        public event RceivedValueBuf newData;

        GpioPin pinLED;
        GpioPin pinADC;
        GpioPin pinDAC;

        GpioController gpioctrl;

        SpiDevice LED;
        SpiDevice ADC;
        SpiDevice DAC;
        Task ta;

        bool trigerOn;
        int myTriggerLevel;
        double triggerLevel=0;
        bool channel_A;
        bool channel_B;
        int StartTrigger=0;

        byte [] SendBuf_A= new byte[4*960];
        byte[] SendBuf_B = new byte[4 * 960];
        byte[] RcvBuf = new byte[4*960];
        int RecievedValue;
        double RecievedValueD;
        double[] RcvValueBuf = new double[2*960];

        /*byte[] SendBuf_Trigger = new byte[4 * 960];
        byte[] RcvBuf_Trigger = new byte[4 * 960];*/
        double[] RcvValueBufTrigger = new double[960];

        DispatcherTimer timer=new DispatcherTimer();

        public bool TrigerOn { get => trigerOn; set => trigerOn = value; }
        
        public bool Channel_A { get => channel_A; set => channel_A = value; }
        public bool Channel_B { get => channel_B; set => channel_B = value; }
        public double TriggerLevel { get => triggerLevel; set => triggerLevel = value; }
        public int MyTriggerLevel { get => myTriggerLevel; set => myTriggerLevel = value; }

        public Oscilloscope()
        {
            Channel_A = true;
            

            for (int i = 0; i < SendBuf_A.Length / 2; i++)
            {
                SendBuf_B[i * 2] = 0X08;
            }

            InitGpio();
            ta=InitSPI();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();
            
            

        }

        private void Timer_Tick(object sender, object e)
        {
            StartTrigger = 0;
            if (!ta.IsCompleted) return;

            if (Channel_A)
            {
                pinADC.Write(GpioPinValue.Low);
                ADC.TransferFullDuplex(SendBuf_A, RcvBuf);
                pinADC.Write(GpioPinValue.High);


                for (int i = 0; i < RcvValueBuf.Length; i++)
                {

                    RecievedValue = 0;
                    RecievedValue += RcvBuf[1 + 2 * i] >> 2;
                    RecievedValue += RcvBuf[2 * i] << 6;
                    RecievedValueD = (RecievedValue - 516.0) * 3.3 / (765 - 267);
                    RcvValueBuf[i] = RecievedValueD;
                    

                }

                
            }
            else if(Channel_B)
            {
                
                
                pinADC.Write(GpioPinValue.Low);
                ADC.TransferFullDuplex(SendBuf_B, RcvBuf);
                pinADC.Write(GpioPinValue.High);


                for (int i = 0; i < RcvValueBuf.Length; i++)
                {

                    RecievedValue = 0;
                    RecievedValue += RcvBuf[1 + 2 * i] >> 2;
                    RecievedValue += RcvBuf[2 * i] << 6;
                    RecievedValueD = (RecievedValue - 516.0) * 3.3 / (765 - 267);
                    RcvValueBuf[i] = RecievedValueD;

                }
                //newData(RcvValueBuf);

            }
            if (TrigerOn)
            {
                for (int i = 0; i < RcvValueBuf.Length/2; i++)
                {
                    if ((RcvValueBuf[i] < TriggerLevel) && (RcvValueBuf[i + 2] >= TriggerLevel))
                    {
                        StartTrigger = i + 1;
                        System.Diagnostics.Debug.WriteLine(StartTrigger.ToString());
                        break;
                    }


                }

            }
            for (int i = 0; i < RcvValueBufTrigger.Length; i++)
            {
                RcvValueBufTrigger[i] = RcvValueBuf[i + StartTrigger];
            }
            newData(RcvValueBufTrigger);
            StartTrigger = 0;
            //System.Diagnostics.Debug.WriteLine("B {0} A {1}",Channel_B.ToString(), TrigerOn.ToString());
        }

        private async Task InitSPI()
        {
            String SpiDeviceSelector = SpiDevice.GetDeviceSelector();
            IReadOnlyList<DeviceInformation> devices =
                await DeviceInformation.FindAllAsync(SpiDeviceSelector);

            var Settings = new SpiConnectionSettings(0);
            Settings.ClockFrequency = 4800000;
            Settings.Mode = SpiMode.Mode3;
            //LED = await SpiDevice.FromIdAsync(devices[0].Id, Settings);
            ADC = await SpiDevice.FromIdAsync(devices[0].Id, Settings);
            //DAC = await SpiDevice.FromIdAsync(devices[2].Id, Settings);
        }

        private void InitGpio()
        {
            gpioctrl = GpioController.GetDefault();

            pinLED = gpioctrl.OpenPin(28);
            pinLED.SetDriveMode(GpioPinDriveMode.Output);
            pinLED.Write(GpioPinValue.High);

            pinADC = gpioctrl.OpenPin(35);
            pinADC.SetDriveMode(GpioPinDriveMode.Output);
            pinADC.Write(GpioPinValue.High);

            pinDAC = gpioctrl.OpenPin(12);
            pinDAC.SetDriveMode(GpioPinDriveMode.Output);
            pinDAC.Write(GpioPinValue.High);
        }

        
    }

    
}
