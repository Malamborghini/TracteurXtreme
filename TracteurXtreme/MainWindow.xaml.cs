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
            if (e.Key == Key.Left) { gauche = true; } // flèche gauche pressée
            if (e.Key == Key.Right) { droite = true; } // flèche droite pressée
            if (e.Key == Key.Up) { haut = true; } // flèche haut pressée
            if (e.Key == Key.Down) { bas = true; } // flèche bas pressée
            DeplacerTracteur(); // appel de la méthode pour les déplacements            
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) { gauche = false; } // flèche gauche relâchée 
            if (e.Key == Key.Right) { droite = false; } // flèche droite relâchée 
            if (e.Key == Key.Up) { haut = false; } // flèche haut relâchée 
            if (e.Key == Key.Down) { bas = false; } // flèche bas relâchée           
        }
        private void DeplacerTracteur()
        {
            double posX = Canvas.GetLeft(rectTracteur);
            double posY = Canvas.GetTop(rectTracteur);

            if (gauche && !droite) { posX -= VITESSE_TRACTEUR; } // si gauche pressée et non droite, déplacement à gauche
            else if (!gauche && droite) { posX += VITESSE_TRACTEUR; } // si droite pressée et non gauche, déplacement à droite

            if (haut && !bas) { posY -= VITESSE_TRACTEUR; } // si haut pressée et non bas, déplacement en haut
            else if (!haut && bas) { posY += VITESSE_TRACTEUR; } // si bas pressée et non haut, déplacement en bas

            // met à jour les positions X et Y
            if (posX >= 0 && posX <= canvasPiste.ActualWidth - rectTracteur.Width) // vérifie si X n'est pas hors largeur du canvas
            { Canvas.SetLeft(rectTracteur, posX); } 
            if (posY >= 0 && posY <= canvasPiste.ActualHeight - rectTracteur.Width) // vérifie si Y n'est pas hors hauteur du canvas
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