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
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Owner.Show();
            this.Hide();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double xDeplacement = Canvas.GetLeft(rectTracteur);
            double yDeplacement = Canvas.GetTop(rectTracteur);
            switch (e.Key)
            {
                case Key.Up:
                    yDeplacement = yDeplacement - VITESSE_TRACTEUR;
                    break;
                case Key.Left:
                    xDeplacement = xDeplacement - VITESSE_TRACTEUR;
                    break;
                case Key.Right:
                    xDeplacement = xDeplacement + VITESSE_TRACTEUR;
                    break;
                case Key.Down:
                    yDeplacement = yDeplacement + VITESSE_TRACTEUR;
                    break;
            }
            if (yDeplacement >= 0 && yDeplacement <= canvasPiste.ActualHeight - rectTracteur.Width)
            {
                Canvas.SetTop(rectTracteur, yDeplacement);
            }
            if (xDeplacement >= 0 && xDeplacement <= canvasPiste.ActualWidth - rectTracteur.Width)
            {
                Canvas.SetLeft(rectTracteur, xDeplacement);
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