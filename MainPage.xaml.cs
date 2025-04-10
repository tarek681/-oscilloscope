using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace Alktifan_Tarek
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Oscilloscope oscilloscope;
        double effictive;
        double avarage;
        double Min;
        double Max;

        double TriggerLevelValue;

        public MainPage()
        {
            this.InitializeComponent();
            oscilloscope=new Oscilloscope();

            TriggerLevelValue = 50;
            oscilloscope.newData += newData_event_handler;
            oscilloscope.newData += newData_event_handler1;

            

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double[] werte = new double[960];
            //werte = 

        }

        private void newData_event_handler(double[] Values)
        {
            PointCollection points = new PointCollection();
            for (int i = 0; i < Values.Length; i++)
            {
                points.Add(new Point(i, (250 - Values[i] * 500 / 3.3)));// - (TriggerLevelValue-50)*5));
            }

            Poly.Points = points;
            
        }

        private void newData_event_handler1(double[] Values)
        {
            effictive = Math.Sqrt(Values.Select(V => V * V).Average());
            EffectiveValue.Text = "EffectiveValue = " + effictive.ToString();

            Max = Values.Select(V => V).Max();
            MaxValue.Text = "MaxValue = " + Max.ToString();

            avarage = Math.Sqrt(Values.Select(V => V).Average());
            AverageValue.Text = "AverageValue = " + avarage.ToString();

            Min = Values.Select(V => V).Min();
            MinValue.Text = "MinValue = " + Min.ToString();

        }

        

        private void Channel_A_Click(object sender, RoutedEventArgs e)
        {
            if (Channel_A.IsChecked == true)
                oscilloscope.Channel_A = true;
            else
                oscilloscope.Channel_A = false;
        }

        private void Channel_B_Click(object sender, RoutedEventArgs e)
        {
            if (Channel_B.IsChecked == true)
                oscilloscope.Channel_B = true;
            else
                oscilloscope.Channel_B = false;

        }

        private void TriggerOn_Click(object sender, RoutedEventArgs e)
        {
            if (TriggerOn.IsChecked == true)
                oscilloscope.TrigerOn = true;
            else
                oscilloscope.TrigerOn = false;
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if(oscilloscope != null)
            oscilloscope.TriggerLevel = (int)TriggerLevelSlider.Value;
            else
                System.Diagnostics.Debug.WriteLine("Null");
        }
    }
}
