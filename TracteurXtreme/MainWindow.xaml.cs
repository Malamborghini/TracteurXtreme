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
using System.Windows.Media.Animation;
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
        public int vitesseTracteurJoueur = 5;
        public int vitesseTracteurAdversaire = 3; //plus c'est elevee plus c'est lent, plus c'est petit plus c'est rapide
        public static bool gauche, droite, haut, bas;
        private static BitmapImage tracteurGauche, tracteurDroite, tracteurBas, tracteurHaut;
        public Rect tracteurHitbox;
        public Rect adversaireHitbox;
        public MenuPrincipal menuPrincipal;
        public DispatcherTimer minuterie;
        public Stopwatch chronometre;
        public TimeSpan tempsEcoule;
        public bool uneSeulefois = true;
        public double tracteurXPixel;
        public double tracteurYPixel;
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

            string cheminTab = "E:\\wpfMethode\\wpfMethode\\ciruict1pisteGrandissimo.txt"; // Chemin du fichier binaire du tracé du circuit
            double taileLargeurCanvas = canvasPiste.ActualHeight; // Hauteur du Canvas
            double tailleHauteurCanvas = canvasPiste.ActualWidth; // Largeur du Canvas
            ChargementTableau(cheminTab); // Appel de la méthode pour chargé le talbeau en 2D
            EstSurLeCircuit(tabCircuit, tailleHauteurCanvas, taileLargeurCanvas, tracteurXPixel, tracteurYPixel); // Appel méthode parce que j'ai eu un message qui m'a fait peur quand je l'ai enlevé 
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
            InitPositionAdversaire();
            DeplacerJoueur();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            menuPrincipal.ShowDialog();
            //menuPrincipal.DialogResult = false;
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
        private void DeplacerJoueur()
        {
            //met le tracteur au bon endroit au demarrage
            
            if (uneSeulefois)
            {
                double posInitX = (canvasPiste.ActualWidth - rectTracteur.Width) - (canvasPiste.ActualWidth / 1.17);
                double posInitY = (canvasPiste.ActualHeight - rectTracteur.Height) - (canvasPiste.ActualHeight / 2);
                Canvas.SetLeft(rectTracteur, posInitX);
                Canvas.SetTop(rectTracteur, posInitY);
                rectTracteur.Height = canvasPiste.ActualHeight / 15;
                rectTracteur.Width = canvasPiste.ActualWidth / 35;
            }

            double posX = Canvas.GetLeft(rectTracteur);
            double posY = Canvas.GetTop(rectTracteur);

            if (gauche && !droite) 
            { 
                posX -= vitesseTracteurJoueur;
                if (imgTracteur.ImageSource != tracteurGauche) // changement image gauche avec vérification
                { imgTracteur.ImageSource = tracteurGauche; } // si gauche pressée et non droite, déplacement à gauche
                rectTracteur.Height = canvasPiste.ActualHeight / 18;
                rectTracteur.Width = canvasPiste.ActualWidth / 28;
            } 
            else if (!gauche && droite) 
            {
                posX += vitesseTracteurJoueur;
                if (imgTracteur.ImageSource != tracteurDroite) // changement image droite avec vérification
                { imgTracteur.ImageSource = tracteurDroite; } // si droite pressée et non gauche, déplacement à droite
                rectTracteur.Height = canvasPiste.ActualHeight / 18;
                rectTracteur.Width = canvasPiste.ActualWidth / 28;
            } 

            if (haut && !bas) 
            {
                posY -= vitesseTracteurJoueur;
                if (imgTracteur.ImageSource != tracteurHaut) // changement image haut avec vérification
                { imgTracteur.ImageSource = tracteurHaut; } // si haut pressée et non bas, déplacement en haut
                rectTracteur.Height = canvasPiste.ActualHeight / 15;
                rectTracteur.Width = canvasPiste.ActualWidth / 35;
            } 
            else if (!haut && bas) 
            {
                posY += vitesseTracteurJoueur;
                if (imgTracteur.ImageSource != tracteurBas) // changement image bas avec vérification
                { imgTracteur.ImageSource = tracteurBas; } // si bas pressée et non haut, déplacement en bas
                rectTracteur.Height = canvasPiste.ActualHeight / 15;
                rectTracteur.Width = canvasPiste.ActualWidth / 35;
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
            Collision();
            tracteurXPixel = Canvas.GetLeft(rectTracteur);
            tracteurYPixel = Canvas.GetTop(rectTracteur);
            EstSurLeCircuit(tabCircuit, canvasPiste.ActualWidth, canvasPiste.ActualHeight, tracteurXPixel, tracteurYPixel);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitPositionAdversaire();
            DeplacerTracteurAdversaire();
        }
        private void InitPositionAdversaire()
        {
            rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
            rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
            
            double posX = (canvasPiste.ActualWidth - rectTracteurRouge.Width) - (canvasPiste.ActualWidth / 1.1);
            double posY = (canvasPiste.ActualHeight - rectTracteurRouge.Height) - (canvasPiste.ActualHeight / 2);
            Canvas.SetLeft(rectTracteurRouge, posX);
            Canvas.SetTop(rectTracteurRouge, posY);
        }
        private void DeplacerTracteurAdversaire()
        {
            double pointDePiste_1 = (canvasPiste.ActualHeight /1.1) - (rectTracteurRouge.Height/1.1);
            double pointDePiste_2 = (canvasPiste.ActualWidth / 2) - (rectTracteurRouge.Width / 2);
            double pointDePiste_3 = (canvasPiste.ActualHeight / 2.9) - (rectTracteurRouge.Height / 2.9);
            double pointDePiste_4 = (canvasPiste.ActualWidth/1.35) - (rectTracteurRouge.Width/1.35);
            double pointDePiste_5 = (canvasPiste.ActualHeight / 1.9) - (rectTracteurRouge.Height / 1.9);
            double pointDePiste_6 = (canvasPiste.ActualWidth / 1.6) - (rectTracteurRouge.Width / 1.6);
            double pointDePiste_7 = (canvasPiste.ActualHeight / 1.35) - (rectTracteurRouge.Height / 1.35);

            DoubleAnimation[] animations = new DoubleAnimation[]
            {
                new DoubleAnimation
                {
                    From = Canvas.GetTop(rectTracteurRouge),
                    To = pointDePiste_1,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = Canvas.GetLeft(rectTracteurRouge),
                    To = pointDePiste_2,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_1,
                    To = pointDePiste_3,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_2,
                    To = pointDePiste_4,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_3,
                    To = pointDePiste_5,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_4,
                    To = pointDePiste_6,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_5,
                    To = pointDePiste_7,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                }
            };

            // Creer la Storyboard et lui ajouter des animations 
            Storyboard storyboard = new Storyboard();
            foreach (DoubleAnimation animation in animations)
            {
                storyboard.Children.Add(animation);
            }

            // mise en place des targets d'animation
            for (int i = 0; i < animations.Length; i ++)
            {
                if (i%2 == 0)
                {
                    Storyboard.SetTarget(animations[i], rectTracteurRouge);
                    Storyboard.SetTargetProperty(animations[i], new PropertyPath("(Canvas.Top)"));
                }
                else
                {
                    Storyboard.SetTarget(animations[i], rectTracteurRouge);
                    Storyboard.SetTargetProperty(animations[i], new PropertyPath("(Canvas.Left)"));
                }
            }

            // Commencer animations avec delais secondes
            for (int i = 0; i < animations.Length; i++)
            {
                animations[i].BeginTime = TimeSpan.FromSeconds((i+1) * vitesseTracteurAdversaire);
            }

            storyboard.Begin();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitPositionAdversaire();
            DeplacerTracteurAdversaire();
        }
        private void Collision()
        {
            tracteurHitbox = new Rect(Canvas.GetLeft(rectTracteur), Canvas.GetTop(rectTracteur), rectTracteur.Width, rectTracteur.Height);
            adversaireHitbox = new Rect(Canvas.GetLeft(rectTracteurRouge), Canvas.GetTop(rectTracteurRouge), rectTracteurRouge.Width, rectTracteurRouge.Height);
            if (tracteurHitbox.IntersectsWith(adversaireHitbox))
            {
                if (droite)
                {
                    Canvas.SetLeft(rectTracteur, Canvas.GetLeft(rectTracteur) - vitesseTracteurJoueur);
                    droite = false;
                }

                if (gauche)
                {
                    Canvas.SetLeft(rectTracteur, Canvas.GetLeft(rectTracteur) + vitesseTracteurJoueur);
                    gauche = false;
                }

                if (haut)
                {
                    Canvas.SetTop(rectTracteur, Canvas.GetTop(rectTracteur) + vitesseTracteurJoueur);
                    haut = false;
                }

                if (bas)
                {
                    Canvas.SetTop(rectTracteur, Canvas.GetTop(rectTracteur) - vitesseTracteurJoueur);
                    bas = false;
                }
            }
        }
        // Charger le fihcier en tableau 2D
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
        private static bool EstSurLeCircuit(int[,] circuit, double taileLargeurCanvas, double tailleHauteurCanvas, double tracteurXPixel, double tracteurYPixel)
        {
            if (taileLargeurCanvas == 0 || tailleHauteurCanvas == 0) // Vérifie si le canvas n'est pas initialisé
            {
                Console.WriteLine("Erreur : Le canvas n'a pas encore été initialisé correctement.");
                return false;
            }

            int nbColonne = circuit.GetLength(1); // Détermine le nb de colonnes dans le tableau 
            int nbLignes = circuit.GetLength(0);  // Détermine le nb de lignes dans le tableau

            double tailleLargeurPixel = taileLargeurCanvas / nbColonne; // Calcule la largeur d'une case en pixel
            double tailleHauteurPixel = tailleHauteurCanvas / nbLignes; // Idem pour la hauteur

            int indiceColonne = (int)(tracteurXPixel / tailleLargeurPixel); // Calcule l'indice de colonne pour la position X
            int indiceLigne = (int)(tracteurYPixel / tailleHauteurPixel); // Calcule l'indice de ligne pour la position Y

            if (indiceColonne < 0 || indiceColonne >= nbColonne || indiceLigne < 0 || indiceLigne >= nbLignes) // Vérifie si les indices sont en dehords des limites du tableau 
            {
                Console.WriteLine("Erreur : Le tracteur est en dehors des limites du tableau.");
                return false;
            }


            if (circuit[indiceLigne, indiceColonne] == 1) // Vérifie si la case du talbeau de la position du tracteur est un 1
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