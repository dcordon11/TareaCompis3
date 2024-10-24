using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Proyecto_Compiladores
{
    public class FileAnalyzer
    {
        // Regex para captura de secciones
        private static readonly Regex productionsRegex = new Regex(@"^\s*<([a-zA-Z_][a-zA-Z0-9_]*)>\s*=\s*((?:<([a-zA-Z_][a-zA-Z0-9_]*)>|'[^']+'|\"".*\""|[a-zA-Z_][a-zA-Z0-9_]*|ε|\s|\||:|;|\.\s*)+)\s*$", RegexOptions.Multiline);

        private string texto;
        private List<string> sets = new List<string>();
        private List<string> tokens = new List<string>();
        private List<string> keywords = new List<string>();
        private Dictionary<string, List<string>> productions = new Dictionary<string, List<string>>();

        // Vamos a almacenar los terminales y los no terminales
        public List<string> nonTerminals = new List<string>();
        public List<string> terminals = new List<string>();

        public FileAnalyzer(string textoCompleto)
        {
            texto = textoCompleto;
        }

        private bool ValidarSeccion(string nombreSeccion, string contenido, Regex regex)
        {
            // Verificar si el contenido de la sección coincide con la expresión regular proporcionada
            if (regex.IsMatch(contenido))
            {
                Console.WriteLine($"La sección {nombreSeccion} es válida.\n");
                return true;  // La sección es válida
            }
            else
            {
                Console.WriteLine($"La sección {nombreSeccion} NO es válida.\n");
                return false;  // La sección no es válida
            }
        }


        public void ValidarTexto()
        {
            // Separar secciones relevantes
            var secciones = Regex.Split(texto, @"\b(PRODUCTIONS)\b", RegexOptions.IgnoreCase);
            Console.WriteLine($"Secciones encontradas: {(secciones.Length - 1) / 2}");

            for (int i = 1; i < secciones.Length; i += 2)
            {
                string seccionNombre = secciones[i].ToUpper();
                string seccionContenido = secciones[i + 1].Trim();

                switch (seccionNombre)
                {
                    case "PRODUCTIONS":
                        bool bandera = ValidarSeccion("PRODUCTIONS", seccionContenido, productionsRegex);
                        if (!bandera)
                        {
                            Console.WriteLine($"Error en la sección {seccionNombre}");
                            break;
                        }
                        else
                        {
                            GuardarProductions(seccionContenido);
                            ValidarProducciones();  // Validar relaciones entre producciones
                        }
                        break;
                    default:
                        Console.WriteLine($"Sección desconocida: {seccionNombre}");
                        break;
                }
            }
        }

        // Método para guardar las producciones
        private void GuardarProductions(string contenido)
        {
            var lineas = contenido.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var linea in lineas)
            {
                int indexOfEquals = linea.IndexOf('=');

                if (indexOfEquals != -1)
                {
                    string left = linea.Substring(0, indexOfEquals).Trim();  // Parte izquierda
                    string right = linea.Substring(indexOfEquals + 1).Trim(); // Parte derecha

                    if (!productions.ContainsKey(left))
                    {
                        productions[left] = new List<string>();
                    }

                    var rightElements = right.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    productions[left].AddRange(rightElements);

                    Console.WriteLine($"Producción añadida: {left} = {string.Join(" ", rightElements)}");
                }
            }
            Console.WriteLine("Producciones registradas correctamente.");
        }

        // Validar producciones y verificar terminales y no terminales
        public void ValidarProducciones()
        {
            Console.WriteLine("Validando Producciones...\n");

            foreach (var produccion in productions)
            {
                string noTerminal = produccion.Key;
                List<string> elementos = produccion.Value;

                Console.WriteLine($"Validando producción: {noTerminal}");

                foreach (var elemento in elementos)
                {
                    string cleanedElement = elemento.Trim();

                    if (cleanedElement.StartsWith("<") && cleanedElement.EndsWith(">"))
                    {
                        Console.WriteLine($"No terminal encontrado: {cleanedElement}");

                        if (!productions.ContainsKey(cleanedElement))
                        {
                            Console.WriteLine($"Error: No terminal '{cleanedElement}' no está definido.");
                        }
                        else
                        {
                            Console.WriteLine($"No terminal '{cleanedElement}' está definido.");
                            nonTerminals.Add(cleanedElement);
                        }
                        continue;
                    }

                    if (EsTerminal(cleanedElement))
                    {
                        Console.WriteLine($"Elemento terminal válido: {cleanedElement}");
                        terminals.Add(cleanedElement);
                    }
                    else
                    {
                        Console.WriteLine($"Error: Elemento '{cleanedElement}' no existe en tokens o sets.");
                    }
                }

                Console.WriteLine("---------------------------------------------------\n");
            }

            Console.WriteLine("Validación de producciones completada.");
        }

        private bool EsTerminal(string element)
        {
            element = element.Trim('\'', ' ');

            // Verifica si el elemento es un terminal válido o ε (epsilon)
            return element == "ε" || new[] { "VAR", "INTEGER", "REAL", "BOOLEAN", "STRING", ";", ":" }.Contains(element);
        }

        public List<string> getNonTerminals()
        {
            return nonTerminals;
        }

        public List<string> getTerminals()
        {
            return terminals.Select(t => t.Trim()).Distinct().ToList();
        }

        public Dictionary<string, List<string>> getProductions()
        {
            return productions;
        }
    }
}
