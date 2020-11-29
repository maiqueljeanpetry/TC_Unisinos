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

    public partial class Tratamento : Window
    {
        //acao botao
        private string operacao;
        private int vcod_paciente;
        private int vcod_fisio;
        private string opbotao;



        public Tratamento()
        {
            InitializeComponent();
            wpanel.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new MainWindow();
            newWindow.Show();
        }

        //pesquisar
        private void BtPesquisar_Click(object sender, RoutedEventArgs e)
        {
            if (txtID.Text.Trim().Count() > 0)
            {
                try
                {

                    //busca pelo codigo
                    int id = Convert.ToInt32(txtID.Text);
                    using (FisioEntities3 ctx = new FisioEntities3())

                    {
                        //   var consulta = ctx.TBUser;
                        //  dgDados.ItemsSource = consulta.ToList();
                        //procura elemento pelo codigo - substitui select
                        TBTratamento U = ctx.TBTratamento.Find(id);
                        dgDados.ItemsSource = new TBTratamento[1] { U };
                    }




                }
                catch { }
            }

            //procura pelo nome
            if (txtPaciente.Text.Trim().Count() > 0)
            {
                try
                {

                    using (FisioEntities3 ctx = new FisioEntities3())


                    {
                        var consulta = from T in ctx.TBTratamento
                                       join U in ctx.TBUser on T.ID_User_PC equals U.ID

                                       join XU in ctx.TBUser on T.ID_User_FS equals XU.ID



                                       where ((U.Nome.Contains(txtPaciente.Text))
                                       // && (T.ID_User_PC == U.ID)
                                       )
                                       select new
                                       {
                                           Id = T.ID_TR,
                                           ID_Paciente = T.ID_User_PC,
                                           Paciente = U.Nome,
                                           ID_Fisio = T.ID_User_FS,
                                           Fisio = XU.Nome,
                                           Patologia = T.Patologia,
                                           Objetivo = T.Objetivo,
                                           Observação = T.Observacao,
                                           Ativo = T.Ativo
                                         
                                       };


                            dgDados.ItemsSource = consulta.ToList();

                    }

                }

               catch { }
            }


            //se não tiver nada solucionado
            if (txtPaciente.Text == "" && txtFisio.Text=="")
            {
                try
                {

                    using (FisioEntities3 ctx = new FisioEntities3())


                    {
                        var consulta = from T in ctx.TBTratamento
                                       join U in ctx.TBUser on T.ID_User_PC equals U.ID

                                       join XU in ctx.TBUser on T.ID_User_FS equals XU.ID
                                       select new
                                       {
                                           Id = T.ID_TR,
                                           ID_Paciente = T.ID_User_PC,
                                           Paciente = U.Nome,
                                           ID_Fisio = T.ID_User_FS,
                                           Fisio = XU.Nome,
                                           Patologia = T.Patologia,
                                           Objetivo = T.Objetivo,
                                           Observação = T.Observacao,
                                           Ativo = T.Ativo

                                       };


                        dgDados.ItemsSource = consulta.ToList();

                    }

                }

                catch { }
            }
        }

        //pequisa fisioterapeuta
        private void BtBuscaFisio_Click(object sender, RoutedEventArgs e)
        {
            opbotao = "Fisio";
            using (FisioEntities3 ctx = new FisioEntities3())

            {
                var consulta = from U in ctx.TBUser
                               where U.Tipo.Contains("Fisioterapeuta")
                               select new { U.ID, U.Nome };
           
                dgFisio.ItemsSource = consulta.ToList();
           
            }


            wpanel.Visibility = Visibility.Visible;
        }

        //pesquisa paciente
        private void BtBuscaPaciente_Click(object sender, RoutedEventArgs e)
        {
            opbotao = "Paciente";

            using (FisioEntities3 ctx = new FisioEntities3())

            {
                var consulta = from U in ctx.TBUser
                               where U.Tipo.Contains("Paciente")
                               select new { U.ID, U.Nome };


                dgFisio.ItemsSource = consulta.ToList();

            }

            wpanel.Visibility = Visibility.Visible;
        }

        //salvar tratamento
        private void BtSalvar(object sender, RoutedEventArgs e)
        {
            //gravar no banco de dados
            if (operacao == "inserir")
            {
                //dados do usuario da tela
                TBTratamento U = new TBTratamento();


                U.Ativo = checkAtivo.IsChecked;
                U.Objetivo = txtObjetivo.Text;
                U.Patologia = txtPatologia.Text;
                U.Observacao = txtObs.Text;
                U.ID_User_FS = Convert.ToInt32(txtID_Fisio.Text);
                U.ID_User_PC = Convert.ToInt32(txtID_Paciente.Text);



                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    ctx.TBTratamento.Add(U);
                    ctx.SaveChanges();
                }
            }
            if (operacao == "alterar")
            {
                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    TBTratamento U = ctx.TBTratamento.Find(Convert.ToInt32(txtID.Text));
                    if (U != null)
                    {
                        U.Ativo = checkAtivo.IsChecked;
                        U.Objetivo = txtObjetivo.Text;
                        U.Patologia = txtPatologia.Text;
                        U.Observacao = txtObs.Text;
                        U.ID_User_FS = Convert.ToInt32(txtID_Fisio.Text);
                        U.ID_User_PC = Convert.ToInt32(txtID_Paciente.Text);
                        ctx.SaveChanges();

                    }
                }
            }

            this.ListarUser();
            this.AlteraBotoes(1);
            this.LimpaCampos();

        }

        private void ListarUser()
        {
            using (FisioEntities3 ctx = new FisioEntities3())
            {
                int a = ctx.TBTratamento.Count();
           //     lbQTDUser.Content = "Quantidade de Tratamentos:" + a.ToString(); ;
                var consulta = ctx.TBTratamento;
                dgDados.ItemsSource = consulta.ToList();
            }

        }


        private void BtIncluir(object sender, RoutedEventArgs e)
        {
            this.operacao = "inserir";
            this.AlteraBotoes(2);
            txtID.Text = "";
            txtID.IsEnabled = false;
            this.LimpaCampos();
        }

        private void BtEditar_Click(object sender, RoutedEventArgs e)
        {
            this.operacao = "alterar";
            this.AlteraBotoes(2);
            //txtID.Text = "";
            txtID.IsEnabled = false;
        }

        //escluir
        private void BtExcluir_Click(object sender, RoutedEventArgs e)
        {
            using (FisioEntities3 ctx = new FisioEntities3())

            {
                TBTratamento U = ctx.TBTratamento.Find(Convert.ToInt32(txtID.Text));
                if (U != null)
                {
                    ctx.TBTratamento.Remove(U);
                    ctx.SaveChanges();
                }
            }

            this.ListarUser();
            this.AlteraBotoes(1);
            this.LimpaCampos();
        }

        private void BtCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.AlteraBotoes(1);
            this.LimpaCampos();
        }

        private void AlteraBotoes(int op)
        {
            btEditar.IsEnabled = false;
            btInserir.IsEnabled = false;
            btCancelar.IsEnabled = false;
            btPesquisar.IsEnabled = false;
            btExcluir.IsEnabled = false;
            btSalvar.IsEnabled = false;
            btSeries.IsEnabled = false;

            if (op == 1)
            {
                //ativar opcoes iniciais
                btInserir.IsEnabled = true;
                btPesquisar.IsEnabled = true;
            }

            if (op == 2)
            {
                //inserir valor
                btCancelar.IsEnabled = true;
                btSalvar.IsEnabled = true;
            }
            if (op == 3)
            {
                ////localizar
                btEditar.IsEnabled = true;
                btExcluir.IsEnabled = true;
            }

        }



        private void LimpaCampos()
        {
            txtID.Clear();
            txtPaciente.Clear();
            txtPatologia.Clear();
            checkAtivo.IsChecked = false;
            txtID.IsEnabled = true;
            txtObs.Clear();
            txtPatologia.Clear();
            vcod_fisio = 0;
            vcod_paciente = 0;
            txtID_Fisio.Clear();
            txtID_Paciente.Clear();
            txtFisio.Clear();
            txtObjetivo.Clear();
            txtDtnasc.Clear();
        }

        //carrega para tela dado selecionado
        private void DgFisio_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (opbotao == "Fisio")
            {
                object item = dgFisio.SelectedItem;
                txtID_Fisio.Text = (dgFisio.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                txtFisio.Text = (dgFisio.SelectedCells[1].Column.GetCellContent(item) as TextBlock).Text;
            }

            if (opbotao == "Paciente")
            {
                object item = dgFisio.SelectedItem;
                txtID_Paciente.Text = (dgFisio.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                txtPaciente.Text = (dgFisio.SelectedCells[1].Column.GetCellContent(item) as TextBlock).Text;
            }


            wpanel.Visibility = Visibility.Hidden;
            //message box exemplo
            //string ID = (dgFisio.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
            //MessageBox.Show(ID);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ListarUser();
            this.AlteraBotoes(1);

        }


        private void DgDados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
          { if (dgDados.SelectedIndex >= 0)
            {
                object item = dgDados.SelectedItem;

                txtID.IsEnabled = false;
                txtID.Text = (dgDados.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                txtID_Paciente.Text = (dgDados.SelectedCells[1].Column.GetCellContent(item) as TextBlock).Text;
                txtPaciente.Text = (dgDados.SelectedCells[2].Column.GetCellContent(item) as TextBlock).Text;
                txtID_Fisio.Text = (dgDados.SelectedCells[3].Column.GetCellContent(item) as TextBlock).Text;
                txtFisio.Text = (dgDados.SelectedCells[4].Column.GetCellContent(item) as TextBlock).Text;
                txtPatologia.Text = (dgDados.SelectedCells[5].Column.GetCellContent(item) as TextBlock).Text;
                txtObjetivo.Text = (dgDados.SelectedCells[6].Column.GetCellContent(item) as TextBlock).Text;
                txtObs.Text = (dgDados.SelectedCells[7].Column.GetCellContent(item) as TextBlock).Text;
               string teste = (dgDados.SelectedCells[8].Column.GetCellContent(item) as TextBlock).Text;
                    checkAtivo.IsChecked = (bool)true;
                }
                else
                {

                    checkAtivo.IsChecked = false;
                }
            
               
            
            }

            this.AlteraBotoes(3);
            btSeries.IsEnabled = true;
        }



        private void BtSeries_Click_1(object sender, RoutedEventArgs e)
        {
            if (dgDados.SelectedIndex >= 0)
            {
                //se selecionou abre o cadastro de series passando o ID_Tratamento como Parametro
                var newWindow = new Series(txtID.Text,txtPaciente.Text);
                newWindow.Show();
        
            }
            //desabilita botao
            btSeries.IsEnabled = false;
        }
    }
}
