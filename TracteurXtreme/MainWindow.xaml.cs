using System.Diagnostics;
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
using System.Windows.Threading;

namespace TracteurXtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int vitesseTracteur = 5;
        public static bool gauche, droite, haut, bas;
        private static BitmapImage tracteurGauche, tracteurDroite, tracteurBas, tracteurHaut;
        public Rect tracteurHitbox;
        public Rect murHitbox;
        public MenuPrincipal menuPrincipal;
        public DispatcherTimer minuterie;
        public Stopwatch chronometre;
        public TimeSpan tempsEcoule;
        public bool uneSeulefois = true;
        public MainWindow()
        {
            InitializeComponent();
            InitBitmap();
            menuPrincipal = new MenuPrincipal();
            menuPrincipal.ShowDialog();
            InitTimer();
            Canvas.SetLeft(labChrono, canvasPiste.ActualHeight);
            Canvas.SetTop(labChrono, 0);

        }
        private void InitTimer()
        {
            chronometre = Stopwatch.StartNew(); // mesurer le temps écoulé
            minuterie = new DispatcherTimer();
            minuterie.Interval = TimeSpan.FromMilliseconds(16);
            minuterie.Tick += Jeu;
            minuterie.Start();
        }
        private void AfficherChrono()
        {
            tempsEcoule = chronometre.Elapsed; // récupère temps écoulé
            labChrono.Content = tempsEcoule.ToString(@"mm\:ss\.fff"); // format chrono (minutes : secondes : millisecondes)
        }
        private void InitBitmap()
        {
            tracteurGauche = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/gauche/tracteurNoirJoueur_gauche.png"));
            tracteurDroite = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/droite/tracteurNoirJoueur_droite.png"));
            tracteurHaut = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/haut/tracteurNoirJoueur_haut.png"));
            tracteurBas = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/bas/tracteurNoirJoueur_bas.png"));
        }
        private void Jeu(object? sender, EventArgs e)
        {
            AfficherChrono();
            double posX = (canvasPiste.ActualWidth - rectTracteurRouge.Width) - (canvasPiste.ActualWidth / 1.1);
            double posY = (canvasPiste.ActualHeight - rectTracteurRouge.Height) - (canvasPiste.ActualHeight / 2);
            Canvas.SetLeft(rectTracteurRouge, posX);
            Canvas.SetTop(rectTracteurRouge, posY);
            DeplacerTracteur(); // appel de la méthode pour les déplacements
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            menuPrincipal.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) // flèche gauche pressée
            { 
                gauche = true;
                if (imgTracteur.ImageSource != tracteurGauche) // changement image gauche avec vérification
                { imgTracteur.ImageSource = tracteurGauche; }
            } 
            if (e.Key == Key.Right) // flèche droite pressée
            { 
                droite = true;
                if (imgTracteur.ImageSource != tracteurDroite) // changement image droite avec vérification
                { imgTracteur.ImageSource = tracteurDroite; }
            } 
            if (e.Key == Key.Up) // flèche haut pressée
            { 
                haut = true;
                if (imgTracteur.ImageSource != tracteurHaut) // changement image haut avec vérification
                { imgTracteur.ImageSource = tracteurHaut; }
            } 
            if (e.Key == Key.Down) // flèche bas pressée
            { 
                bas = true;
                if (imgTracteur.ImageSource != tracteurBas) // changement image bas avec vérification
                { imgTracteur.ImageSource = tracteurBas; }
            } 

            // mettre en pause
            if (e.Key == Key.Space)
            {
                if (minuterie.IsEnabled) { minuterie.Stop(); }
                else { minuterie.Start(); }
            }
            if (!minuterie.IsEnabled)
            {
                labPauseJeu.Content = "Pause";
            }
            else { labPauseJeu.Content = ""; }
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
            if (uneSeulefois)
            {
                double posInitX = (canvasPiste.ActualWidth - rectTracteur.Width) - (canvasPiste.ActualWidth / 1.17);
                double posInitY = (canvasPiste.ActualHeight - rectTracteur.Height) - (canvasPiste.ActualHeight / 2);
                Canvas.SetLeft(rectTracteur, posInitX);
                Canvas.SetTop(rectTracteur, posInitY);
            }

            double posX = Canvas.GetLeft(rectTracteur);
            double posY = Canvas.GetTop(rectTracteur);

            if (gauche && !droite) { posX -= vitesseTracteur; } // si gauche pressée et non droite, déplacement à gauche
            else if (!gauche && droite) { posX += vitesseTracteur; } // si droite pressée et non gauche, déplacement à droite

            if (haut && !bas) { posY -= vitesseTracteur; } // si haut pressée et non bas, déplacement en haut
            else if (!haut && bas) { posY += vitesseTracteur; } // si bas pressée et non haut, déplacement en bas

            // met à jour les positions X et Y
            if (minuterie.IsEnabled)
            {
                // vérifie si X n'est pas hors largeur du canvas
                if (posX >= 0 && posX <= canvasPiste.ActualWidth - rectTracteur.Width)
                {
                    Canvas.SetLeft(rectTracteur, posX);
                    uneSeulefois = false;
                }
                // vérifie si Y n'est pas hors hauteur du canvas
                if (posY >= 0 && posY <= canvasPiste.ActualHeight - rectTracteur.Width)
                {
                    Canvas.SetTop(rectTracteur, posY);
                    uneSeulefois = false;
                }
            }
                
        }

        private void Collision()
        {
            //tracteurHitbox = new Rect(Canvas.GetLeft(rectTracteur), Canvas.GetTop(rectTracteur), rectTracteur.Width, rectTracteur.Height);
            //murHitbox = new Rect(Canvas.GetLeft(murTest), Canvas.GetTop(murTest), murTest.Width, murTest.Height);
            //if (tracteurHitbox.IntersectsWith(murHitbox))
            //{
            //    //vaDroite = false;
            //}
        }
    }
}