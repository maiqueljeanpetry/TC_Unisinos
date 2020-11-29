using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{

    public partial class Acompanhamento : Window
    {
        public Acompanhamento()
        {
            InitializeComponent();
        }

        private void BtSeries_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                using (FisioEntities3 ctx = new FisioEntities3())

                //busca todos tratamentos que estão concluídos
                {
                    var consulta = from S in ctx.TBSerie
                                   join T in ctx.TBTratamento on S.ID_TR equals T.ID_TR
                                   join U in ctx.TBUser on T.ID_User_PC equals U.ID


                                   where (U.Nome.Contains(txtNome.Text)
                                           && (S.Nivel_concluido == true) )
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


                    //mostra valores na tela
                    int total = consulta.Sum(S => S.Score.Value);
                    int acerto = consulta.Sum(S => S.Qt_Acertos.Value);
                    int erro = consulta.Sum(S => S.Qt_Erros.Value);

                    lpontos.Content = total.ToString();
                    lacertos.Content = acerto.ToString();
                    lerros.Content = erro.ToString();


                }

            }

            catch { }

        }

    }

}
