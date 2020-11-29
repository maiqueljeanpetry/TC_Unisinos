// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System;
    using System.Windows.Controls;
    using Point = System.Windows.Point;
    using System.Timers;
    using System.Data;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.Collections;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Media.Animation;
    using System.Threading;
    using System.Windows.Threading;
    using System.Media;
    using Timer = System.Threading.Timer;
   using MathNet;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;



    /// <summary>
    /// Lógica de interação para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //utilizado para captura de audio
        private SpeechRecognitionEngine speechEngine;

        // ****    Largura do desenho de saída
        private const float RenderWidth = 640.0f;

         // ****Saída de desenho alto
        private const float RenderHeight = 480.0f;

         // ****Espessura dos pontos de detecção
        private const double JointThickness = 5;

        //Espessura da elipse do centro do corpo
        private const double BodyCenterThickness = 10;

  
        // ****Espessura das linhas de borda da tela (linhas vermelhas)
        private const double ClipBoundsThickness = 10;

        // ****Ponto central de detecção do corpo
        private readonly Brush centerPointBrush = Brushes.Blue;

        // ****Cor de pontos de detecção detectados
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        // ****cor dos pontos de detecção intuitiva
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        // ****Espessura e cor dos ossos esqueléticos detectados
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        // ****Espessura e cor dos ossos de esqueleto intuidos
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        // ****Ativação do kinect
        private KinectSensor sensor;
        
        // ****Grupo de desenho de esqueleto
        private DrawingGroup drawingGroup;

        // ****Desenho da imagem a ser exibida
        private DrawingImage imageSource;

        //----------------------------------
        //--------variáveis ​​adicionadas--------
        //----------------------------------

        /// Para reservar pixels de color
        private byte[] colorPixels;

        /// mapa de bit de color
        private WriteableBitmap colorBitmap;

        /// Espessura e cor da ombro e quadril "osso"
        private Pen trackedBonePenHead = new Pen(Brushes.Green, 6);

        /// será verdade se o movimento da cabeça começou, falso caso contrário
        private bool movIniciado = false;

        private bool movIni = false;

       /// será verdade quando o movimento da cabeça for bem sucedido
        private bool movFinalizado = false;

        /// número de frames por segundo
        private readonly int FPS = 30;

        /// contador de frames
        private int contFPS = 0;

        public int vacerto;
        public int verro;
        public Boolean viniciou;
        public Boolean vfinalizou;
        public int Vpontos;
        public int vlbacertos;
        public int totaacertos;
        public int vlbrepeticoes;
        public int vlberros;
        public int vangulo;
        public Boolean vacertoumaca = false;
        public Boolean mostramaca = false;
        public int vcount = 0;
        public double vangulo_x = 0;
        public double vangulo_y = 0;


        private int counter;

        //controla calibração
        public string vcalib = "Não Calibrado";

        public int idpaciente;

        /// Espera inicial para que o usuário esteja em posição
        private bool esperaInicial = true;


        //--------------------------------------------------------------------------------------------------
        //------------------------------------------------METODOS-------------------------------------------
        //--------------------------------------------------------------------------------------------------
        // ****Inicialização dos componentes da classe
        public MainWindow()
        {
            InitializeComponent();
            //torna invisivel as frutas
            banana.Visibility = Visibility.Hidden;
            pera.Visibility = Visibility.Hidden;
            cereja.Visibility = Visibility.Hidden;
            certo.Visibility = Visibility.Hidden;
            errado.Visibility = Visibility.Hidden;
            macaverde.Visibility = Visibility.Hidden;
            maca.Visibility = Visibility.Hidden;


            //acertos  
            vacerto = 0;
            stacertos.Content = vacerto.ToString();

            //erros
            verro = 0;

            //ponto
            Vpontos = 0;
            stpontos_nivel.Content = Vpontos.ToString();

            stpontos.Content = "0";

            btFinalizar.IsEnabled = false;
            btCancelar.IsEnabled = false;
        }

        // Desenha indicadores para mostrar quais arestas estão cortando dados do esqueleto
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        // **** Fronteiras na tela de continuidade do esqueleto(sua própria chamada é desativada)
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// ***Execução de tarefas iniciais
        /// </summary>
        /// <param name="sender">objeto enviando o evento</param>
        /// <param name="e">argumentos de evento</param>



        //captura de audio
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //torna invisivel as frutas
            banana.Visibility = Visibility.Hidden;
            pera.Visibility = Visibility.Hidden;
            cereja.Visibility = Visibility.Hidden;
            maca.Visibility = Visibility.Hidden;
            macaverde.Visibility = Visibility.Hidden;
            // Cria o grupo de desenho que usaremos para desenhar
            this.drawingGroup = new DrawingGroup();

            // Cria uma fonte de imagem que possamos usar em nosso controle de imagem
            this.imageSource = new DrawingImage(this.drawingGroup);


            // Examine todos os sensores e inicie o primeiro conectado
            // Isso requer que um Kinect esteja conectado no momento da inicialização do aplicativo.
            // Para tornar seu aplicativo robusto contra plug / desconecte,
          
            // *** Inicializa o sensor
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }


            if (this.sensor != null)
            {
                // No ImageE, o esqueleto será projetado e no ImageC a imagem colorida do sensin kinect
                // Exiba o desenho usando nosso controle de imagem
                ImageE.Source = this.imageSource;

                // Liga o fluxo de esqueleto para receber quadros de esqueleto
                this.sensor.SkeletonStream.Enable();

                // Adiciona um manipulador de eventos a ser chamado sempre que houver novos dados de quadro de cores
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // fluxo de cores
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Espaço reservado para pixels coloridos
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                //Bitmaps para mostrar na tela
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, 
                    this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Para que no objeto Imagem a imagem seja projetada
                this.ImageC.Source = this.colorBitmap;

                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Inicia o sensor
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }

            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

     
                //CRIAR E CARREGAR A GRAMÁTICA
                var comandosvoz = new Choices();
                comandosvoz.Add(new SemanticResultValue("iniciar","INICIAR"));
                comandosvoz.Add(new SemanticResultValue("salvar", "SALVAR"));
                comandosvoz.Add(new SemanticResultValue("cancelar","CANCELAR"));
                comandosvoz.Add(new SemanticResultValue("ponto","PONTOS"));

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(comandosvoz);

                var g = new Grammar(gb);

                speechEngine.LoadGrammar(g);

                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += SpeechRejected;
             

                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                this.statusBarText.Text = Properties.Resources.NoSpeechRecognizer;
            }
      }

        /// Executar tarefas de desligamento
           /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
                this.sensor.AudioSource.Stop();
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }


        //*** Para poder ver a imagem capturada pela câmera kinect
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    //Copia os dados de pixel da imagem para uma matriz temporária
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    //  Escreva os dados do pixel em nosso bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// Manipulador de eventos para o evento SkeletonFrameReady do sensor Kinect
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        // ****Skeleton Event Controller

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Desenha um plano de fundo transparente para definir o tamanho da renderização
                //É necessário declarar a "cor" como transparente para ver o esqueleto e a imagem do corpo

                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                         lblaviso.Visibility = Visibility.Visible;

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (!movFinalizado)
                               

                           //desenha esqueleto
                            this.DrawBonesAndJoints(skel, dc);

                            //calibracao 
                            this.calib(skel);

                  
                            //se estiver calibrado começa ação

                            ////testar dados banco de dados
                            if (vcalib == "Calibrado")
                            {
                                //variavel alimentada no botão INICIAR
                                if (viniciou == true)
                                {

                                   //busca angulo objeto
                                    if (mostramaca == false)
                                    {

                                        this.calculo_angulo(skel);
                                    }
                                    
                                    //posiciona maca na posicao do angulo
                                    if (vacertoumaca == false)
                                    {

                                        vcount = vcount + 1;
                                        angx_Copy2.Text = vcount.ToString();

                                        macaverde.Visibility = Visibility.Visible;

                                        double poscanvasx_ang = 0;
                                        double poscanvasy_ang = 0;

                                        //double vposx = Convert.ToDouble(angx.Text);
                                        //double vposy = Convert.ToDouble(angy.Text);

                                        poscanvasx_ang = (810 * vangulo_x) / 600;
                                        poscanvasy_ang = (610 * vangulo_y) / 400;

                                        ////  pointx.Text = poscanvasx.ToString();
                                        ////  pointy.Text = poscanvasy.ToString();


                                        Canvas.SetLeft(macaverde, vangulo_x);// - ellipse.Width / 2);
                                        Canvas.SetTop(macaverde, vangulo_y);// - ellipse.Height / 2);

                                        macaverde.Visibility = Visibility.Visible;
                                    }
                        


                                    //Testes mão esquerda
                                    this.maoe(skel);
                                }

                            }

                            
                            Joint maoe = skel.Joints[JointType.HandLeft];
                          

                            //teste mao esquerda
                             maoY.Text = SkeletonPointToScreen(maoe.Position).ToString();

                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                           
                        }
                    }
                }

                //  evitar desenhar fora da nossa área de renderização
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }




        //busca series
        private void BtSeries_Click(object sender, RoutedEventArgs e)
        {
            //busca séries liberadas que não estão concluídas
            lbnivel.Content = "";
            lbposicao1.Content = "";
            lbposicao3.Content = "";
            lbangulo.Content = "";
            lbarticulacao.Content = "";
            lbrepeticao.Content = "";
            lbacertos.Content = "";
            lberro.Content = "";
            lblaviso.Content = "";



            try
            {

                using (FisioEntities3 ctx = new FisioEntities3())


                {
                    var consulta = from S in ctx.TBSerie
                                   join T in ctx.TBTratamento on S.ID_TR equals T.ID_TR
                                   join U in ctx.TBUser on T.ID_User_PC equals U.ID

                                //   join XU in ctx.TBUser on T.ID_User_FS equals XU.ID



                                     where (U.Nome.Contains(txtNome.Text)
                                             && (S.Nivel_concluido == false)

                                             && (S.Nivel_liberado == true)
                                   )
                                   select new
                                   {
                                       ID_Serie = S.ID,

                                       Paciente = U.Nome,
                                       Nivel = S.Nivel,
                                       Angulo = S.Angulo,
                                       Articulacao = S.Articulacao,
                                       Posicao = S.Posicao,
                                       Repeticoes = S.Qt_rep_nivel,
                                       Id = T.ID_TR,
                                       ID_Paciente = T.ID_User_PC,
                                       Qt_Acertos = S.Qt_acertos,
                                       Qt_Erros = S.Qt_erros,
                                       Score = S.Score_nivel


                                  
                                   };


                   dgDados.ItemsSource = consulta.ToList();

                    //carrega ID da tabela de series para pesquisar todas series
                   dgDados.SelectedItem = dgDados.Items[0];

                    int total = consulta.Sum(S => S.Score.Value);
                
                }
                
            }

            catch { }

        }





            private void DgDados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtidnivel.Text = "";
            lbnivel.Content = ""; 
            lbposicao1.Content = "";
            lbposicao3.Content = "";
            lbangulo.Content = "";
            lbarticulacao.Content = "";
            lbrepeticao.Content = "";
            lbacertos.Content = "";
            lberro.Content = "";
            lbang.Text = "0";


            //carrega dados para tela
            if (dgDados.SelectedIndex >= 0)
            {
                object item = dgDados.SelectedItem;

                txtidnivel.Text= (dgDados.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                lbnivel.Content = (dgDados.SelectedCells[2].Column.GetCellContent(item) as TextBlock).Text;
                lbposicao1.Content = (dgDados.SelectedCells[5].Column.GetCellContent(item) as TextBlock).Text;
                lbposicao3.Content = (dgDados.SelectedCells[5].Column.GetCellContent(item) as TextBlock).Text;
                lbangulo.Content = (dgDados.SelectedCells[3].Column.GetCellContent(item) as TextBlock).Text;
                lbarticulacao.Content = (dgDados.SelectedCells[4].Column.GetCellContent(item) as TextBlock).Text;
                lbrepeticao.Content = (dgDados.SelectedCells[6].Column.GetCellContent(item) as TextBlock).Text;
                lbacertos.Content = (dgDados.SelectedCells[9].Column.GetCellContent(item) as TextBlock).Text;
                lberro.Content = (dgDados.SelectedCells[10].Column.GetCellContent(item) as TextBlock).Text;
                stpontos_nivel.Content = (dgDados.SelectedCells[11].Column.GetCellContent(item) as TextBlock).Text;
            }


            if (lbacertos.Content.Equals(""))
            {
                lbacertos.Content = "0";
            }


        }

        //utilizado para animação das outras frutas
        private Random rand1 = new Random();
        private Random rand2 = new Random();
        private Random rand3 = new Random();



        DispatcherTimer dt = new DispatcherTimer();
        DispatcherTimer dtp = new DispatcherTimer();
        DispatcherTimer dtc = new DispatcherTimer();
        DispatcherTimer dtm = new DispatcherTimer();

        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer2 = new DispatcherTimer();


        //animação
        public void Button_Click(object sender, RoutedEventArgs e)

        {

            //animacao banana
            InitializeComponent();
            dt.Interval = TimeSpan.FromSeconds(10);
            dt.Tick += new EventHandler(dt_banana);
            dt.Start();

            //animacao pera
            InitializeComponent();
            dtp.Interval = TimeSpan.FromSeconds(18);
            dtp.Tick += new EventHandler(dt_pera);
            dtp.Start();

            //animacao cereja
            InitializeComponent();
            dtc.Interval = TimeSpan.FromSeconds(12);
            dtc.Tick += new EventHandler(dt_cereja);
            dtc.Start();

        }

        void dt_macaverde(object sender, EventArgs e)
        {
            vacertoumaca = false;

        }

        //animação banana
        void dt_banana(object sender, EventArgs e)
        {
            if (Canvas.GetTop(banana) == 500)
            {
        
            }

            banana.Visibility = Visibility.Visible;
            DoubleAnimation ba = new DoubleAnimation(0, 500, TimeSpan.FromSeconds(5));
          //  ba.RepeatBehavior = RepeatBehavior.Forever;
            //  da.AutoReverse = true;

            banana.BeginAnimation(Canvas.TopProperty, ba);

         //   MessageBox.Show(Canvas.GetTop(banana).ToString()              );

        }
        
        //animação pera
        void dt_pera(object sender, EventArgs e)
        {
            //torna invisivel as frutas
            pera.Visibility = Visibility.Visible;
    
            DoubleAnimation pe = new DoubleAnimation(0, 500, TimeSpan.FromSeconds(10));
            //        pe.RepeatBehavior = RepeatBehavior.Forever;
            //        //  da.AutoReverse = true;

            pera.BeginAnimation(Canvas.TopProperty, pe);

        }

        //animação cereja
        void dt_cereja(object sender, EventArgs e)
        {
            //torna invisivel as frutas
            cereja.Visibility = Visibility.Visible;

            DoubleAnimation ce = new DoubleAnimation(0, 500, TimeSpan.FromSeconds(15));
            //        ce.RepeatBehavior = RepeatBehavior.Forever;
            //        //  da.AutoReverse = true;

                    cereja.BeginAnimation(Canvas.TopProperty, ce);



        }

        //animação maça
        void dt_maca(object sender, EventArgs e)
        {
            //torna invisivel as frutas
            maca.Visibility = Visibility.Visible;

            DoubleAnimation mc = new DoubleAnimation(0, 550, TimeSpan.FromSeconds(15));
            //        ce.RepeatBehavior = RepeatBehavior.Forever;
            //        //  da.AutoReverse = true;

            maca.BeginAnimation(Canvas.TopProperty, mc);
        }


        //calibraçao corpo na tela
        private void calib(Skeleton skeleton)
        {
            Joint tronco = skeleton.Joints[JointType.Spine];
            Joint cabech = skeleton.Joints[JointType.Head];

            Point point = new Point();
            SkeletonPoint skeletonPoint = cabech.Position;
            DepthImagePoint depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            point.X = depthPoint.X;
            point.Y = depthPoint.Y;
                      
            //se cabeca >60 e tronco no centro esta calibrado
            if ((point.Y>60) & (point.X >= 250 & point.X<=350))
            {
                statcalib.Content = "Calibrado";
                lbposicao.Visibility = Visibility.Hidden;
                vcalib = "Calibrado";
                lblaviso.Content = "";
            }
            else
            {
                statcalib.Content = "Não Calibrado";
                lbposicao.Visibility = Visibility.Visible;
                vcalib = "Não Calibrado";
                mostramaca = false;
            }

        }

        private void BtIniciar_Click(object sender, RoutedEventArgs e)
        {
            iniciar();
        }


        //testes mão esquerda
        public void maoe(Skeleton skeleton)
        {
            Joint maoesq = skeleton.Joints[JointType.HandLeft];
            //   Canvas objSender = macas as Canvas;

            Joint cotesq = skeleton.Joints[JointType.ElbowLeft];

            //ombro esquerdo
            Joint ombroesq = skeleton.Joints[JointType.ShoulderLeft];

            //mostra pontos do esqueleto da tela
            //    maox.Text = SkeletonPointToScreen(maoesq.Position).ToString();
            //    txtombro.Text = SkeletonPointToScreen(ombroesq.Position).ToString();

            esqueleto.Children.Clear();
            //desenha elipse
            Ellipse ellipse = new Ellipse
            {
                Fill = Brushes.Transparent,//.Pink,
                Width = 50,
                Height = 50
            };



            // 2D coordinates in pixels
            Point pointm = new Point();

            SkeletonPoint skeletonPoint = maoesq.Position;

            DepthImagePoint depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            pointm.X = depthPoint.X;
            pointm.Y = depthPoint.Y;


            int poscanvasx = 0;
            int poscanvasy = 0;

            poscanvasx = (810 * (int)pointm.X) / 640;
            poscanvasy = (610 * (int)pointm.Y) / 480;

            //desenha elipse na mão esquerda
            Canvas.SetLeft(ellipse, poscanvasx - 15);// - ellipse.Width / 2);
            Canvas.SetTop(ellipse, poscanvasy + 10);// - ellipse.Height / 2);

            esqueleto.Children.Add(ellipse);


            double top12 = Canvas.GetTop(maca);
            double left12 = Canvas.GetLeft(maca);

            macax.Text = Canvas.GetTop(ellipse).ToString();
            macay.Text = Canvas.GetLeft(ellipse).ToString();

            pointx.Text = poscanvasx.ToString();
            pointy.Text = poscanvasy.ToString();



            //PEGAR VALOR DO ANGULO DA TELA
            //   double angulox = 0; //Math.Cos(90 * Math.PI / 180) * raiosk; //* Math.PI / 180  ;// + pointomb.X;//+pos ombro x;
            //    double anguloy = 0; //Math.Sin(90 * Math.PI / 180) * raiosk;//// * Math.PI) / 180) * raiosk;// + pointomb.Y;
            //    angulox = (angulox + pointomb.X)/2;
            //      anguloy = anguloy + pointomb.Y;

            //angx_Copy.Text = Math.Cos(90).ToString();
            //angy_Copy.Text = Math.Sin(90).ToString();


            ////angx_Copy2.Text = Math.Cos(90 * Math.PI).ToString();
            ////angy_Copy2.Text = Math.Sin(90 * Math.PI).ToString();


            //Teste colisão maça verde
            if (Canvas.GetTop(ellipse) - 25 == Canvas.GetTop(macaverde) && Canvas.GetLeft(ellipse) - 25 == Canvas.GetLeft(macaverde))
            {
                Canvas.SetLeft(macaverde, 0);
                Canvas.SetTop(macaverde, 0);

                macaverde.Visibility = Visibility.Hidden;

                vacertoumaca = true;


                //sinal sonoro
                SystemSounds.Beep.Play();


                //se acertou parar sumir maca esperar x segundos e sumir certo
                timer.Interval = TimeSpan.FromSeconds(4);
                timer.Tick += dispatcherTimer_Tick;
                timer.Start();

                timer2.Interval = TimeSpan.FromSeconds(10);
                timer2.Tick += dispatcherTimer_Tick1;
                timer2.Start();

                vacerto += 1;
                stacertos.Content = vacerto.ToString();

                //alimenta score se acertou maca
                Vpontos += 2;
                stpontos_nivel.Content = Vpontos.ToString();


                txtacerto.Text = vacerto.ToString();
                certo.Visibility = Visibility.Visible;


                vlbacertos = Convert.ToInt32(stacertos.Content);
                //   totaacertos = vlbacertos + vacerto;
                vlbrepeticoes = Convert.ToInt32(lbrepeticao.Content);

                //testar se num de repetições = acertos perguntar se quer finalizar

                if (vlbacertos >= vlbrepeticoes)
                {

                    lbaviso1.Visibility = Visibility.Visible;

                }
            }

            //testar se acertou outro objeto com a elipse
            //se acertou diminui do score
            //Teste colisão maça 
            if (Canvas.GetTop(ellipse) - 25 == Canvas.GetTop(maca) && Canvas.GetLeft(ellipse) - 25 == Canvas.GetLeft(maca))
            {
                Canvas.SetLeft(maca, 0);
                Canvas.SetTop(maca, 0);

                maca.Visibility = Visibility.Hidden;


                //sinal sonoro
                SystemSounds.Beep.Play();

                //incrementa erro
                verro += 1;


                //diminui pontos
                Vpontos += -1;
                stpontos_nivel.Content = Vpontos.ToString();

                errado.Visibility = Visibility.Visible;

                vlbrepeticoes = Convert.ToInt32(lbrepeticao.Content);
            }

            //Teste colisão banana
            if (Canvas.GetTop(ellipse) - 25 == Canvas.GetTop(banana) && Canvas.GetLeft(ellipse) - 25 == Canvas.GetLeft(banana))
            {
                Canvas.SetLeft(banana, 0);
                Canvas.SetTop(banana, 0);

                banana.Visibility = Visibility.Hidden;


                //sinal sonoro
                SystemSounds.Beep.Play();

                //incrementa erro
                verro += 1;


                //diminui pontos
                Vpontos += -1;
                stpontos_nivel.Content = Vpontos.ToString();

                errado.Visibility = Visibility.Visible;

                vlbrepeticoes = Convert.ToInt32(lbrepeticao.Content);
            }


            //Teste colisão pera
            if (Canvas.GetTop(ellipse) - 25 == Canvas.GetTop(pera) && Canvas.GetLeft(ellipse) - 25 == Canvas.GetLeft(pera))
            {
                Canvas.SetLeft(pera, 0);
                Canvas.SetTop(pera, 0);

                pera.Visibility = Visibility.Hidden;


                //sinal sonoro
                SystemSounds.Beep.Play();

                //incrementa erro
                verro += 1;


                //diminui pontos
                Vpontos += -1;
                stpontos_nivel.Content = Vpontos.ToString();

                errado.Visibility = Visibility.Visible;

                vlbrepeticoes = Convert.ToInt32(lbrepeticao.Content);
            }

            //Teste colisão cereja
            if (Canvas.GetTop(ellipse) - 25 == Canvas.GetTop(cereja) && Canvas.GetLeft(ellipse) - 25 == Canvas.GetLeft(cereja))
            {
                Canvas.SetLeft(cereja, 0);
                Canvas.SetTop(cereja, 0);

                cereja.Visibility = Visibility.Hidden;


                //sinal sonoro
                SystemSounds.Beep.Play();

                //incrementa erro
                verro += 1;


                //diminui pontos
                Vpontos += -1;
                stpontos_nivel.Content = Vpontos.ToString();

                errado.Visibility = Visibility.Visible;

                vlbrepeticoes = Convert.ToInt32(lbrepeticao.Content);
            }


            //testa se tem muitos erros e bloqueia nivel


            //testar se número de erros maior que 30, então bloqueia nivel e cancela atividade
            if (verro >= 30)
            {


                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    TBSerie S = ctx.TBSerie.Find(Convert.ToInt32(txtidnivel.Text));
                    if (S != null)
                    {

                        S.Nivel_concluido = false;

                        S.Qt_acertos = vacerto;
                        S.Qt_erros = verro;
                        S.Score_nivel = Vpontos;
                        S.Nivel_liberado = false;


                        ctx.SaveChanges();

                    }
                }

                lblaviso.Content = "Exercício Bloqueado! Entre em contato com Fisioterapeuta";

                //cancela atual
                cancelar();
            }
        }

        //calculo ângulo
        private void calculo_angulo(Skeleton skeleton)
        {
            Joint maoesq_ang = skeleton.Joints[JointType.HandLeft];
      
            //ombro esquerdo
            Joint ombroesq_ang = skeleton.Joints[JointType.ShoulderLeft];


            // 2D coordinates in pixels
            Point pointm_ang = new Point();

            SkeletonPoint skeletonPoint = maoesq_ang.Position;

            DepthImagePoint depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            pointm_ang.X = depthPoint.X;
            pointm_ang.Y = depthPoint.Y;


            //distancia euclidiana
            Point pointomb_ang = new Point();
            SkeletonPoint skeletonPointombro = ombroesq_ang.Position;

            DepthImagePoint depthPointomb_ang = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPointombro, DepthImageFormat.Resolution640x480Fps30);

            pointomb_ang.X = depthPointomb_ang.X;
            pointomb_ang.Y = depthPointomb_ang.Y;


            //Posição X e Y do ombro e da mão
            double[] vombro_ang = { pointomb_ang.X, pointomb_ang.Y };
            double[] vmao_ang = { pointm_ang.X, pointm_ang.Y };

            tdombro.Text = vombro_ang[0].ToString() + " - " + vombro_ang[1].ToString();//+ " --- " + vombro[2].ToString();
            tdmao.Text = vmao_ang[0].ToString() + " - " + vmao_ang[1].ToString();//+ "

            double pombx = vombro_ang[0];

            angx_Copy.Text = pombx.ToString();
            double pomby = vombro_ang[1];

            angx_Copy3.Text = pomby.ToString();
            //double angulox = (raiosk * Math.Cos(90 * Math.PI / 180));// + pointomb.X;//+pos ombro x;
            //double anguloy = (raiosk * Math.Sin(90 * Math.PI / 180));// + pointomb.Y;


            // Distância Euclidiana entre(vombro, vmao);
            double raiosk = MathNet.Numerics.Distance.Euclidean(vombro_ang, vmao_ang);




            txtdistancia.Text = raiosk.ToString();

            //descobrir posição do angulo
            //x= raio * cos  angulo
            //y= raio * sen angulo


            //PEGAR VALOR DO ANGULO DA TELA
            double angulox = 0; //Math.Cos(90 * Math.PI / 180) * raiosk; //* Math.PI / 180  ;// + pointomb.X;//+pos ombro x;
            double anguloy = 0; //Math.Sin(90 * Math.PI / 180) * raiosk;//// * Math.PI) / 180) * raiosk;// + pointomb.Y;
                                //    angulox = (angulox + pointomb.X)/2;
                                //      anguloy = anguloy + pointomb.Y;

            //angx_Copy.Text = Math.Cos(90).ToString();
            //angy_Copy.Text = Math.Sin(90).ToString();


            ////angx_Copy2.Text = Math.Cos(90 * Math.PI).ToString();
            ////angy_Copy2.Text = Math.Sin(90 * Math.PI).ToString();


            //angx_Copy1.Text = angulox.ToString();
            //angy_Copy1.Text = anguloy.ToString();

            //double seno = Math.Cos(90 * Math.PI / 180);
            //double cosseno = Math.Sin(90 * Math.PI / 180);

            
            int vvang = 0;
          //  vvang = Convert.ToInt32(lbang.Text);
            vvang = Convert.ToInt32(lbangulo.Content);


            //realiza o cálculo da posição conforme o angulo parametrizado
            switch (vvang)
            {
                case 15:
                    angulox = 0.96592582628 * raiosk;
                    anguloy = 0.2588190451 * raiosk;
                    break;

                case 30:
                    angulox = 0.86602540378 * raiosk;
                    anguloy = 0.5 * raiosk;
                    break;

                case 45:
                    angulox = 0.70710678118 * raiosk;
                    anguloy = 0.70710678118 * raiosk;
                    break;

                case 60:
                    angulox = 0.5 * raiosk;
                    anguloy = 0.86602540378 * raiosk;
                    break;

                case 75:
                    angulox = 0.2588190451 * raiosk;
                    anguloy = 0.96592582628 * raiosk;
                    break;

                case 90:
                    angulox = 0 * raiosk;
                    anguloy = 1 * raiosk;
                    break;

                case 105:
                    angulox = -0.2588190451 * raiosk;
                    anguloy = 0.96592582628 * raiosk;
                    break;

                case 120:
                    angulox = -0.5 * raiosk;
                    anguloy = 0.86602540378 * raiosk;
                    break;

                case 135:
                    angulox = -0.70710678118 * raiosk;
                    anguloy = 0.70710678118 * raiosk;
                    break;

                case 150:
                    angulox = -0.86602540378 * raiosk;
                    anguloy = 0.5 * raiosk;
                    break;

                case 165:
                    angulox = -0.96592582628 * raiosk;
                    anguloy = 0.2588190451 * raiosk;
                    break;

                case 180:
                    angulox = -1 * raiosk;
                    anguloy = 0 * raiosk;
                    break;


            }


            angx_Copy1.Text = angulox.ToString();
            angy_Copy1.Text = anguloy.ToString();

            //nova posição da tela - MÃO ESQUERDA 

           // SE ÂNGULO 90 GRAUS
            if (vvang == 90)
            {
                vangulo_x = pombx - anguloy;
                vangulo_y = pomby + angulox;
            }
            else
            {
                vangulo_x = pombx - anguloy;
                vangulo_y = pomby + angulox;

                vangulo_x = vangulo_x - 25;
                vangulo_y = vangulo_y + 50;

            }
            if (vvang == 105)
            {
                vangulo_x = pombx - anguloy;
                vangulo_y = pomby - anguloy;

                 vangulo_x = vangulo_x-50;
                 vangulo_y = vangulo_y+75;

            }
            angx.Text = vangulo_x.ToString();
            angy.Text = vangulo_y.ToString();



            mostramaca = true;
            



            }

           
        //atualiza pontos
        private void BtScore_Click(object sender, RoutedEventArgs e)
        {
            pontos();
          
        }

        

        private void BBuscaID_Click(object sender, RoutedEventArgs e)
        {
            object item2 = dgDados.SelectedItem;

            txtidserie.Text = (dgDados.SelectedCells[7].Column.GetCellContent(item2) as TextBlock).Text;
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BtSeries_Click(sender, e);
            BBuscaID_Click(sender, e);

        }
    
   
        //finaliza atividade
        private void BtFinalizar_Click(object sender, RoutedEventArgs e)
        {

            salvar();


        }

        //cancela atividade
        private void BtCancelar_Click(object sender, RoutedEventArgs e)
        {
            cancelar();
        }

        private static double vectorNorm(double x, double y)//, double z)
        {

            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));// + Math.Pow(z, 2));

        }

        //some sinal certo da tela depois q acertou maca
        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
            certo.Visibility = Visibility.Hidden;
            //   maca.Visibility = Visibility.Hidden;
        

        }

        //tempo para sumir maça
        public void dispatcherTimer_Tick1(object sender, EventArgs e)
        {
            //  certo.Visibility = Visibility.Hidden;
            vacertoumaca = false;

            macaverde.Visibility = Visibility.Visible;
            maca.Visibility = Visibility.Visible;
            pera.Visibility = Visibility.Visible;
            banana.Visibility = Visibility.Visible;
            cereja.Visibility = Visibility.Visible;
        }

       

       
        // ***Desenhe os ossos e as articulações do esqueleto
        /// <param name="skeleton">squeleto para desenhar</param>
        /// <param name="drawingContext">drawing context to draw to</param>


        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Braço esquerdo
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Braço direito
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Perna esquerda
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Perna direita
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);


            // Articulações de renderização
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }

                
            }
        }

        /// <summary>
        /// Mapeia um SkeletonPoint para ficar dentro do nosso espaço de renderização e converte em Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Converte ponto em profundidade de espaço.
            // Não estamos usando profundidade diretamente, mas queremos os pontos em nossa resolução de saída de 640x480.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        // **** Desenhe uma linha de osso entre duas juntas
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>


        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {

            //   var  scaledjoint =scaledjoint
            //  var escalat = (640, 480, .5f, ///5f);

         //   Joint scaledjoint = Joint..scaleto(640, 480, .3f, .3f);

            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // Se não conseguirmos encontrar nenhuma dessas articulações, saia
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Não desenhe se ambos os pontos forem inferidos
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // Assumimos que todos os ossos extraídos são inferidos, a menos que ambas as articulações sejam rastreadas
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                //o ombro-cabeça "osso" irá pintar a cor desejada de acordo com o fato de o movimento estar correto ou não

                if (joint0 == skeleton.Joints[JointType.ShoulderCenter]) 
                    drawPen = this.trackedBonePenHead;
                else
                    drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Lida com a verificação ou desmarcação da caixa de combinação do modo sentado
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        // ****Ative ou desative o modo sentado

        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }

        /// <summary>
        /// O botão "Repetir" pressiona o método que reinicia as variáveis ​​que precisam fazer.

        /// com o movimento da cabeça para poder repetir o exercício sem ter que

        /// reiniciar el programa
        /// </summary>
        private void btnRepetir_Click(object sender, RoutedEventArgs e)
        {
            contFPS = 0;
            movFinalizado = false;
            lblFin.Visibility = Visibility.Hidden;
            lblInicio.Visibility = Visibility.Visible;
            esperaInicial = true;
        }


       


        //EXECUTA AÇÕES CONFORME COMANDOS DE VOZ
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            
            switch (e.Result.Semantics.Value.ToString())
            {
                case "INICIAR":
                    MessageBox.Show("inicia");
                    iniciar();
                break;

                case "SALVAR":
                    salvar();
                    break;

                case "PONTOS":
                  //  pontos();
                    break;

                case "CANCELAR":
                    cancelar();
                    break;
            }
        }

        private void cancelar()
        {
            //acertos  
            vacerto = 0;
            stacertos.Content = vacerto.ToString();

            //eros
            verro = 0;

            //ponto
            Vpontos = 0;
            stpontos_nivel.Content = Vpontos.ToString();



            viniciou = false;
            vfinalizou = true;

            btIniciar.IsEnabled = true;
            btFinalizar.IsEnabled = false;
            btCancelar.IsEnabled = false;

            lblaviso.Content = "Vamos lá!!! Tente Novamente!";
          
            lbaviso1.Visibility = Visibility.Hidden;
            this.sensor.AudioSource.Stop();
        }

        private void pontos()
        {
            object item2 = dgDados.SelectedItem;

            txtidserie.Text = (dgDados.SelectedCells[7].Column.GetCellContent(item2) as TextBlock).Text;
            try
            {

                using (FisioEntities3 ctx = new FisioEntities3())

                {
                    int vid = Convert.ToInt32(txtidserie.Text);
                    var consultascore = from S in ctx.TBSerie
                   
                                        where (S.ID_TR == vid)
                                        select new
                                        {
                                            Score = S.Score_nivel

                                        };


                    //MOSTRA TOTAL SCORE
                    int total = consultascore.Sum(S => S.Score.Value);
                    stpontos.Content = total.ToString();

                }

            }

            catch { }
        }

        //salvar exercício
        private void salvar()
        {
            //fazer pergunta se tem certeza que deseja encerrar a serie, 
                       
            viniciou = false;
            vfinalizou = true;

            btIniciar.IsEnabled = true;
            btFinalizar.IsEnabled = false;
            btCancelar.IsEnabled = false;

            vlbacertos = Convert.ToInt32(stacertos.Content);
            //   totaacertos = vlbacertos + vacerto;
            vlbrepeticoes = Convert.ToInt32(lbrepeticao.Content);

            
            //testar se num de repetições = acertos perguntar se quer finalizar
            if (vlbacertos >= vlbrepeticoes)
            {
                if (MessageBox.Show("Você atigingiu o número de repetições! Deseja finalizar a atividade?", "Confirmação", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //finaliza atividade


                    ////gravar score 
                    //Acertando todos os objetos do nível sem erro,  recebe como pontuação o número do nível x 10.
                    //Ex: Acertou todos do nível  1 sem erros recebe como pontuação 10 pontos.
                    //Se tiver mais de 2 erros, o score de nível é dividido por 2.



                    using (FisioEntities3 ctx = new FisioEntities3())
                    {
                        TBSerie S = ctx.TBSerie.Find(Convert.ToInt32(txtidnivel.Text));
                        if (S != null)
                        {

                            S.Nivel_concluido = true;

                            S.Qt_acertos = vacerto;
                            S.Qt_erros = verro;
                            S.Score_nivel = Vpontos;



                            ctx.SaveChanges();

                        }
                    }

                    lblaviso.Content = "Parabéns!!! Nível Concluído!";
                }
                else
                {
                    //salva atividade sem finalizar
                    using (FisioEntities3 ctx = new FisioEntities3())
                    {
                        TBSerie S = ctx.TBSerie.Find(Convert.ToInt32(txtidnivel.Text));
                        if (S != null)
                        {


                            S.Qt_acertos = vacerto;
                            S.Qt_erros = verro;
                            S.Score_nivel = Vpontos;



                            ctx.SaveChanges();

                        }
                    }

                    lblaviso.Content = "Não desista do seu objetivo!!!";
                }


            }
            else
            {

                //salva atividade sem finalizar
                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    TBSerie S = ctx.TBSerie.Find(Convert.ToInt32(txtidnivel.Text));
                    if (S != null)
                    {

                        S.Qt_acertos = vacerto;
                        S.Qt_erros = verro;
                        S.Score_nivel = Vpontos;
                                               
                        ctx.SaveChanges();

                    }
                }

                lblaviso.Content = "Não desista do seu objetivo!!!";
        
                lbaviso1.Visibility = Visibility.Hidden;
            }

                                         
            //atualizar meus pontos geral
            pontos();

            //limpa campos
            lbnivel.Content = "";
            lbposicao1.Content = "";
            lbposicao3.Content = "";
            lbangulo.Content = "";
            lbarticulacao.Content = "";
            lbrepeticao.Content = "";
            lbacertos.Content = "";
            lberro.Content = "";
            stacertos.Content = "";
            stpontos_nivel.Content = "";
            //refaz pesquisa

        }


        //ação botão iniciar
        private void iniciar()
        {
                    
            //se não tiver algum destes campos carregados, informa o usuario e não comeca a atividade
            if ((lbarticulacao.Content.ToString().Trim() == "") || (lbnivel.Content.ToString().Trim() == "") || (lbposicao1.Content.ToString().Trim() == "") || (lbangulo.Content.ToString().Trim() == ""))
            {
                MessageBox.Show("Verifique os campos Nível | Posição | Articulação | Ângulo");
            }
            else
            {
                lblaviso.Content = "";
                //angulo
                vangulo = 0;
                vangulo= Convert.ToInt32(lbangulo.Content);

                //acertos  
                vacerto = 0;
                stacertos.Content = vacerto.ToString();
                stacertos.Content = lbacertos.Content;

                vacerto = Convert.ToInt32(stacertos.Content);

                //eros
                verro = 0;

                //ponto
                Vpontos = 0;
                //  stpontos_nivel.Content = Vpontos.ToString();
                if (stpontos_nivel.Content.ToString().Trim() == "")
                {
                    Vpontos = 0;
                }
                else
                {
                    Vpontos = Convert.ToInt32(stpontos_nivel.Content);
                }


                viniciou = true;
                vfinalizou = false;

                btIniciar.IsEnabled = false;
                btFinalizar.IsEnabled = true;
                btCancelar.IsEnabled = true;


                lblaviso.Content = "Prepare-se o exercício vai começar";

                lbaviso1.Visibility = Visibility.Hidden;
                //animacao banana
                InitializeComponent();
                dt.Interval = TimeSpan.FromSeconds(10);
                dt.Tick += new EventHandler(dt_banana);
                dt.Start();

                //animacao pera
                InitializeComponent();
                dtp.Interval = TimeSpan.FromSeconds(18);
                dtp.Tick += new EventHandler(dt_pera);
                dtp.Start();

                //animacao cereja
                InitializeComponent();
                dtc.Interval = TimeSpan.FromSeconds(12);
                dtc.Tick += new EventHandler(dt_cereja);
                dtc.Start();

                //animacao maca
                InitializeComponent();
                dtm.Interval = TimeSpan.FromSeconds(6);
                dtm.Tick += new EventHandler(dt_maca);
                dtm.Start();
            }



            }


        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //   ClearRecognitionHighlights();
        }
    }
          
}





