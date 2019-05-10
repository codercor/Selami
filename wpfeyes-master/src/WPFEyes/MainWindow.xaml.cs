using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WPFEyes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Stores the last cursor position for scrolling
        /// </summary>
        private Point lastCursorPosition;

        private bool isPressed;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.lastCursorPosition = this.PointToScreen(e.GetPosition(this));
            this.CaptureMouse();
            Debug.WriteLine("Down: " + this.lastCursorPosition.X.ToString() + ", " + this.lastCursorPosition.Y.ToString());
            this.isPressed = true;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Up");
            this.isPressed = false;
            this.ReleaseMouseCapture();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isPressed)
            {
                var nextPosition = this.PointToScreen(e.GetPosition(this));
                var dX = nextPosition.X - this.lastCursorPosition.X;
                var dY = nextPosition.Y - this.lastCursorPosition.Y;

                Debug.WriteLine("X: " + dX.ToString() + ", Y:" + dY.ToString());

                this.Left += dX;
                this.Top += dY;
                this.lastCursorPosition = nextPosition;
            }
        }

        private void Small_Click(object sender, RoutedEventArgs e)
        {
            this.Width = 70;
            this.Height = 40;

        }

        private void Medium_Click(object sender, RoutedEventArgs e)
        {
            this.Width = 150;
            this.Height = 100;

        }

        private void Large_Click(object sender, RoutedEventArgs e)
        {
            this.Width = 500;
            this.Height= 200;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
