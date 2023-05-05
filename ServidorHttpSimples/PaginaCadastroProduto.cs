using System.Text;

namespace ServidorHttpSimples
{
    public class PaginaCadastroProduto : PaginaDinamica
    {
        public override byte[] Post(SortedList<string, string> parametros)
        {
            Produto p = new();
            p.Codigo = parametros.ContainsKey("codigo") ?
                Convert.ToInt32(parametros["codigo"]) : 0;
            p.Nome = parametros.ContainsKey("nome") ?
                parametros["nome"] : "";
            if(p.Codigo > 0) Produto.Listagem.Add(p);
            string html = "<script>window.location.replace(\"produtos.dhtml\")</script>";
            return Encoding.UTF8.GetBytes(html);
        }
    }
}
