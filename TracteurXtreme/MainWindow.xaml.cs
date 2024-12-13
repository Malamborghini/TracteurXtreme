using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
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
        public double tracteurXpixel;
        public double tracteurYpixel;
        static int[,] tabCircuit;
        public MainWindow()
        {
            InitializeComponent();
            InitBitmap();
            menuPrincipal = new MenuPrincipal();
            menuPrincipal.ShowDialog();
            InitTimer();
            Canvas.SetLeft(labChrono, canvasPiste.ActualHeight);
            Canvas.SetTop(labChrono, 0);

            string chemin = "P:\\SAE_1.01\\racing\\TracteurXtreme\\TracteurXtreme\\img\\tabPistes\\piste1.txt"; // Chemin du fichier binaire du tracé du circuit
            double taileLargeurCanvas = canvasPiste.ActualHeight; // Hauteur du Canvas
            double tailleHauteurCanvas = canvasPiste.ActualWidth; // Largeur du Canvas
            ChargementTableau(chemin); // Appel de la méthode pour chargé le talbeau en 2D
            EstSurLeCircuit(tabCircuit, tailleHauteurCanvas, taileLargeurCanvas, tracteurXpixel, tracteurYpixel); // Appel méthode parce que j'ai eu un message qui m'a fait peur quand je l'ai enlevé 
            this.Loaded += (s, e) => InitialiserCanvas(); //quand le main aura fini de charger execute la méthode InitialiserCanvas (s = sender, e = args)
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
            if (e.Key == Key.Left) { gauche = true; }  // flèche gauche pressée
            if (e.Key == Key.Right) { droite = true; }// flèche droite pressée

            if (e.Key == Key.Up) { haut = true; } // flèche haut pressée
            if (e.Key == Key.Down) { bas = true; } // flèche bas pressée

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

            if (gauche && !droite) 
            { 
                posX -= vitesseTracteur;
                if (imgTracteur.ImageSource != tracteurGauche) // changement image gauche avec vérification
                { imgTracteur.ImageSource = tracteurGauche; } // si gauche pressée et non droite, déplacement à gauche
            } 
            else if (!gauche && droite) 
            {
                posX += vitesseTracteur;
                if (imgTracteur.ImageSource != tracteurDroite) // changement image droite avec vérification
                { imgTracteur.ImageSource = tracteurDroite; } // si droite pressée et non gauche, déplacement à droite
            } 

            if (haut && !bas) 
            {
                posY -= vitesseTracteur;
                if (imgTracteur.ImageSource != tracteurHaut) // changement image haut avec vérification
                { imgTracteur.ImageSource = tracteurHaut; } // si haut pressée et non bas, déplacement en haut
            } 
            else if (!haut && bas) 
            {
                posY += vitesseTracteur;
                if (imgTracteur.ImageSource != tracteurBas) // changement image bas avec vérification
                { imgTracteur.ImageSource = tracteurBas; } // si bas pressée et non haut, déplacement en bas
            } 

            // mettre à jour les positions X et Y
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
            tracteurXpixel = Canvas.GetLeft(rectTracteur);
            tracteurYpixel = Canvas.GetTop(rectTracteur);
            EstSurLeCircuit(tabCircuit, canvasPiste.ActualWidth, canvasPiste.ActualHeight, tracteurXpixel, tracteurYpixel);
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

        // Transforme la fichier en tableau 2D
        public static void ChargementTableau(string chemin)
        {
            string[] ligne = File.ReadAllLines(chemin); // Lis chaque ligne du fichier
            int colone = ligne.Length;  // Détermine le nombre de colonnes
            int lignes = ligne[0].Length; // Détermine le nombre de lignes

            tabCircuit = new int[colone, lignes]; // Initialise un tableau 2D avec le bon nb de lignes et de colonnes

            for (int i = 0; i < colone; i++)
            {
                for (int j = 0; j < lignes; j++)
                {
                    tabCircuit[i, j] = ligne[i][j] - '0';
                }
            }
        }

        // Verifie si le tracteur se trouve sur le circuit
        private static bool EstSurLeCircuit(int[,] tabCircuit, double taileLargeurCanvas, double tailleHauteurCanvas, double tracteurXPixel, double tracteurYPixel)
        {
            if (taileLargeurCanvas == 0 || tailleHauteurCanvas == 0) // Vérifie si le canvas n'est pas initialisé
            {
                Console.WriteLine("Erreur : Le canvas n'a pas encore été initialisé correctement.");
                return false;
            }

            int nbColonne = tabCircuit.GetLength(1); // Détermine le nb de colonnes dans le tableau 
            int nbLignes = tabCircuit.GetLength(0);  // Détermine le nb de lignes dans le tableau

            double tailleLargeurPixel = taileLargeurCanvas / nbColonne; // Calcule la largeur d'une case en pixel
            double tailleHauteurPixel = tailleHauteurCanvas / nbLignes; // Idem pour la hauteur

            int indiceColonne = (int)(tracteurXPixel / tailleLargeurPixel); // Calcule l'indice de colonne pour la position X
            int indiceLigne = (int)(tracteurYPixel / tailleHauteurPixel); // Calcule l'indice de ligne pour la position Y

            if (indiceColonne < 0 || indiceColonne >= nbColonne || indiceLigne < 0 || indiceLigne >= nbLignes) // Vérifie si les indices sont en dehords des limites du tableau 
            {
                Console.WriteLine("Erreur : Le tracteur est en dehors des limites du tableau.");
                return false;
            }

            if (MainWindow.tabCircuit[indiceLigne, indiceColonne] == 1) // Vérifie si la case du talbeau de la position du tracteur est un 1
            {
                Console.WriteLine("Vous êtes sur le circuit");
                return true;

            }
            else
            {
                Console.WriteLine("pas sur le circuit");
                return false;
            }
        }

        private void InitialiserCanvas()
        {
            double taileLargeurCanvas = canvasPiste.ActualHeight; // Récupère la hauteur du Canvas
            double tailleHauteurCanvas = canvasPiste.ActualWidth; // Idem pour la largeur 

            if (taileLargeurCanvas == 0 || tailleHauteurCanvas == 0) // Vérifie si le canvas est initialisé
            {
                Console.WriteLine("Erreur : Le canvas n'a pas encore été initialisé correctement.");
                return;
            }
        }
    }
}