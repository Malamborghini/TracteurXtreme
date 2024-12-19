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
using static System.Net.WebRequestMethods;

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

        public bool uneSeulefois = true,
                    jeuEnPause = false,
                    jeuTermine = false,
                    intersectionBonusRoue = true,
                    intersectionBonusDiesel = true,
                    gagne = false,
                    montrerMsgBox = true,
                    modeTriche = false;

        public double tracteurXPixel;
        public double tracteurYPixel;
        static int[,] tabCircuit;
        long tempsEcouleTotal;
        public long tempsDerniereImageChangee = 0;
        public int changerImageTracteurRouge = 0, nbToucheLigneArrive = 0;
        ImageBrush backgroundLabelGo;

        private Storyboard? adversaireStoryboard;
        public BitmapImage Rose { get; set; }
        public BitmapImage Feu { get; set; }
        public BitmapImage Ferme { get; set; }
        public BitmapImage Aquatique { get; set; }
        public static string ChoixDecor {  get; set; }

        // Variables pour comter les tours effectues et cooldown
        int nbToursEffectues = 0; // Compte combien de fois la ligne d'arrivée est franchie
        bool ligneArriveCooldown = false; // Cooldown ligne arrive
        DateTime dernierTempsTraverse = DateTime.MinValue; // La dernière fois que le trateur a franchi la ligne d'arrivée
        const int seuilRechargement = 1000; // Cooldown en milliseconds (1 second)

        // Variables pour comter les tours effectues et cooldown adversaire
        int nbToursAdversaire = 0; // Compte combien de fois la ligne d'arrivée est franchie
        bool adversaireArriveCooldown = false; // Cooldown ligne arrive
        DateTime dernierTempsAdversaire = DateTime.MinValue; // La dernière fois que le trateur a franchi la ligne d'arrivée
        const int rechargementAdversaire = 1000; // Cooldown en milliseconds (1 second)

        private static MediaPlayer musique;

        public MainWindow()
        {
            InitializeComponent();
            InitBitmap();
            InitDecor();
            menuPrincipal = new MenuPrincipal();
            menuPrincipal.ShowDialog();
            InitTimer();
            InitMusique();

            double taileLargeurCanvas = canvasPiste.ActualHeight; // Hauteur du Canvas
            double tailleHauteurCanvas = canvasPiste.ActualWidth; // Largeur du Canvas
            ChargementTableau(); // Appel de la méthode pour chargé le talbeau en 2D
            EstSurLeCircuit(tabCircuit, tailleHauteurCanvas, taileLargeurCanvas, tracteurXPixel, tracteurYPixel); // Appel méthode parce que j'ai eu un message qui m'a fait peur quand je l'ai enlevé 
            this.Loaded += (s, e) => InitialiserCanvas(taileLargeurCanvas, tailleHauteurCanvas); //quand le main aura fini de charger execute la méthode InitialiserCanvas (s = sender, e = args)

        }
        private void InitTimer()
        {
            chronometre = Stopwatch.StartNew(); // mesurer le temps écoulé
            minuterie = new DispatcherTimer();
            minuterie.Interval = TimeSpan.FromMilliseconds(16);
            minuterie.Tick += Jeu;
            minuterie.Start();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (jeuTermine) { CommencerJeu(); }
            else if (montrerMsgBox)
            {
                MessageBox.Show("Impossible de rejouer tant que la partie n'est pas terminé");
                montrerMsgBox = false;
            }
            montrerMsgBox = true;
        }
        private void AfficherChrono()
        {
            double emplacementChrono = (canvasPiste.ActualWidth / 2);
            Canvas.SetLeft(labChrono, emplacementChrono);
            Canvas.SetTop(labChrono, 0);

            double abssice = (canvasPiste.ActualWidth / 2) - labDepart.Width/2;
            double ordonne = (canvasPiste.ActualHeight / 2) - labDepart.Height/2;

            Canvas.SetLeft(labDepart, abssice);
            Canvas.SetTop(labDepart, ordonne );

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
            if (!jeuEnPause)
            {
                // mesure temps écoulé depuis le changement de la derniere image
                tempsEcouleTotal = chronometre.ElapsedMilliseconds;
                if (tempsEcouleTotal - tempsDerniereImageChangee >= 2000) // 2000 ms = 2 seconds
                {
                    ChangeTracteurImage();
                    tempsDerniereImageChangee = tempsEcouleTotal; // met à jour le temps du dernier changement d'image
                }

                backgroundLabelGo = new ImageBrush();
                backgroundLabelGo.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/drapeauxCourse/racingFlag4.png"));
                backgroundLabelGo.Stretch = Stretch.Fill;

                // label ready go
                if (tempsEcouleTotal < 1500)
                {
                    labDepart.Content = "Ready";
                    labDepart.Background = backgroundLabelGo;
                }
                if (tempsEcouleTotal >= 1500 && tempsEcouleTotal < 2000)
                {
                    labDepart.Content = "  GO!";
                    labDepart.Background = backgroundLabelGo;
                }
                else if (tempsEcouleTotal >= 2000)
                {
                    labDepart.Content = "";
                    labDepart.Background = null;
                }

                AfficherChrono();
                InitPositionAdversaire();
                DeplacerJoueur();
                Collision();
                ChangerNiveau();
                double posInitX = (canvasPiste.ActualWidth - rectLigneArrive.Width) - (canvasPiste.ActualWidth / 1.18);
                double posInitY = (canvasPiste.ActualHeight - rectLigneArrive.Height) - (canvasPiste.ActualHeight / 2.2);
                Canvas.SetLeft(rectLigneArrive, posInitX);
                Canvas.SetTop(rectLigneArrive, posInitY);
                rectLigneArrive.Width = canvasPiste.ActualWidth / 9;

                FinirCourse();
            }
        }
        private void ChangerNiveau()
        {
            switch (ChoixDecor)
            {
                case "cbNiveauRose":
                    canvasPiste.Background = new ImageBrush(Rose);
                    vitesseTracteurJoueur = 10;
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
            GestionBonus();
        }
        private void GestionBonus()
        {
            Canvas.SetTop(rectBonusUneRoue, canvasPiste.ActualHeight / 1.2);
            Canvas.SetLeft(rectBonusUneRoue, canvasPiste.ActualWidth / 5.5);

            Canvas.SetTop(rectBonusDiesel, canvasPiste.ActualHeight / 17);
            Canvas.SetLeft(rectBonusDiesel, canvasPiste.ActualWidth / 2);

            if (nbToursEffectues >= 1 && intersectionBonusRoue)
            {
                rectBonusUneRoue.Visibility = Visibility.Visible;
                intersectionBonusRoue = false;
            }
            if (nbToursEffectues >= 2 && intersectionBonusDiesel)
            {
                rectBonusDiesel.Visibility = Visibility.Visible;
                intersectionBonusDiesel = false;
            }
            foreach (var x in canvasPiste.Children.OfType<Rectangle>())
            {
                Rect bonusHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                if ((string)x.Tag == "bonus")
                {
                    if (tracteurHitbox.IntersectsWith(bonusHitBox) && x.Visibility == Visibility.Visible)
                    {
                        x.Visibility = Visibility.Hidden;
                        vitesseTracteurJoueur += 5;
                        Console.WriteLine("ma vitesse " + vitesseTracteurJoueur);
                    }
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            menuPrincipal.ShowDialog();
            jeuEnPause = true;
            adversaireStoryboard.Pause();
            labPauseJeu.Content = "Pause";
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
                if (!jeuEnPause)
                {
                    minuterie.Stop();
                    adversaireStoryboard?.Pause(); // Pause adversaire storyboard
                    if (chronometre.IsRunning)
                    {
                        chronometre.Stop(); // Stop timer images
                    }
                    labPauseJeu.Content = "Pause";
                }
                else
                {
                    minuterie.Start();
                    adversaireStoryboard?.Resume(); // Resume adversaire storyboard
                    if (!chronometre.IsRunning)
                    {
                        chronometre.Start(); // Resume timer images
                    }
                    labPauseJeu.Content = "";
                }
                jeuEnPause = !jeuEnPause;
            }

            if (jeuTermine && e.Key==Key.R)
            {
                CommencerJeu();
            }

            if (!jeuTermine && e.Key == Key.T)
            {
                modeTriche = true;
            }
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
            bool estSurLeCircuit = EstSurLeCircuit(tabCircuit, canvasPiste.ActualWidth, canvasPiste.ActualHeight, tracteurXPixel, tracteurYPixel);
            if (estSurLeCircuit == false)
            {
                if (!modeTriche)
                {
                    vitesseTracteurJoueur = 1;
                    Console.WriteLine("vitesse réduit");
                }
            }
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
            if (!jeuEnPause)
            {
                if (tempsEcouleTotal >= 2000)
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
            
            tracteurXPixel = Canvas.GetLeft(rectTracteur);
            tracteurYPixel = Canvas.GetTop(rectTracteur);
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
            double pointDePiste_11 = (canvasPiste.ActualHeight / 2) - (rectTracteurRouge.Height / 2);

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
            adversaireStoryboard = new Storyboard();
            foreach (DoubleAnimation animation in animations)
            {
                adversaireStoryboard.Children.Add(animation);
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

            // repeter les animations 2 fois
            adversaireStoryboard.RepeatBehavior = new RepeatBehavior(2);  // repeter 2 fois

            adversaireStoryboard.Begin();
        }
        private void ChangeTracteurImage()
        {
            switch (changerImageTracteurRouge)
            {
                case 1:
                    imgFillTracteurRouge.ImageSource = tracteurRougeBas;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    if (chronometre.ElapsedMilliseconds - tempsDerniereImageChangee >= 2000)
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
                case 11:
                    imgFillTracteurRouge.ImageSource = tracteurRougeBas;
                    rectTracteurRouge.Height = canvasPiste.ActualHeight / 15;
                    rectTracteurRouge.Width = canvasPiste.ActualWidth / 35;
                    break;
            }
            changerImageTracteurRouge++;
            if (changerImageTracteurRouge > 11)
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
            ligneArriveHitbox = new Rect(Canvas.GetLeft(rectLigneArrive), Canvas.GetTop(rectLigneArrive), rectLigneArrive.Width, rectLigneArrive.Height);

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

            // le joueur gagne
            if (tracteurHitbox.IntersectsWith(ligneArriveHitbox) && !ligneArriveCooldown) // verifier collision et cooldown
            {
                // Activer cooldown
                ligneArriveCooldown = true;
                dernierTempsTraverse = DateTime.Now;

                // Incrementer le compteur de tours
                nbToursEffectues++;

                // Check tour final
                if (nbToursEffectues == 3)
                {
                    gagne = true;
                    jeuTermine = true;
                    MessageBox.Show("Woohoo tu as gagné !");
                    nbToursEffectues = 0; // reinitisaliser pour rejouer
                }

                // Cooldown reset (asynchronous delay to avoid blocking UI thread)
                Task.Delay(seuilRechargement).ContinueWith(_ =>
                {
                    ligneArriveCooldown = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }

            // le joueur perd
            //if (adversaireHitbox.IntersectsWith(ligneArriveHitbox) && !ligneArriveCooldown && !gagne) // verifier collision et cooldown
            //{
            //    // Activer cooldown
            //    adversaireArriveCooldown = true;
            //    dernierTempsAdversaire = DateTime.Now;

            //    // Incrementer le compteur de tours
            //    nbToursAdversaire++;

            //    // Check tour final
            //    if (nbToursAdversaire == 3)
            //    {
            //        gagne = false;
            //        jeuTermine = true;
            //        MessageBox.Show("Boohoo tu as perdu !");
            //        nbToursAdversaire = 0; // reinitisaliser pour rejouer
            //    }

            //    // Cooldown reset (asynchronous delay to avoid blocking UI thread)
            //    Task.Delay(rechargementAdversaire).ContinueWith(_ =>
            //    {
            //        adversaireArriveCooldown = false;
            //    }, TaskScheduler.FromCurrentSynchronizationContext());
            //}
        }
        // Charger le fihcier en tableau 2D
        public static void ChargementTableau()
        {
            Uri cheminTabUri = new Uri("pack://application:,,,/img/tabPistes/piste1.txt");
            StreamResourceInfo resourceInfo = Application.GetResourceStream(cheminTabUri); // Accede au contenu du fichier

            using (StreamReader lecture = new StreamReader(resourceInfo.Stream)) // Lire le contenue du fichier
            {
                string[] lignes = lecture.ReadToEnd().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // Lire tout le fichier et divise ligne par ligne 

                // Déterminer le nombre de colonnes (longueur de la plus longue ligne)
                int nbLignes = lignes.Length;
                int nbColonnes = lignes[0].Trim().Length;

                // Initialiser le tableau avec les dimensions appropriées
                tabCircuit = new int[nbLignes, nbColonnes];

                for (int i = 0; i < nbLignes; i++)
                {
                    string ligneActuelle = lignes[i].Trim(); // Supprimer les espaces inutiles
                    for (int j = 0; j < ligneActuelle.Length; j++)
                    {
                        tabCircuit[i, j] = ligneActuelle[j] - '0'; // Conversion directe de char à int
                    }
                    for (int j = ligneActuelle.Length; j < nbColonnes; j++) // Remplir les colonnes restantes avec une valeur par défaut
                    {
                        tabCircuit[i, j] = 0;
                    }
                }
            }

        }
        // Verifie si le tracteur se trouve sur le circuit
        private bool EstSurLeCircuit(int[,] circuit, double taileLargeurCanvas, double tailleHauteurCanvas, double tracteurXPixel, double tracteurYPixel)
        {
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
                //Console.WriteLine("Vous êtes sur le circuit");
                return true;

            }
            else
            {
                Console.WriteLine("pas sur le circuit");
                return false;
            }
        }

        private void InitialiserCanvas(double taileLargeurCanvas, double tailleHauteurCanvas)
        {
            if (taileLargeurCanvas == 0 || tailleHauteurCanvas == 0) // Vérifie si le canvas est initialisé
            {
                Console.WriteLine("Erreur : Le canvas n'a pas encore été initialisé correctement.");
                return;
            }
        }
        private void FinirCourse()
        {
            //bool uneFois = true;
            //if (tempsEcouleTotal >= 25000 && tracteurHitbox.IntersectsWith(ligneArriveHitbox) && uneFois)
            //{
            //    gagne = true;
            //    jeuTermine = true;
            //    minuterie.Stop();
            //    chronometre.Stop();
            //    MessageBox.Show("Vous avez gagne");
            //    uneFois = false;
            //    adversaireStoryboard.Stop();
            //}

            if (tempsEcouleTotal >= 48000 && tempsEcouleTotal <= 48100 && !gagne)
            {
                MessageBox.Show("Vous avez perdu");
                jeuTermine = true;
            }

            if (jeuTermine)
            {
                minuterie.Stop();
                chronometre.Stop();
                adversaireStoryboard.Stop();
            }
        }
        private void CommencerJeu()
        {
            gauche = false;
            droite = false;
            haut = false;
            bas = false;

            nbToursEffectues = 0;
            gagne = false;
            jeuEnPause = false;
            jeuTermine = false;
            modeTriche = false;

            InitTimer();
            uneSeulefois = true;
            DeplacerTracteurAdversaire();

            tempsDerniereImageChangee = 0;
            changerImageTracteurRouge = 0;
            nbToucheLigneArrive = 0;

            intersectionBonusDiesel = true;
            intersectionBonusRoue = true;
            rectBonusDiesel.Visibility = Visibility.Hidden;
            rectBonusUneRoue.Visibility = Visibility.Hidden;
        }
        private void InitMusique()
        {
            musique = new MediaPlayer();
            musique.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory +
           "music/You_drive_me _crazy.mp3"));
            musique.MediaEnded += RelanceMusique;
            musique.Volume = 1;
            musique.Play();
        }
        private void RelanceMusique(object? sender, EventArgs e)
        {
            musique.Position = TimeSpan.Zero;
            musique.Play();
        }
    }
}