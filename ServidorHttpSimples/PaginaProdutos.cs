using System.Text;

namespace ServidorHttpSimples
{
    public class PaginaProdutos : PaginaDinamica
    {
        public override byte[] Get(SortedList<string, string> parametros)
        {
            string codigo = parametros.ContainsKey("id")?
                parametros["id"] : "";

            StringBuilder htmlGerado = new();
            foreach (var p in Produto.Listagem)
            {
                bool negrito = (!string.IsNullOrEmpty(codigo) && codigo == p.Codigo.ToString());
                htmlGerado.Append("<tr>");
                if (negrito)
                {
                    htmlGerado.Append($"<td><strong>{p.Codigo:D4}</strong></td>");
                    htmlGerado.Append($"<td><strong>{p.Nome}</strong></td>");
                }
                else
                {
                    htmlGerado.Append($"<td>{p.Codigo:D4}</td>");
                    htmlGerado.Append($"<td>{p.Nome}</td>");
                }
                htmlGerado.Append("</tr>");
            }

            string textoHtmlGerado = HtmlModelo.Replace("{{HtmlGerado}}", htmlGerado.ToString());
            return Encoding.UTF8.GetBytes(textoHtmlGerado);
        }
    }
}