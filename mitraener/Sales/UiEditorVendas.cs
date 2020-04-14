using System;
using Primavera.Extensibility.BusinessEntities.ExtensibilityService.EventArgs;
using Primavera.Extensibility.Sales.Editors;
using StdBE100;
using StdPlatBS100;

namespace mitraener.Sales
{
    public class UiEditorVendas : EditorVendas
    {

        String cliente = "";
        String artigo = "", documento = "";
       
        float pbase = 0, desconto = 0, comp = 0, pvp1 = 0;
        VndBE100.VndBELinhaDocumentoVenda linha = new VndBE100.VndBELinhaDocumentoVenda();

        public override void ValidaLinha(int NumLinha, ExtensibilityEventArgs e)
        {    



            StdBELista tblArtigoMoeda = new StdBELista();
            string artigoLinha = "";
            string armazemSugestao = "";
            string localSugestao = "";

            bool calcula = false;
            StdBELista tblArtigo = new StdBELista();
            StdBELista tblDocumento = new StdBELista();
            linha = this.DocumentoVenda.Linhas.GetEdita(NumLinha);

            try
            {
                documento = this.DocumentoVenda.Tipodoc;
                tblDocumento = BSO.Consulta("select ArmazemSugestao, LocalSugestao from SeriesVendas where TipoDoc = '" + documento + "'");


                //alterar armazem dos artigos na linha
                if (tblDocumento.Vazia() == false)
                {
                    armazemSugestao = tblDocumento.DaValor<string>("ArmazemSugestao");
                    localSugestao = tblDocumento.DaValor<string>("LocalSugestao");
 

                    string linhaval = linha.Armazem;
                    if (linhaval == null || linhaval == "" || linhaval == armazemSugestao)
                    {
                        linha.Armazem = armazemSugestao;
                        linha.Localizacao = localSugestao;
                    }
                }
        

            }
            catch ( Exception err)
            {
                PSO.Dialogos.MostraErro("[Consulta Documento]: " +  err.ToString());
                
            }

            try
            {
                cliente = this.DocumentoVenda.Entidade;
                artigoLinha = linha.Artigo;
                tblArtigo = BSO.Consulta("select * from Artigo where Artigo = '" + artigoLinha + "'");
                calcula = (bool) tblArtigo.Valor("CDU_Calculo");
                tblArtigoMoeda = BSO.Consulta("select * from ArtigoMoeda where Artigo = '" + artigoLinha + "'");
 
            }
            catch (Exception err)
            {
                PSO.Dialogos.MostraErro("[Consulta Artigo e ArtigoMoeda]: " + err.ToString());

            }








            StdBELista tblDesconto = null;
            float descValor = 0;

            if (tblArtigoMoeda != null)
            {
                pvp1 = tblArtigoMoeda.DaValor<float>("PVP1");

                float precBase = float.Parse( linha.CamposUtil["CDU_PrecBase"].Valor.ToString());
                if (precBase == 0 || precBase == pvp1)
                {
                    linha.CamposUtil["CDU_PrecBase"].Valor = pvp1;
                }
            }


            if (tblArtigo != null)
            {

                if ( calcula == true)
                {
                     artigo = artigoLinha;

                    try
                    {
                        tblDesconto = BSO.Consulta("select * from TDU_DescCliente where CDU_Cliente = '" + cliente + "' and CDU_Artigo = '" + artigo + "'");
                        if (tblDesconto.Vazia() == false)
                        {
                            descValor = tblDesconto.DaValor<float>("CDU_Desconto");
                        }else
                        {
                            descValor = 0;
                        }

                    }
                    catch (Exception err)
                    {
                        tblDesconto = null;
                        descValor = 0;
                        PSO.Dialogos.MostraErro( "[Consulta Desconto cliente]: " + err.ToString());

                    }





                    if (tblDesconto != null)
                    {


                        float linhaval =float.Parse(linha.CamposUtil["CDU_DescVal"].Valor.ToString());
                        if (linhaval == 0 || linhaval == descValor)
                        {
                            linha.CamposUtil["CDU_DescVal"].Valor = descValor;

                        }
                    }

                    getLinhaValores(NumLinha);

                    linha.PrecUnit = pbase - desconto - comp;



                }

            }


            base.ValidaLinha(NumLinha, e);

        }


        void getLinhaValores(int NumLinha)
        {
            linha = this.DocumentoVenda.Linhas.GetEdita(NumLinha);

            pbase = float.Parse(linha.CamposUtil["CDU_PrecBase"].Valor.ToString());
            desconto = float.Parse(linha.CamposUtil["CDU_DescVal"].Valor.ToString());
            comp = float.Parse(linha.CamposUtil["CDU_CompNT"].Valor.ToString());
        }

    }
}
