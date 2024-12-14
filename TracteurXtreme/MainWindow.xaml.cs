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
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TracteurXtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int vitesseTracteurJoueur = 4;
        public double vitesseTracteurAdversaire = 2 ; //plus c'est elevee plus c'est lent, plus c'est petit plus c'est rapide
        public static bool gauche, droite, haut, bas;
        private static BitmapImage tracteurGauche, tracteurDroite, tracteurBas, tracteurHaut,
                                   tracteurRougeDroite, tracteurRougeGauche, tracteurRougeBas, tracteurRougeHaut;
        public Rect tracteurHitbox;
        public Rect adversaireHitbox;
        public Rect ligneArriveHitbox;
        public MenuPrincipal menuPrincipal;
        public DispatcherTimer minuterie;
        public Stopwatch chronometre;
        public TimeSpan tempsEcoule;
        int secondes = 0;
        public bool uneSeulefois = true, jeuEnPause = false;
        public double tracteurXPixel;
        public double tracteurYPixel;
        static int[,] tabCircuit;
        public long lastImageChangeTime = 0;
        public int changerImageTracteurRouge = 0;
        Rectangle bonusDiesel, bonusUneRoue, bonusDesRoues, bonusCollecteChamps;
        ImageBrush bonusRoueImg, bonusDesRouesImg, bonusDieselImg, bonusChampsImg;
        public BitmapImage Rose { get; set; }
        public BitmapImage Feu { get; set; }
        public BitmapImage Ferme { get; set; }
        public BitmapImage Aquatique { get; set; }
        public static string ChoixDecor {  get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitBitmap();
            InitDecor();
            menuPrincipal = new MenuPrincipal();
            menuPrincipal.ShowDialog();
            InitTimer();
            //InitTopDepart();

            //Rectangle bonusDiesel = new Rectangle();
            //bonusDiesel.Width = 100;
            //bonusDiesel.Height = 100;
            //bonusDiesel.Fill = Brushes.Green;
            //bonusDiesel.Stroke = Brushes.Red;
            //bonusDiesel.StrokeThickness = 2;
            //bonusDiesel.Name = "rectBonusDiesel";
            //Canvas.SetTop(bonusDiesel, 0);
            //Canvas.SetLeft(bonusDiesel, 0);

            string cheminTab = "D:\\C#\\IUT\\sae_tracteur\\TracteurXtreme\\TracteurXtreme\\img\\tabPistes\\piste1.txt"; // Chemin du fichier binaire du tracé du circuit
            double taileLargeurCanvas = canvasPiste.ActualHeight; // Hauteur du Canvas
            double tailleHauteurCanvas = canvasPiste.ActualWidth; // Largeur du Canvas
            ChargementTableau(cheminTab); // Appel de la méthode pour chargé le talbeau en 2D
            EstSurLeCircuit(tabCircuit, tailleHauteurCanvas, taileLargeurCanvas, tracteurXPixel, tracteurYPixel); // Appel méthode parce que j'ai eu un message qui m'a fait peur quand je l'ai enlevé 
            this.Loaded += (s, e) => InitialiserCanvas(); //quand le main aura fini de charger execute la méthode InitialiserCanvas (s = sender, e = args)
        }
        private void InitTimer()
        {
            //imgFillTracteurRouge.ImageSource = tracteurRougeBas; // Start with case 1 image
            //changerImageTracteurRouge = 0;

            chronometre = Stopwatch.StartNew(); // mesurer le temps écoulé
            minuterie = new DispatcherTimer();
            minuterie.Interval = TimeSpan.FromMilliseconds(16);
            minuterie.Tick += Jeu;
            minuterie.Start();
        }
        //private void InitTopDepart()
        //{
        //    minuterie = new DispatcherTimer();
        //    minuterie.Interval = TimeSpan.FromSeconds(1);
        //    minuterie.Tick += Minuterie_Tick;
        //    minuterie.Start();
        //}
        //private void Minuterie_Tick(object sender, EventArgs e)
        //{
        //    secondes++;
        //    if (secondes == 1) { labDepart.Content = "3"; }
        //    if (secondes == 2) { labDepart.Content = "2"; }
        //    if (secondes == 3) { labDepart.Content = "1"; }
        //    if (secondes == 4) { labDepart.Content = "Go"; }
        //    if (secondes > 4) 
        //    {
        //        labDepart.Content = "";
        //        minuterie.Stop();
        //        InitTimer();               
        //    }           
        //}
        private void AfficherChrono()
        {
            double emplacementChrono = (canvasPiste.ActualWidth / 2);
            Canvas.SetLeft(labChrono, emplacementChrono);
            Canvas.SetTop(labChrono, 0);

            tempsEcoule = chronometre.Elapsed; // récupère temps écoulé
            labChrono.Content = tempsEcoule.ToString(@"mm\:ss\.fff"); // format chrono (minutes : secondes : millisecondes)
        }
        private void InitBitmap()
        {
            tracteurGauche = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/gauche/tracteurNoirJoueur_gauche.png"));
            tracteurDroite = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/droite/tracteurNoirJoueur_droite.png"));
            tracteurHaut = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/haut/tracteurNoirJoueur_haut.png"));
            tracteurBas = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/bas/tracteurNoirJoueur_bas.png"));

            tracteurRougeGauche = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/gauche/tracteurRouge_gauche.png"));
            tracteurRougeDroite = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/droite/tracteurRouge_droite.png"));
            tracteurRougeHaut = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/haut/tracteurRouge_haut.png"));
            tracteurRougeBas = new BitmapImage(new Uri("pack://application:,,,/img/imgTracteurs/bas/tracteurRouge_bas.png"));

            bonusRoueImg = new ImageBrush();
            bonusDesRouesImg = new ImageBrush();
            bonusDieselImg = new ImageBrush();
            bonusChampsImg = new ImageBrush();
            bonusRoueImg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bonus/bonus_1roue.png"));
            bonusDesRouesImg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bonus/bonus_desRoues.png"));
            bonusDieselImg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bonus/bonus_diesel.png"));
            bonusChampsImg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bonus/bonus_champs.png"));
        }
        private void InitDecor()
        {
            Rose = new BitmapImage(new Uri("pack://application:,,,/img/decorNiveau/pisteBulleRose.jpg"));
            Aquatique = new BitmapImage(new Uri("pack://application:,,,/img/decorNiveau/pisteAquatique.jpg"));
            Feu = new BitmapImage(new Uri("pack://application:,,,/img/decorNiveau/pisteFeu.jpg"));
            Ferme = new BitmapImage(new Uri("pack://application:,,,/img/piste_fondVert.png"));
        }
        private void Jeu(object? sender, EventArgs e)
        {
            // Measure elapsed time since last image change
            long elapsedTime = chronometre.ElapsedMilliseconds;
            if (elapsedTime - lastImageChangeTime >= 2000) // 2000 ms = 2 seconds
            {
                ChangeTracteurImage();
                lastImageChangeTime = elapsedTime; // Update the last image change time
            }

            AfficherChrono();
            InitPositionAdversaire();
            DeplacerJoueur();
            ChangerNiveau();
            double posInitX = (canvasPiste.ActualWidth - rectLigneArrive.Width) - (canvasPiste.ActualWidth / 1.18);
            double posInitY = (canvasPiste.ActualHeight - rectLigneArrive.Height) - (canvasPiste.ActualHeight / 2.5);
            Canvas.SetLeft(rectLigneArrive, posInitX);
            Canvas.SetTop(rectLigneArrive, posInitY);
            rectLigneArrive.Height = canvasPiste.ActualHeight / 10;
            rectLigneArrive.Width = canvasPiste.ActualWidth / 9.5;
        }
        private void ChangerNiveau()
        {
            switch (ChoixDecor)
            {
                case "cbNiveauRose":
                    canvasPiste.Background = new ImageBrush(Rose);
                    vitesseTracteurJoueur = 10;

                    bonusDiesel = new Rectangle();
                    bonusDiesel.Width = 50;
                    bonusDiesel.Height = 50;
                    //bonusDiesel.Fill = Brushes.Green;
                    //bonusDiesel.Stroke = Brushes.Red;
                    ImageBrush imageBrush = new ImageBrush();
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bonus/bonus_diesel.png"));
                    bonusDiesel.Fill = imageBrush;
                    bonusDiesel.StrokeThickness = 2;
                    bonusDiesel.Name = "rectBonusDiesel";
                    Thickness margin = bonusDiesel.Margin;
                    margin.Left = 50;
                    bonusDiesel.Margin = margin;
                    canvasPiste.Children.Add(bonusDiesel);
                    Canvas.SetTop(bonusDiesel, canvasPiste.ActualHeight/17);
                    Canvas.SetLeft(bonusDiesel, canvasPiste.ActualWidth/2);


                    break;
                case "cbNiveauFeu":
                    canvasPiste.Background = new ImageBrush(Feu);
                    vitesseTracteurJoueur = 2;
                    break;
                case "cbNiveauFerme":
                    canvasPiste.Background = new ImageBrush(Ferme);
                    vitesseTracteurJoueur = 5;

                    break;
                case "cbNiveauAquatique":
                    canvasPiste.Background = new ImageBrush(Aquatique);
                    vitesseTracteurJoueur = 3;
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            menuPrincipal.ShowDialog();
            jeuEnPause = true;
            //menuPrincipal.DialogResult = false;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) { gauche = true; }  // flèche gauche pressée
            if (e.Key == Key.Right) { droite = true; }// flèche droite pressée
            if (e.Key == Key.Up) { haut = true; } // flèche haut pressée
            if (e.Key == Key.Down) { bas = true; } // flèche bas pressée

            if (jeuEnPause) { minuterie.Stop(); }
            else { minuterie.Start(); }

            // mettre en pause
            if (e.Key == Key.Space || e.Key == Key.P)
            {
                if (!jeuEnPause) { jeuEnPause = true; }
                else { jeuEnPause = false; }
            }
            if (jeuEnPause)
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
            double pointDePiste_1 = (canvasPiste.ActualHeight /1.07) - (rectTracteurRouge.Height/1.07);
            double pointDePiste_2 = (canvasPiste.ActualWidth / 2) - (rectTracteurRouge.Width / 2);
            double pointDePiste_3 = (canvasPiste.ActualHeight / 2.9) - (rectTracteurRouge.Height / 2.9);
            double pointDePiste_4 = (canvasPiste.ActualWidth/1.35) - (rectTracteurRouge.Width/1.35);
            double pointDePiste_5 = (canvasPiste.ActualHeight / 2.1) - (rectTracteurRouge.Height / 2.1);
            double pointDePiste_6 = (canvasPiste.ActualWidth / 1.7) - (rectTracteurRouge.Width / 1.7);
            double pointDePiste_7 = (canvasPiste.ActualHeight / 1.3) - (rectTracteurRouge.Height / 1.3);
            double pointDePiste_8 = (canvasPiste.ActualWidth / 1.06) - (rectTracteurRouge.Width / 1.06);
            double pointDePiste_9 = (canvasPiste.ActualHeight / 16) - (rectTracteurRouge.Height / 16);
            double pointDePiste_10 = (canvasPiste.ActualWidth / 18) - (rectTracteurRouge.Width / 18);
            double pointDePiste_11 = (canvasPiste.ActualHeight / 1.9) - (rectTracteurRouge.Height / 1.9);

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
                },

                new DoubleAnimation
                {
                    From = pointDePiste_6,
                    To = pointDePiste_8,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_7,
                    To = pointDePiste_9,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_8,
                    To = pointDePiste_10,
                    Duration = TimeSpan.FromSeconds(vitesseTracteurAdversaire)
                },

                new DoubleAnimation
                {
                    From = pointDePiste_9,
                    To = pointDePiste_11,
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
            //if (jeuEnPause) { storyboard.Pause(); }
        }
        private void ChangeTracteurImage()
        {
            switch (changerImageTracteurRouge)
            {
                case 1:
                    imgFillTracteurRouge.ImageSource = tracteurRougeBas;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    if (chronometre.ElapsedMilliseconds - lastImageChangeTime >= 2000)
                    {
                        imgFillTracteurRouge.ImageSource = tracteurRougeDroite;
                        rectTracteurRouge.Height = canvasPiste.ActualHeight / 18;
                        rectTracteurRouge.Width = canvasPiste.ActualWidth / 28;
                    }
                    break;
                case 2:
                    imgFillTracteurRouge.ImageSource = tracteurRougeHaut;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    break;
                case 3:
                    imgFillTracteurRouge.ImageSource = tracteurRougeDroite;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 18;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 28;
                    break;
                case 4:
                    imgFillTracteurRouge.ImageSource = tracteurRougeBas;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    break;
                case 5:
                    imgFillTracteurRouge.ImageSource = tracteurRougeGauche;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 18;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 28;
                    break;
                case 6:
                    imgFillTracteurRouge.ImageSource = tracteurRougeBas;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    break;
                case 7:
                    imgFillTracteurRouge.ImageSource = tracteurRougeDroite;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 18;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 28;
                    break;
                case 8:
                    imgFillTracteurRouge.ImageSource = tracteurRougeHaut;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    break;
                case 9:
                    imgFillTracteurRouge.ImageSource = tracteurRougeGauche;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 18;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 28;
                    break;
                case 10:
                    imgFillTracteurRouge.ImageSource = tracteurRougeBas;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    break;
            }
            changerImageTracteurRouge++;
            if (changerImageTracteurRouge > 10)
            {
                changerImageTracteurRouge = 0;
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitPositionAdversaire();
            DeplacerTracteurAdversaire(); 
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
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
            ligneArriveHitbox = new Rect(Canvas.GetLeft(rectLigneArrive), Canvas.GetTop(rectLigneArrive), rectLigneArrive.Width, rectLigneArrive.Height);
            if (tracteurHitbox.IntersectsWith(ligneArriveHitbox))
            {
                Console.WriteLine("touche ligne arrivee");
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