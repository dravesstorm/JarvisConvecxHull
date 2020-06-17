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
using Microsoft.Win32;
using System.IO;

namespace Jarvis
{
    public partial class MainWindow : Window
    {
        AlgorithMCH SpacePoint;
        private Ellipse[] myDots;
        int MCHcount;

        public MainWindow()
        {
            InitializeComponent();
            Tb_DotsNumber.MaxLength = 3;
        }

        private void Btn_PaintDots_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.Parse(Tb_DotsNumber.Text) < 3 || Int32.Parse(Tb_DotsNumber.Text) > 999)
            {
                MessageBox.Show(messageBoxText: "Можно построить случайно 3-999 точек", caption: "Error", button: MessageBoxButton.OK, icon: MessageBoxImage.Hand);
                return;
            }
            SpacePoint = new AlgorithMCH(Int32.Parse(Tb_DotsNumber.Text));
            myDots = new Ellipse[SpacePoint.n];
            Random rand = new Random();

            for (int i = 0; i < SpacePoint.n; i++)
            {
                bool tooClose = false; //флаг, показывает если центры точек слишком близко
                do
                {
                    int x = rand.Next(-450, 450);
                    int y = rand.Next(-450, 450);
                    SpacePoint.CoordOfDots[i, 0] = x;
                    SpacePoint.CoordOfDots[i, 1] = y;
                    tooClose = false;
                    if (i == 0) break;
                    for (int j = 0; j < i; j++)
                    {//сравниваем все точки с новой, если расстояне <60 - tooClose
                        if (SpacePoint.VectorModule(SpacePoint.CoordOfDots[j, 0] - SpacePoint.CoordOfDots[i, 0], SpacePoint.CoordOfDots[j, 1] - SpacePoint.CoordOfDots[i, 1]) < 15)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                } while (tooClose);
                myDots[i] = new Ellipse
                {
                    Margin = new Thickness(SpacePoint.CoordOfDots[i, 0], 0, 0, SpacePoint.CoordOfDots[i, 1]),
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Purple,
                    Stroke = Brushes.Red,
                };
                WorkGrid.Children.Add(myDots[i]);
            }
            Btn_PaintDots.IsEnabled = false;
            Tb_DotsNumber.IsEnabled = false;
            Btn_MCHBuild.IsEnabled = true;
            rb_FileDots.IsEnabled = false;
        }

