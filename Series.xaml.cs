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

    public partial class Series : Window
    {
        private object text;
        //acao botao
        private string operacao;


        public Series()
        {
            InitializeComponent();
                                 
        }

        public Series(string valor, string valor2)
        {
            InitializeComponent();

            txtparam.Text = valor;
            txtparam_nome.Text = valor2;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BtIncluir(object sender, RoutedEventArgs e)
        {
            this.operacao = "inserir";
            this.AlteraBotoes(2);

            txtIDSerie.IsEnabled = false;
            this.LimpaCampos();
        }

        private void BtEditar_Click(object sender, RoutedEventArgs e)
        {
            this.operacao = "alterar";
            this.AlteraBotoes(2);
            //txtID.Text = "";
            txtIDSerie.IsEnabled = false;
        }

        private void BtSalvar(object sender, RoutedEventArgs e)
        {

            //gravar no banco de dados
            if (operacao == "inserir")
            {

                //dados do usuario da tela
                TBSerie S = new TBSerie();
                S.Angulo = Convert.ToInt32(txtAngulo.Text);
                S.ID_TR = Convert.ToInt32(txtparam.Text);
                S.Nivel = Convert.ToInt32(txtNivel.Text);
                S.Tipo_movimento = txtTipo_mov.Text;
                S.Articulacao = txtArticulacao.Text;
                S.Posicao = txtPosicao.Text;
                S.Qt_rep_nivel = Convert.ToInt32(txtRepeticoes.Text);
                S.Nivel_concluido = checkNivel_Concluido.IsChecked;
                S.Nivel_liberado = checkNivelLib.IsChecked;



                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    ctx.TBSerie.Add(S);
                    ctx.SaveChanges();
                }
            }
            if (operacao == "alterar")
            {
                using (FisioEntities3 ctx = new FisioEntities3())
                {
                    TBSerie S = ctx.TBSerie.Find(Convert.ToInt32(txtIDSerie.Text));
                    if (S != null)
                    {
                        S.Angulo = Convert.ToInt32(txtAngulo.Text);
                        S.Nivel = Convert.ToInt32(txtNivel.Text);
                        S.Angulo = Convert.ToInt32(txtAngulo.Text);
                        S.Tipo_movimento = txtTipo_mov.Text;
                        S.Articulacao = txtArticulacao.Text;
                        S.Posicao = txtPosicao.Text;
                        S.Qt_rep_nivel = Convert.ToInt32(txtRepeticoes.Text);
                        S.Nivel_concluido = checkNivel_Concluido.IsChecked;
                        S.Nivel_liberado = checkNivelLib.IsChecked;

                        ctx.SaveChanges();

                    }
                }
            }

            this.ListarUser();
            this.AlteraBotoes(1);
            this.LimpaCampos();

        }

        //excluir
        private void BtExcluir_Click(object sender, RoutedEventArgs e)
        {
            using (FisioEntities3 ctx = new FisioEntities3())

            {
                TBSerie U = ctx.TBSerie.Find(Convert.ToInt32(txtIDSerie.Text));
                if (U != null)
                {
                    ctx.TBSerie.Remove(U);
                    ctx.SaveChanges();
                }
            }

            this.ListarUser();
            this.AlteraBotoes(1);
            this.LimpaCampos();
        }

        //pesquisa
        private void BtPesquisar_Click(object sender, RoutedEventArgs e)
        {
            using (FisioEntities3 ctx = new FisioEntities3())

                //busca todos niveis do tratamento
                try
            {
                    int id = Convert.ToInt32(txtparam.Text);
                    var consulta = from S in ctx.TBSerie
                               where (S.ID_TR==id)
                               select S ;

                dgDados.ItemsSource = consulta.ToList();

            }

            catch { }

    }

        private void BtCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.AlteraBotoes(1);
            this.LimpaCampos();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ListarUser();
            this.AlteraBotoes(1);
        }

        private void ListarUser()
        {
            using (FisioEntities3 ctx = new FisioEntities3())
            {
               
                var consulta = ctx.TBSerie;
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

        private void LimpaCampos()
        {
            txtIDSerie.Clear();
            txtAngulo.Text = "";
            txtArticulacao.Text = "";
            txtNivel.Text= "";
            txtPosicao.Text = "";
            txtRepeticoes.Clear();
            txtTipo_mov.Text = "";
            checkNivelLib.IsChecked = false;
            checkNivel_Concluido.IsChecked = false;

        }

        //carrega série selecionada para a tela
        private void DgDados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgDados.SelectedIndex >= 0)
            {
                txtIDSerie.IsEnabled = false;

                TBSerie S = (TBSerie)dgDados.SelectedItem;

                txtIDSerie.Text = S.ID.ToString();
                txtAngulo.Text = S.Angulo.ToString();
                txtNivel.Text = S.Nivel.ToString(); 
                txtAngulo.Text = S.Angulo.ToString(); 
               // txtPosicao.Text = S.Posicao;
                txtRepeticoes.Text= S.Qt_rep_nivel.ToString();
                txtTipo_mov.Text = S.Tipo_movimento;
                txtart.Text= S.Articulacao;
                txtpos.Text = S.Posicao;
                S.Posicao = txtPosicao.Text;
                checkNivel_Concluido.IsChecked= S.Nivel_concluido;
                checkNivelLib.IsChecked = S.Nivel_liberado;


                this.AlteraBotoes(3);


            }
        }

    }
}
