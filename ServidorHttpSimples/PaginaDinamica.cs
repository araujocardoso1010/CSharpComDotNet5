﻿using System.Text;

namespace ServidorHttpSimples
{
    public abstract class PaginaDinamica
    {
        public string HtmlModelo { get; set; }
        public virtual byte[] Get(SortedList<string, string> parametros)
        {
            return Encoding.UTF8.GetBytes(HtmlModelo);
        }
        public virtual byte[] Post(SortedList<string, string> parametros)
        {
            return Encoding.UTF8.GetBytes(HtmlModelo);
        }
    }
}