          private void Btn_DotsFromFile_Click(object sender, RoutedEventArgs e)
        {
            StreamReader fileIn = new StreamReader(Tb_FileName.Text);
            int CountityOfDots = int.Parse(fileIn.ReadLine());
            if (CountityOfDots < 3 || CountityOfDots > 999)
            {
                MessageBox.Show(messageBoxText: "Можно построить случайно 3-999 точек", caption: "Error", button: MessageBoxButton.OK, icon: MessageBoxImage.Hand);
                return;
            }
            string str = "";
            str = fileIn.ReadToEnd();
            fileIn.Close();
            string[] strDots = str.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            SpacePoint = new AlgorithMCH(CountityOfDots);
            myDots = new Ellipse[SpacePoint.n];

            for (int i = 0; i < SpacePoint.n; i++)
            {
                string[] OrdAbs = strDots[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                SpacePoint.CoordOfDots[i, 0] = int.Parse(OrdAbs[0]);
                SpacePoint.CoordOfDots[i, 1] = int.Parse(OrdAbs[1]);

                myDots[i] = new Ellipse
                {
                    Margin = new Thickness(SpacePoint.CoordOfDots[i, 0], 0, 0, SpacePoint.CoordOfDots[i, 1]),
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Purple,
                    Stroke = Brushes.Red,
                };
                WorkGrid.Children.Add(myDots[i]);
            }
            Btn_PaintDots.IsEnabled = false;
            Btn_DotsFromFile.IsEnabled = false;
            Tb_DotsNumber.IsEnabled = false;
            Btn_MCHBuild.IsEnabled = true;
            rb_RandomDots.IsEnabled = false;
        }

        private void Btn_MCHBuild_Click(object sender, RoutedEventArgs e)
        //построение оболочки
        {
            MCHcount = SpacePoint.JarvisMCH();
            Ellipse[] MCH = new Ellipse[MCHcount + 1];
            for (int i = 0; i < MCH.Length; i++)
            {
                MCH[i] = new Ellipse
                {
                    Margin = new Thickness(SpacePoint.CoordOfDots[i, 0], 0, 0, SpacePoint.CoordOfDots[i, 1]),
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.Azure,
                };
                WorkGrid.Children.Add(MCH[i]);
            }

            for (int i = 0; i < MCHcount; i++)
            {
                Line connection = new Line();
                connection.VerticalAlignment = MCH[i].VerticalAlignment;
                connection.HorizontalAlignment = MCH[i].HorizontalAlignment;
                connection.X1 = (MCH[i].Margin.Left + WorkGrid.Width) / 2;
                connection.Y1 = (-MCH[i].Margin.Bottom + WorkGrid.Height) / 2;
                connection.X2 = (MCH[i + 1].Margin.Left + WorkGrid.Width) / 2;
                connection.Y2 = (-MCH[i + 1].Margin.Bottom + WorkGrid.Height) / 2;
                connection.Stroke = Brushes.Red;
                connection.StrokeThickness = 0.5;
                WorkGrid.Children.Add(connection);
            }

            Tb_OutputFileName.IsEnabled = true;
            openFile.IsEnabled = true;
            Btn_WriteToFile.IsEnabled = true;
        }

        private void Tb_DotsNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        //проверка ввода, можно вводить только цифры
        {
            if (!(e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key == Key.Back || e.Key == Key.Delete))
            {
                e.Handled = true;
            }
        }

        private void rb_RandomDots_Checked(object sender, RoutedEventArgs e)
        //переключатель
        {
            Tb_DotsNumber.Visibility = Visibility.Visible;
            Btn_PaintDots.Visibility = Visibility.Visible;
            lbl_NumOfDots.Visibility = Visibility.Visible;
            Btn_DotsFromFile.Visibility = Visibility.Hidden;
            Tb_FileName.Visibility = Visibility.Hidden;
            openFile2.Visibility = Visibility.Hidden;
        }

        private void rb_FileDots_Checked(object sender, RoutedEventArgs e)
        //переключатель
        {
            Tb_DotsNumber.Visibility = Visibility.Hidden;
            Btn_PaintDots.Visibility = Visibility.Hidden;
            lbl_NumOfDots.Visibility = Visibility.Hidden;
            Btn_DotsFromFile.Visibility = Visibility.Visible;
            Tb_FileName.Visibility = Visibility.Visible;
            openFile2.Visibility = Visibility.Visible;
        }

      

        private void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            WorkGrid.Children.Clear();
            Btn_PaintDots.IsEnabled = true;
            Tb_DotsNumber.IsEnabled = true;
            rb_FileDots.IsEnabled = true;
            Btn_DotsFromFile.IsEnabled = true;
            rb_RandomDots.IsEnabled = true;
            Btn_MCHBuild.IsEnabled = false;
            Tb_OutputFileName.IsEnabled = false;
            Btn_WriteToFile.IsEnabled = false;
            openFile.IsEnabled = false;
        }

        private void Btn_Refresh_MouseEnter(object sender, MouseEventArgs e)
        {
            Btn_Refresh.RenderTransform = new RotateTransform(-90);
        }

        private void Btn_Refresh_MouseLeave(object sender, MouseEventArgs e)
        {
            Btn_Refresh.RenderTransform = new RotateTransform(0);
        }

        private void Btn_WriteToFile_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter WriteMCH = new StreamWriter(Tb_OutputFileName.Text);
            WriteMCH.WriteLine(MCHcount.ToString());
            for (int i = 0; i < MCHcount; i++)
            {
                string strDot = SpacePoint.CoordOfDots[i, 0].ToString() + '\t' + SpacePoint.CoordOfDots[i, 1].ToString();
                WriteMCH.WriteLine(strDot);
            }
            MessageBox.Show("Результат записан в файл");
            WriteMCH.Close();
        }

        private void Btn_WriteToFile_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageBrush ico = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"D:\2 path\курсач\Jarvis\Jarvis\WriteDownIcon.png"))
            };
            Btn_WriteToFile.Background = ico;
        }

        private void Btn_WriteToFile_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageBrush ico = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"D:\2 path\курсач\Jarvis\Jarvis\WriteDownIcon1.png"))
            };
            Btn_WriteToFile.Background = ico;
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Tb_OutputFileName.Text = openFileDialog.FileName;
            }
        }

        private void openFile2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Tb_FileName.Text = openFileDialog.FileName;
            }
        }
    }
}
