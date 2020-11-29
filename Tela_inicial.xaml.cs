using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Lógica interna para Inicial.xaml
    /// </summary>
    public partial class Inicial : Window
    {
        public Inicial()
        {
            InitializeComponent();
        }

 
 
        private void BCadastro_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new User();
                newWindow.Show();
        }

  
        private void BTratamento_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new Tratamento();
            newWindow.Show();
        }



        private void BAcompanhamento_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new Acompanhamento();
            newWindow.Show();

        }

        private void BExercicio_Click_2(object sender, RoutedEventArgs e)
        {
            var newWindow = new MainWindow();
            newWindow.Show();

        }
    }
}
