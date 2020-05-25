using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace mitraener.Sales
{
   public class ArtigoConjunto
    {
        public String artigo { get; set; }
        public String descricao { get; set; }


        public int[] linhas { get; set; }
        public int linha_inicial { get; set; }
        public int linha_final { get; set; }
        public int total_linha { get; set; }
        public ArtigoConjunto()
        {
            {
                this.artigo = null;
                this.descricao = null;
                this.linhas = linhas;
                this.linha_inicial = 0;
                this.linha_final = 0;
                this.total_linha = 0;
            }
        }

        public ArtigoConjunto(String artigo, String descricao, int[] linhas, int inicio, int fim, int totalLinha)
        {
            this.artigo = artigo;
            this.descricao = descricao;
            this.linhas = linhas;
            this.linha_inicial = inicio;
            this.linha_final = fim;
            this.total_linha = totalLinha;

        }

        public String info()
        {
            String descricao = "artigo " + this.artigo + "\n ";
            descricao += "descricao " + this.descricao + "\n ";
            descricao += "linha_inicial " + this.linha_inicial + "\n ";
            descricao += "linha_final " + this.linha_final + "\n ";
            descricao += "total_linha " + this.total_linha + "\n ";


            return descricao;
        }

    }
}
