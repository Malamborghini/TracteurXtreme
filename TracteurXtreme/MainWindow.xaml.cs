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
        public static bool gauche, droite, haut, bas;
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
            if (e.Key == Key.Left) { gauche = true; }
            if (e.Key == Key.Right) { droite = true; }
            if (e.Key == Key.Up) { haut = true; }
            if (e.Key == Key.Down) { bas = true; }
            DeplacerTracteur();
            
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) { gauche = false; }
            if (e.Key == Key.Right) { droite = false; }
            if (e.Key == Key.Up) { haut = false; }
            if (e.Key == Key.Down) { bas = false; }           
        }
        private void DeplacerTracteur()
        {
            double posX = Canvas.GetLeft(rectTracteur);
            double posY = Canvas.GetTop(rectTracteur);

            if (gauche && !droite) { posX -= VITESSE_TRACTEUR; }
            else if (!gauche && droite) { posX += VITESSE_TRACTEUR; }
            if (haut && !bas) { posY -= VITESSE_TRACTEUR; }
            else if (!haut && bas) { posY += VITESSE_TRACTEUR; }
            //Canvas.SetLeft(rectTracteur, posX);
            //Canvas.SetTop(rectTracteur, posY);

            if (posX >= 0 && posX <= canvasPiste.ActualWidth - rectTracteur.Width)
            { Canvas.SetLeft(rectTracteur, posX); }
            if (posY >= 0 && posY <= canvasPiste.ActualHeight - rectTracteur.Width)
            { Canvas.SetTop(rectTracteur, posY); }

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