using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Compiladores
{
    public class Lexema
    {
        // Propiedades del lexema: tipo (como terminal, identificador, número, etc.) y valor
        public string Tipo { get; }
        public string Valor { get; }

        // Constructor para crear un lexema
        public Lexema(string tipo, string valor)
        {
            Tipo = tipo;
            Valor = valor;
        }

        // Método para convertir el lexema a una cadena de texto
        public override string ToString()
        {
            return $"<{Tipo}, {Valor}>";  // Representación en formato <Tipo, Valor>
        }
    }
}
