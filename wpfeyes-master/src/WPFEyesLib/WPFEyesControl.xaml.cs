using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace WPFEyesLib
{
    /// <summary>
    /// Interaction logic for WPFEyesControl.xaml
    /// </summary>
    public partial class WPFEyesControl : UserControl
    {
        public double RatioInnerOut
        {
            get { return (double)GetValue(RatioInnerOutProperty); }
            set { SetValue(RatioInnerOutProperty, value); }
        }       

        public double DistanceBetween
        {
            get { return (double)GetValue(DistanceBetweenProperty); }
            set { SetValue(DistanceBetweenProperty, value); }
        }



        public Brush FillInner
        {
            get { return (Brush)GetValue(FillInnerProperty); }
            set { SetValue(FillInnerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillInner.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillInnerProperty =
            DependencyProperty.Register("FillInner", typeof(Brush), typeof(WPFEyesControl), new PropertyMetadata(Brushes.Black));



        public Brush FillOuter
        {
            get { return (Brush)GetValue(FillOuterProperty); }
            set { SetValue(FillOuterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillOuter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillOuterProperty =
            DependencyProperty.Register("FillOuter", typeof(Brush), typeof(WPFEyesControl), new PropertyMetadata(Brushes.White));

        // Using a DependencyProperty as the backing store for DistanceBetween.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DistanceBetweenProperty =
            DependencyProperty.Register("DistanceBetween", typeof(double), typeof(WPFEyesControl), new PropertyMetadata(0.3));

        // Using a DependencyProperty as the backing store for RatioInnerOut.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RatioInnerOutProperty =
            DependencyProperty.Register("RatioInnerOut", typeof(double), typeof(WPFEyesControl), new PropertyMetadata(0.5));
                
        public WPFEyesControl()
        {
            InitializeComponent();
        }

        public void RefreshContent()
        {
            var positionMouse = MouseUtilities.CorrectGetPosition(this);
            var totalWidth = this.ActualWidth;
            var totalHeight = this.ActualHeight;

            var distanceBetween = this.DistanceBetween * totalWidth;
            var perEyeWidth = (totalWidth - distanceBetween) / 2;

            var sizeOuterEye = Math.Min(totalHeight, perEyeWidth);
            var sizeInnerEye = sizeOuterEye * this.RatioInnerOut;

            var verticalLine = totalHeight / 2;
            var centralLeft = sizeOuterEye / 2;
            var centralRight = totalWidth - sizeOuterEye / 2;

            // Set size
            this.InnerLeft.Height = sizeInnerEye;
            this.InnerLeft.Width = sizeInnerEye;
            this.InnerRight.Height = sizeInnerEye;
            this.InnerRight.Width = sizeInnerEye;

            this.OuterLeft.Height = sizeOuterEye;
            this.OuterLeft.Width = sizeOuterEye;
            this.OuterRight.Height = sizeOuterEye;
            this.OuterRight.Width = sizeOuterEye;

            this.InnerLeft.Fill = this.FillInner;
            this.InnerRight.Fill = this.FillInner;
            this.OuterLeft.Fill = this.FillOuter;
            this.OuterRight.Fill = this.FillOuter;


            // Calculate the offset of the mouse cursor
            var leftdX = - centralLeft + positionMouse.X;
            var rightdX = - centralRight + positionMouse.X;
            var leftdY = -verticalLine + positionMouse.Y;
            var rightdY = -verticalLine + positionMouse.Y;
            var totalDistance = (sizeOuterEye - sizeInnerEye) / 2;

            var distanceLeft = Math.Sqrt(leftdX * leftdX + leftdY * leftdY);
            var distanceRight = Math.Sqrt(rightdX * rightdX + rightdY * rightdY);

            leftdX = distanceLeft > totalDistance ? leftdX / distanceLeft * totalDistance : leftdX;
            leftdY = distanceLeft > totalDistance ? leftdY / distanceLeft * totalDistance : leftdY;
            rightdX = distanceRight > totalDistance ? rightdX / distanceRight * totalDistance : rightdX;
            rightdY = distanceRight > totalDistance ? rightdY / distanceRight * totalDistance : rightdY;            

            // Position
            Canvas.SetLeft(this.OuterLeft, 0);
            Canvas.SetTop(this.OuterLeft, verticalLine - sizeOuterEye / 2);
            Canvas.SetLeft(this.OuterRight, totalWidth - sizeOuterEye);
            Canvas.SetTop(this.OuterRight, verticalLine - sizeOuterEye / 2);

            Canvas.SetLeft(this.InnerLeft, (sizeOuterEye - sizeInnerEye) / 2 + leftdX);
            Canvas.SetTop(this.InnerLeft, verticalLine - sizeInnerEye / 2 + leftdY);
            Canvas.SetLeft(this.InnerRight, totalWidth - (sizeOuterEye + sizeInnerEye) / 2 + rightdX);
            Canvas.SetTop(this.InnerRight, verticalLine - sizeInnerEye / 2 + rightdY);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer(
                TimeSpan.FromSeconds(1.0 / 60),
                DispatcherPriority.SystemIdle,
                new EventHandler((x, y) => this.RefreshContent()),
                this.Dispatcher);
            timer.Start();
        }
    }
}
