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
    /// <summary>
    /// Lógica interna para User.xaml
    /// </summary>
    public partial class User : Window
    {
          private string operacao;

        public User()
        {
            InitializeComponent();
        }



        private void BtSalvar(object sender, RoutedEventArgs e)
        {

            //gravar no banco de dados
            if (operacao == "inserir")
            {
                //dados do usuario da tela
                TBUser U = new TBUser();
                U.Nome = txtNome.Text;
                U.Email = txtEmail.Text;
                U.Fone = txtFone.Text;
                U.Login = txtLogin.Text;
                U.Senha = txtSenha.Password;
                U.Ativo = checkAtivo.IsChecked;
                U.Tipo = txttipo.Text;



                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    ctx.TBUser.Add(U);
                    ctx.SaveChanges();
                }
            }
            if (operacao == "alterar")
            {
                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    TBUser U = ctx.TBUser.Find(Convert.ToInt32(txtID.Text));
                    if (U != null)
                    {
                        U.Nome = txtNome.Text;
                        U.Email = txtEmail.Text;
                        U.Fone = txtFone.Text;
                        U.Login = txtLogin.Text;
                        U.Senha = txtSenha.Password;
                        U.Ativo = checkAtivo.IsChecked;
                        U.Tipo = txttipo.Text;
                        ctx.SaveChanges();

                    }
                }
            }

            this.ListarUser();
            this.AlteraBotoes(1);
            this.LimpaCampos();

        }

      
        private void BtIncluir(object sender, RoutedEventArgs e)
        {
            this.operacao = "inserir";
            this.AlteraBotoes(2);
            txtID.Text = "";
            txtID.IsEnabled = false;
            this.LimpaCampos();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //dados do usuario da tela
            TBUser U = new TBUser();
            U.Nome = txtNome.Text;
            U.Email = txtEmail.Text;

            //gravar no banco de dados
            using (FisioEntities3 ctx = new FisioEntities3())
            {

                ctx.TBUser.Add(U);
                ctx.SaveChanges();


            }


        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
          this.ListarUser();
          this.AlteraBotoes(1);
        }

        //pesquisa usuários
        private void ListarUser()
        {
            using (FisioEntities3 ctx = new FisioEntities3())
            {
                int a = ctx.TBUser.Count();
                lbQTDUser.Content = "Quantidade de Usuários:" + a.ToString(); ;


                var consulta = from U in ctx.TBUser
                               where U.Nome.Contains(txtNome.Text)
                               select new { U.ID, U.Nome, U.Tipo, U.Email, U.Fone, U.Login, U.Ativo };


                dgDados.ItemsSource = consulta.ToList();
            }

        }

        private void AlteraBotoes(int op)
        {
            btEditar.IsEnabled = false;
            btInserir.IsEnabled = false;
            btCancelar.IsEnabled = false;
            btPesquisar.IsEnabled = false;
            btExcluir.IsEnabled = false;
            btSalvar.IsEnabled = false;

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

        //limpar campos
        private void LimpaCampos()
        {
            txtID.Clear();
            txtNome.Clear();
            txtEmail.Clear();
            txtFone.Clear();
            txtLogin.Clear();
            txtSenha.Password = "";
            checkAtivo.IsChecked = false;
            txtID.IsEnabled = true;




        }

        //cancelar
        private void BtCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.AlteraBotoes(1);
            this.LimpaCampos();

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
                        TBUser U = ctx.TBUser.Find(id);
                        dgDados.ItemsSource = new TBUser[1] { U };
                    }

                }
                catch { }
            }

            //procura pelo nome
            if (txtNome.Text.Trim().Count() > 0)
            {
                try
                {

                    using (FisioEntities3 ctx = new FisioEntities3())

                    {
                        var consulta = from U in ctx.TBUser
                                       where U.Nome.Contains(txtNome.Text)
                                       select U;


                        dgDados.ItemsSource = consulta.ToList();
                    }
                }
                catch { }
            }
        }

        private void BtEditar_Click(object sender, RoutedEventArgs e)
        {
            this.operacao = "alterar";
            this.AlteraBotoes(2);
            //txtID.Text = "";
            txtID.IsEnabled = false;


        }

        //carrega compos da pesquisa para tela
        private void DgDados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgDados.SelectedIndex >= 0)
            {

                object item = dgDados.SelectedItem;
                txtID.IsEnabled = false;
                
                //   TBUser U = (TBUser)dgDados.SelectedItem;
                txtFone.Text = (dgDados.SelectedCells[4].Column.GetCellContent(item) as TextBlock).Text;
                txtID.Text = (dgDados.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                txtEmail.Text = (dgDados.SelectedCells[3].Column.GetCellContent(item) as TextBlock).Text;
                txtNome.Text = (dgDados.SelectedCells[1].Column.GetCellContent(item) as TextBlock).Text;
                txtLogin.Text = (dgDados.SelectedCells[5].Column.GetCellContent(item) as TextBlock).Text;
                txtSenha.Password = (dgDados.SelectedCells[6].Column.GetCellContent(item) as TextBlock).Text;

              
            }

            this.AlteraBotoes(3);


        }

        //excluir
        private void BtExcluir_Click(object sender, RoutedEventArgs e)
        {
            using (FisioEntities3 ctx = new FisioEntities3())

            {
                TBUser U = ctx.TBUser.Find(Convert.ToInt32(txtID.Text));
                if (U != null)
                {
                    ctx.TBUser.Remove(U);
                    ctx.SaveChanges();
                }
            }

            this.ListarUser();
            this.AlteraBotoes(1);
            this.LimpaCampos();
        }

    }
}

