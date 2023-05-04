using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServidorHttpSimples
{
    public class ServidorHttp
    {   
        private TcpListener Controlador { get; set; } // escuta uma porta de rede a procura de alguma solicitação de conexão TCP
        private int Porta { get; set; }
        private int QtdRequests { get; set; } // contador de requisições
        private SortedList<string, string> TiposMime { get; set; }

        private SortedList<string, string> DiretoriosHost { get; set; }

        public ServidorHttp(int porta = 8080)
        {
            Porta = porta;
            TiposMimeAdd();
            MapearDiretoriosHost();
            try
            {
                Controlador = new TcpListener(IPAddress.Parse("127.0.0.1"), Porta);
                Controlador.Start();
                Console.WriteLine("O Servidor HTTP está rodando.");
                Console.WriteLine($"Para acessar, digite no navegador: http://localhost:{Porta}");
                Task servidorHttpTask = Task.Run(() => AguardarRequests());
                servidorHttpTask.GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao iniciar o servidor na porta {Porta}:\n{e.Message}");
            }
        }
        private async Task AguardarRequests() // Método assíncrono
        {
            while (true)
            {
                Socket conexao = await Controlador.AcceptSocketAsync(); // retorna os dados da conexão
                QtdRequests++;
                Task task = Task.Run(() => ProcessarRequest(conexao, QtdRequests));
            }
        }

        private void ProcessarRequest(Socket conexao, int numeroRequest)
        {
            Console.WriteLine($"Processando request #{numeroRequest}...\n");
            if (conexao.Connected)
            {
                byte[] bytesRequisicao = new byte[1024]; // espaço reservado para armazenar os dados da conexão
                conexao.Receive(bytesRequisicao, bytesRequisicao.Length, 0); // guardando os dados no vetor
                string textoRequisicao = Encoding.UTF8.GetString(bytesRequisicao).Replace((char)0, ' ').Trim(); // Eliminando os zeros das posições do vetor que não foram ocupados.
                if(textoRequisicao.Length > 0 )
                {
                    Console.WriteLine($"\n{textoRequisicao}\n");

                    string[] linhas = textoRequisicao.Split("\r\n");
                    int primeiroEspaco = linhas[0].IndexOf(" ");
                    int segundoEspaco = linhas[0].LastIndexOf(" ");
                    string metodoHttp = linhas[0].Substring(0, primeiroEspaco);
                    string recursoBuscado = linhas[0].Substring(primeiroEspaco + 1, segundoEspaco - primeiroEspaco - 1);
                    if (recursoBuscado == "/") recursoBuscado = "/index.html";
                    string textoParametros = recursoBuscado.Contains("?") ?
                        recursoBuscado.Split("?")[1] : "";
                    SortedList<string, string> parametros = ProcessarParametros(textoParametros);
                    recursoBuscado = recursoBuscado.Split("?")[0];
                    string versaoHttp = linhas[0].Substring(segundoEspaco + 1);
                    primeiroEspaco  = linhas[1].IndexOf(" ");
                    string nomeHost = linhas[1].Substring(primeiroEspaco + 1);
                    byte[] bytesCabecalho = null;
                    byte[] bytesConteudo = null;
                    FileInfo arquivo = new(ObterDiretorio(nomeHost, recursoBuscado));
                    if (arquivo.Exists)
                    {
                        if (TiposMime.ContainsKey(arquivo.Extension.ToLower()))
                        {
                            if (arquivo.Extension.ToLower() == ".dhtml") 
                                bytesConteudo = GerarHtmlDinamico(arquivo.FullName, parametros);
                            else 
                                bytesConteudo = File.ReadAllBytes(arquivo.FullName);
                            string tipoMime = TiposMime[arquivo.Extension.ToLower()];
                            bytesCabecalho = GerarCabecalho(versaoHttp, tipoMime, "200", bytesConteudo.Length);
                        }
                        else
                        {
                            bytesConteudo = Encoding.UTF8.GetBytes("<h1>Erro 415   Tipo de arquivo não suportado.");
                            bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset-utf8", "415", bytesConteudo.Length);
                        }
                    }
                    else
                    {
                        bytesConteudo = Encoding.UTF8.GetBytes("<h1>Erro 404 - Arquivo não encontrado</h1>");
                        bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8", "404", bytesConteudo.Length);
                    }
                    int bytesEnviados = conexao.Send(bytesCabecalho, bytesCabecalho.Length, 0);
                    bytesEnviados += conexao.Send(bytesConteudo, bytesConteudo.Length, 0);
                    conexao.Close();
                    Console.WriteLine($"\n{bytesEnviados} bytes enviados em resposta à requisição #{numeroRequest}.");
                }
            }
            Console.WriteLine($"\nRequest {numeroRequest} finalizado.");
        }

        public byte[] GerarCabecalho(string versaoHttp, string tipoMime, string codigoHttp, int qtdBytes = 0)
        {
            StringBuilder texto = new();
            texto.Append($"{versaoHttp} {codigoHttp}{Environment.NewLine}");
            texto.Append($"Server: Servidor Http Simples 1.0{Environment.NewLine}");
            texto.Append($"Content-Type: {tipoMime}{Environment.NewLine}");
            texto.Append($"Content-Length: {qtdBytes}{Environment.NewLine}{Environment.NewLine}");
            return Encoding.UTF8.GetBytes(texto.ToString());
        }

        // tipos mime a serem suportados
        private void TiposMimeAdd()
        {
            TiposMime = new()
            {
                { ".htm", "text/html;charset=utf-8" },
                { ".html", "text/html;charset=utf-8" },
                { ".dhtml", "text/html;charset=utf-8" },
                { ".css", "text/css" },
                { ".js", "text/javascript" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".svg", "image/svg+xml" },
                { ".webp", "image/webp" },
                { ".ico", "image/ico" },
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" }
            };
        }

        public void MapearDiretoriosHost()
        {
            DiretoriosHost = new()
            {
                { "localhost", "C:\\Users\\Alexandre\\Documents\\Cursos\\CSharp\\CSharpComDotNet5\\ServidorHttpSimples\\www\\localhost" },
                { "servidorsimples.com", "C:\\Users\\Alexandre\\Documents\\Cursos\\CSharp\\CSharpComDotNet5\\ServidorHttpSimples\\www\\servidorsimples.com" }
            };
        }

        public string ObterDiretorio(string host, string arquivo)
        {
            string diretorio = DiretoriosHost[host.Split(":")[0]];
            return diretorio + arquivo.Replace("/", "\\");
        }

        public byte[] GerarHtmlDinamico(string caminhoArquivo, SortedList<string, string> parametros)
        {
            string coringa = "{{HtmlGerado}}";
            string htmlModelo = File.ReadAllText(caminhoArquivo);
            StringBuilder htmlGerado = new();
            /*htmlGerado.Append("<ul>");
            foreach(var tipo in TiposMime.Keys)
            {
                htmlGerado.Append($"<li>Arquivos com extensão {tipo}</li>");
            }
            htmlGerado.Append("</ul>");*/
            if (parametros.Count > 0)
            {
                htmlGerado.Append("<ul>");
                foreach (var p in parametros)
                {
                    htmlGerado.Append($"<li>{p.Key}={p.Value}</li>");
                }
                htmlGerado.Append("</ul>");
            }
            else
            {
                htmlGerado.Append("<p>Nenhum parâmetro foi passado.</p>");
            }
            string textoHtmlGerado = htmlModelo.Replace(coringa, htmlGerado.ToString());
            return Encoding.UTF8.GetBytes(textoHtmlGerado, 0, textoHtmlGerado.Length);
        }

        private SortedList<string, string> ProcessarParametros(string textoParametros)
        {
            SortedList<string, string> parametros = new();
            if (!string.IsNullOrEmpty(textoParametros.Trim()))
            {
                string[] paresChaveValor = textoParametros.Split("&");
                foreach(var par in  paresChaveValor)
                {
                    parametros.Add(par.Split("=")[0].ToLower(), par.Split("=")[1]);
                }
            }
            return parametros;
        }
    }
}