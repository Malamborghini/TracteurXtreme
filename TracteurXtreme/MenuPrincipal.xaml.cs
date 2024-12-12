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
using System.Windows.Shapes;

namespace TracteurXtreme
{
    /// <summary>
    /// Logique d'interaction pour MenuPrincipal.xaml
    /// </summary>
    public partial class MenuPrincipal : Window
    {
        public MenuPrincipal()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            ComboBoxItem selectionChoix = (ComboBoxItem)cbChoixNiveau.SelectedItem;

            if (selectionChoix == null)
            {
                //string content = selectionChoix.Content.ToString();
                //MessageBox.Show("Selected Item: " + content);
                MessageBox.Show("Il faut sélectionner un niveau pour jouer");
            }
            else
            {
                //this.DialogResult = true;
                this.Hide();
            }
        }
    }
}
