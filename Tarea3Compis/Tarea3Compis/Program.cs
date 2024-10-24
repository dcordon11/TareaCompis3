using Proyecto_Compiladores;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProyectoCompiladores
{
    class Program
    {
        private static FileAnalyzer fileAnalyzer;
        private static string textoCompleto = ""; // Contenido completo del archivo

        private static List<string> terminals = new List<string>();

        public static void Main(string[] args)
        {
            Console.WriteLine("Tarea 3");

            // Leer el archivo y analizarlo
            LeerArchivo("../../../Grammar.txt");
            fileAnalyzer = new FileAnalyzer(textoCompleto);
            fileAnalyzer.ValidarTexto(); // Analiza y valida el contenido del archivo

            terminals = fileAnalyzer.getTerminals();
            fileAnalyzer.getNonTerminals();
            var rawProductions = fileAnalyzer.getProductions();

            // Obtener los tokens desde el analizador léxico
            var tokens = LexicalAnalyzer.GetLexicalTokens("VAR x : INTEGER; VAR y : REAL; BEGIN x := 10; y := 5.5; END.");
            List<string> tokenAMandar = new List<string>();

            Console.WriteLine("Tokens encontrados:");
            foreach (var token in tokens)
            {
                tokenAMandar.Add(token.Tipo);  // Se agrega el tipo de token a la lista para el parser
                Console.WriteLine(token);  // Imprime los tokens encontrados
            }

            Console.WriteLine("------------------------------------------------------------------------------------------");

            // Definir las producciones de la gramática
            Dictionary<string, List<string>> producciones = new Dictionary<string, List<string>>
            {
                { "<S>", new List<string> { "<var_declaration> $" } },
                { "<var_declaration>", new List<string> { "VAR identifier : <type> ; <var_declaration>", "ε" } },
                { "<type>", new List<string> { "INTEGER", "REAL", "BOOLEAN", "STRING" } },
            };

            // Crear el parser LR(1)
            LR1Parser parser = new LR1Parser(producciones);

            // Crear las transiciones y generar los estados LR(1)
            parser.CrearTransiciones();

            // Imprimir la tabla de acciones en un archivo
            parser.ImprimirTablaAccionEnArchivo();

            // Imprimir los estados generados en un archivo
            parser.ImprimirEstadosGeneradosEnArchivo();

            // Cadena de entrada para el parser (esta cadena representa una declaración de variables válida)
            string input = "VAR x : INTEGER; VAR y : REAL; BEGIN x := 10; y := 5.5; END.";

            // Llamar al método ParsearCadena para procesar la cadena de entrada
            parser.ParsearCadena(input, tokenAMandar);

            Console.ReadKey();
        }

        static void LeerArchivo(string rutaArchivo)
        {
            try
            {
                using (StreamReader sr = new StreamReader(rutaArchivo))
                {
                    string linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        textoCompleto += linea + Environment.NewLine; // Mantener el formato de líneas
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"El archivo no pudo ser leído: {e.Message}");
            }
        }
    }
}
