using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TracteurXtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly int VITESSE_TRACTEUR = 5;
        public Rect tracteurHitbox;
        public Rect murHitbox;
        public MenuPrincipal menuPrincipal;
        public MainWindow()
        {
            InitializeComponent();

            menuPrincipal = new MenuPrincipal();
            menuPrincipal.ShowDialog();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            menuPrincipal.ShowDialog();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double xDeplacement = Canvas.GetLeft(rectTracteur);
            double yDeplacement = Canvas.GetTop(rectTracteur);
            
            if (e.Key == Key.Left)
            {
                xDeplacement = xDeplacement - VITESSE_TRACTEUR;
            }
            else if (e.Key == Key.Right)
            {
                xDeplacement = xDeplacement + VITESSE_TRACTEUR;
            }
            if (xDeplacement >= 0 && xDeplacement <= canvasPiste.ActualWidth - rectTracteur.Width)
            {
                Canvas.SetLeft(rectTracteur, xDeplacement);
            }

            if (e.Key == Key.Down)
            {
                yDeplacement = yDeplacement + VITESSE_TRACTEUR;
            }
            if (e.Key == Key.Up)
            {
                yDeplacement = yDeplacement - VITESSE_TRACTEUR;
            }
            if (yDeplacement >= 0 && yDeplacement <= canvasPiste.ActualHeight - rectTracteur.Width)
            {
                Canvas.SetTop(rectTracteur, yDeplacement);
            }
            
        }
        private void Collision()
        {
            tracteurHitbox = new Rect(Canvas.GetLeft(rectTracteur), Canvas.GetTop(rectTracteur), rectTracteur.Width, rectTracteur.Height);
            murHitbox = new Rect(Canvas.GetLeft(murTest), Canvas.GetTop(murTest), murTest.Width, murTest.Height);
            if (tracteurHitbox.IntersectsWith(murHitbox))
            {
                //vaDroite = false;
            }
        }
    }
}