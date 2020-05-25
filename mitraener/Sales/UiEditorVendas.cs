using System;
using System.Collections.Generic;
using System.Linq;
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
        double quantidade = 0;
        float pbase = 0, desconto = 0, comp = 0, pvp1 = 0;
        VndBE100.VndBELinhaDocumentoVenda linha = new VndBE100.VndBELinhaDocumentoVenda();
        //ArtigoConjunto[] lista_artigo = new ArtigoConjunto[100];
        List<ArtigoConjunto> lista_artigo = new List < ArtigoConjunto >();
        ArtigoConjunto artigo_conjunto = new ArtigoConjunto();






        public override void ArtigoIdentificado(string Artigo, int NumLinha, ref bool Cancel, ExtensibilityEventArgs e)
        {

            bool inserido = false;
            for (int i = 0; i < lista_artigo.Count; i++)
            {
                if (lista_artigo[i].artigo == Artigo)
                {
                    inserido = true;
                    PSO.Dialogos.MostraAviso("O Artigo " + Artigo + " Ja foi inserido!");
                    Cancel = true;
                }
            }

            if (inserido == false) { 

            if (isArtigoConjunto(Artigo) == true)
            {
                    artigo_conjunto = new ArtigoConjunto(); ;
                artigo_conjunto.artigo = Artigo;
                artigo_conjunto.descricao = this.DocumentoVenda.Linhas.GetEdita(NumLinha).Descricao;
                artigo_conjunto.linha_inicial = NumLinha;
                StdBELista tblArtigo = new StdBELista();

                try
                {
                    tblArtigo = BSO.Consulta("select count(*) as total from ComponentesArtigos  where ArtigoComposto =  '" + Artigo + "'");


                    //alterar armazem dos artigos na linha
                    if (tblArtigo.Vazia() == false)
                    {
                        artigo_conjunto.total_linha = tblArtigo.DaValor<int>("total");
                        artigo_conjunto.linha_final = artigo_conjunto.linha_inicial + artigo_conjunto.total_linha;
                        lista_artigo.Add(artigo_conjunto);


                    }


                }
                catch (Exception err)
                {
                    PSO.Dialogos.MostraErro("[ArtigoIdentificado]: " + err.ToString());

                }
            }
        }
            base.ArtigoIdentificado(Artigo, NumLinha, ref Cancel, e);
        }
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
                    quantidade = linha.Quantidade;
                    if ( armazemSugestao.Length > 0)
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

            int size = this.DocumentoVenda.Linhas.NumItens;

            
            for ( int x = 0; x < lista_artigo.Count; x++)
            {
                ArtigoConjunto a = lista_artigo[x];
            

                if (NumLinha >= a.linha_inicial && NumLinha <= a.linha_final) {


                    if (size >= a.linha_final)
                    {
                        for (int i = a.linha_inicial; i <= a.linha_final; i++)
                        {
                            artigoLinha = DocumentoVenda.Linhas.GetEdita(i).Artigo;
                            DocumentoVenda.Linhas.GetEdita(i).Quantidade = quantidade;

                        }
                    }
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


        bool isArtigoConjunto(String artigo)
        {

            StdBELista tblArtigo = new StdBELista();
            bool rv = false;
            try
            {
                tblArtigo = BSO.Consulta("select TipoComponente from Artigo where Artigo = '" + artigo + "'");


                //alterar armazem dos artigos na linha
                if (tblArtigo.Vazia() == false)
                {
                    rv = tblArtigo.DaValor<int>("TipoComponente") == 1 ? true : false;
                    
            
                }


            }
            catch (Exception err)
            {
                PSO.Dialogos.MostraErro("[Consulta Artigo]: " + err.ToString());
                rv = false;

            }

        

            return rv;
        }



        //Limpar a lista de artigos depois e antes da venda
        public override void TipoDocumentoIdentificado(string Tipo, ref bool Cancel, ExtensibilityEventArgs e)
        {
            lista_artigo.Clear();
            base.TipoDocumentoIdentificado(Tipo, ref Cancel, e);
        }
        public override void ErroAoGravar(string Filial, string Tipo, string Serie, int NumDoc, ExtensibilityEventArgs e)
        {
            lista_artigo.Clear();
            base.ErroAoGravar(Filial, Tipo, Serie, NumDoc, e);
        }

        public override void DepoisDeGravar(string Filial, string Tipo, string Serie, int NumDoc, ExtensibilityEventArgs e)
        {
            lista_artigo.Clear();
            base.DepoisDeGravar(Filial, Tipo, Serie, NumDoc, e);
        }
        public override void ClienteIdentificado(string Cliente, ref bool Cancel, ExtensibilityEventArgs e)
        {
            lista_artigo.Clear();

            base.ClienteIdentificado(Cliente, ref Cancel, e);
        }



    }
}
