using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoCompiladores
{
    public class ItemLR1
    {
        public string LadoIzquierdo { get; set; }
        public string[] LadoDerecho { get; set; }
        public int Punto { get; set; }
        public string Lookahead { get; set; }

        public ItemLR1(string ladoIzq, string[] ladoDer, int punto, string lookahead)
        {
            LadoIzquierdo = ladoIzq;
            LadoDerecho = ladoDer;
            Punto = punto;
            Lookahead = lookahead;
        }

        public bool EsItemDeReduccion()
        {
            return Punto >= LadoDerecho.Length;
        }

        public override string ToString()
        {
            string ladoDerechoConPunto = string.Join(" ", LadoDerecho.Take(Punto)) + " . " + string.Join(" ", LadoDerecho.Skip(Punto));
            return $"{LadoIzquierdo} → {ladoDerechoConPunto} , {Lookahead}";
        }

        public override bool Equals(object obj)
        {
            if (obj is ItemLR1 other)
            {
                return LadoIzquierdo == other.LadoIzquierdo
                    && Punto == other.Punto
                    && Lookahead == other.Lookahead
                    && LadoDerecho.Length == other.LadoDerecho.Length
                    && LadoDerecho.SequenceEqual(other.LadoDerecho);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = LadoIzquierdo.GetHashCode() ^ Punto.GetHashCode() ^ Lookahead.GetHashCode();
            foreach (var symbol in LadoDerecho)
            {
                hashCode ^= symbol.GetHashCode();
            }
            return hashCode;
        }
    }

    public class LR1Parser
    {
        public Dictionary<string, List<string>> producciones;
        public List<HashSet<ItemLR1>> estados;
        public Dictionary<int, Dictionary<string, Transicion>> tablaAccion;
        public Dictionary<int, Dictionary<string, int>> tablaGoto;

        public LR1Parser(Dictionary<string, List<string>> _producciones)
        {
            producciones = _producciones;
            estados = new List<HashSet<ItemLR1>>();
            tablaAccion = new Dictionary<int, Dictionary<string, Transicion>>();
            tablaGoto = new Dictionary<int, Dictionary<string, int>>();
        }

        public void CrearTransiciones()
        {
            HashSet<ItemLR1> estadoInicial = new HashSet<ItemLR1>
            {
                new ItemLR1("<S>", new string[] { "<var_declaration>", "$" }, 0, "$")
            };

            HashSet<ItemLR1> cierreInicial = Closure(estadoInicial);
            estados.Add(cierreInicial);

            Console.WriteLine("Estados generados:");

            for (int i = 0; i < estados.Count; i++)
            {
                HashSet<ItemLR1> estado = estados[i];

                Console.WriteLine($"\nEstado {i}:");
                foreach (var item in estado)
                {
                    Console.WriteLine(item.ToString());

                    if (!item.EsItemDeReduccion())
                    {
                        foreach (var simbolo in ObtenerSimbolos(estado))
                        {
                            HashSet<ItemLR1> nuevoEstado = Goto(estado, simbolo);

                            if (nuevoEstado.Count > 0)
                            {
                                int indiceExistente = ObtenerIndiceEstadoExistente(nuevoEstado);

                                if (indiceExistente == -1)
                                {
                                    estados.Add(nuevoEstado);
                                    AgregarTransicion(i, simbolo, estados.Count - 1);
                                }
                                else if (!tablaAccion.ContainsKey(i) || !tablaAccion[i].ContainsKey(simbolo))
                                {
                                    AgregarTransicion(i, simbolo, indiceExistente);
                                }

                                if (esTerminal(simbolo))
                                {
                                    AgregarAccion(i, simbolo, $"SHIFT a Estado {(indiceExistente == -1 ? estados.Count - 1 : indiceExistente)}");
                                }
                                else
                                {
                                    if (!tablaGoto.ContainsKey(i))
                                    {
                                        tablaGoto[i] = new Dictionary<string, int>();
                                    }
                                    tablaGoto[i][simbolo] = indiceExistente == -1 ? estados.Count - 1 : indiceExistente;
                                }
                            }
                        }
                    }
                }

                foreach (var item in estado)
                {
                    if (item.EsItemDeReduccion())
                    {
                        if (item.LadoIzquierdo == "<S>" && item.Lookahead == "$")
                        {
                            AgregarAccion(i, "$", "ACCEPT");
                        }
                        else
                        {
                            AgregarAccion(i, item.Lookahead, $"REDUCE {item.LadoIzquierdo} → {string.Join(" ", item.LadoDerecho)}");
                        }
                    }
                }
            }
        }

        private int ObtenerIndiceEstadoExistente(HashSet<ItemLR1> estado)
        {
            for (int i = 0; i < estados.Count; i++)
            {
                if (estados[i].SetEquals(estado))
                {
                    return i;
                }
            }
            return -1;
        }

        private HashSet<ItemLR1> Closure(HashSet<ItemLR1> items)
        {
            HashSet<ItemLR1> cerrado = new HashSet<ItemLR1>(items);
            bool cambios = true;

            while (cambios)
            {
                cambios = false;
                HashSet<ItemLR1> nuevosItems = new HashSet<ItemLR1>();

                foreach (var item in cerrado)
                {
                    if (item.Punto < item.LadoDerecho.Length)
                    {
                        string simbolo = item.LadoDerecho[item.Punto];

                        if (producciones.ContainsKey(simbolo))
                        {
                            foreach (var produccion in producciones[simbolo])
                            {
                                string[] produccionDividida = DividirProduccion(produccion);
                                ItemLR1 nuevoItem = new ItemLR1(simbolo, produccionDividida, 0, item.Lookahead);

                                if (!cerrado.Contains(nuevoItem) && !nuevosItems.Contains(nuevoItem))
                                {
                                    nuevosItems.Add(nuevoItem);
                                    cambios = true;
                                }
                            }
                        }
                    }
                }

                cerrado.UnionWith(nuevosItems);
            }

            return cerrado;
        }

        private HashSet<ItemLR1> Goto(HashSet<ItemLR1> estado, string simbolo)
        {
            HashSet<ItemLR1> nuevoEstado = new HashSet<ItemLR1>();

            foreach (var item in estado)
            {
                if (item.Punto < item.LadoDerecho.Length && item.LadoDerecho[item.Punto] == simbolo)
                {
                    ItemLR1 nuevoItem = new ItemLR1(item.LadoIzquierdo, item.LadoDerecho, item.Punto + 1, item.Lookahead);
                    nuevoEstado.Add(nuevoItem);
                }
            }

            return nuevoEstado.Count > 0 ? Closure(nuevoEstado) : new HashSet<ItemLR1>();
        }

        private string[] DividirProduccion(string produccion)
        {
            return produccion.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void AgregarAccion(int estado, string simbolo, string accion)
        {
            if (!tablaAccion.ContainsKey(estado))
            {
                tablaAccion[estado] = new Dictionary<string, Transicion>();
            }

            if (!tablaAccion[estado].ContainsKey(simbolo))
            {
                tablaAccion[estado][simbolo] = new Transicion { Simbolo = accion };
                Console.WriteLine($"Acción generada: Estado {estado}, Símbolo {simbolo}, Acción: {accion}");
            }
        }

        private void AgregarTransicion(int estadoOrigen, string simbolo, int estadoDestino)
        {
            if (!tablaGoto.ContainsKey(estadoOrigen))
            {
                tablaGoto[estadoOrigen] = new Dictionary<string, int>();
            }

            if (!tablaGoto[estadoOrigen].ContainsKey(simbolo))
            {
                tablaGoto[estadoOrigen][simbolo] = estadoDestino;
            }
        }

        private HashSet<string> ObtenerSimbolos(HashSet<ItemLR1> estado)
        {
            HashSet<string> simbolos = new HashSet<string>();

            foreach (var item in estado)
            {
                if (item.Punto < item.LadoDerecho.Length)
                {
                    simbolos.Add(item.LadoDerecho[item.Punto]);
                }
            }

            return simbolos;
        }

        private bool esTerminal(string simbolo)
        {
            return simbolo != null && !(simbolo.StartsWith("<") && simbolo.EndsWith(">"));
        }

        public void ParsearCadena(string input, List<string> tokens)
        {
            // Añadir el símbolo de fin de cadena
            tokens.Add("$");

            Stack<int> pilaEstados = new Stack<int>();
            Stack<string> pilaSimbolos = new Stack<string>();
            pilaEstados.Push(0); // Estado inicial

            int step = 1;
            Console.WriteLine("Tokens recibidos: " + string.Join(", ", tokens));
            Console.WriteLine("Step\tStack\t\tInput\t\t\tAction");

            while (tokens.Count > 0)
            {
                int estadoActual = pilaEstados.Peek(); // Estado actual en la pila
                string simboloActual = tokens[0];      // Primer token de la entrada

                // Imprimir el estado y el símbolo actual
                Console.WriteLine($"Estado actual: {estadoActual}, Símbolo actual: {simboloActual}");

                if (tablaAccion.ContainsKey(estadoActual) && tablaAccion[estadoActual].ContainsKey(simboloActual))
                {
                    Transicion accion = tablaAccion[estadoActual][simboloActual];

                    Console.WriteLine($"{step}\t{string.Join(" ", pilaEstados)}\t\t{string.Join(" ", tokens)}\t\t{accion.Simbolo}");

                    // Procesar SHIFT
                    if (accion.Simbolo.StartsWith("SHIFT"))
                    {
                        int nuevoEstado = int.Parse(accion.Simbolo.Split(' ').Last()); // Obtener el estado al que se hace shift
                        pilaSimbolos.Push(simboloActual);
                        pilaEstados.Push(nuevoEstado);

                        // Remover el token procesado si no es el símbolo de fin de cadena
                        if (!simboloActual.Equals("$"))
                        {
                            tokens.RemoveAt(0); // Avanzar en la entrada
                        }

                        step++;
                    }
                    // Procesar REDUCE
                    else if (accion.Simbolo.StartsWith("REDUCE"))
                    {
                        string produccion = accion.Produccion;
                        string ladoIzquierdo = produccion.Split('→')[0].Trim(); // Lado izquierdo de la producción
                        string[] ladoDerecho = produccion.Split('→')[1].Trim().Split(' '); // Lado derecho de la producción

                        // Desapilar los símbolos de la pila de acuerdo a la producción
                        int elementosADesapilar = ladoDerecho.Length;
                        if (ladoDerecho.Length == 1 && ladoDerecho[0] == "ε") // Si es epsilon, no se desapilan símbolos
                        {
                            elementosADesapilar = 0;
                        }

                        for (int i = 0; i < elementosADesapilar; i++)
                        {
                            pilaSimbolos.Pop();
                            pilaEstados.Pop();
                        }

                        // Empujar el lado izquierdo de la producción a la pila
                        pilaSimbolos.Push(ladoIzquierdo);

                        // Buscar el estado de GOTO en la tabla GOTO
                        if (tablaGoto.ContainsKey(pilaEstados.Peek()) && tablaGoto[pilaEstados.Peek()].ContainsKey(ladoIzquierdo))
                        {
                            int estadoGoto = tablaGoto[pilaEstados.Peek()][ladoIzquierdo];
                            pilaEstados.Push(estadoGoto);
                        }
                        else
                        {
                            Console.WriteLine($"Error de GOTO: No se encontró una transición para {ladoIzquierdo} en el estado {pilaEstados.Peek()}");
                            break;
                        }

                        step++;
                    }
                    // Procesar ACCEPT
                    else if (accion.Simbolo == "ACCEPT")
                    {
                        Console.WriteLine($"{step}\t{string.Join(" ", pilaEstados)}\t\t{string.Join(" ", tokens)}\t\tACCEPT");
                        break;
                    }
                }
                else
                {
                    // Si no hay una acción válida, es un error de parseo
                    Console.WriteLine($"Error de parseo: No se encontró una acción válida para el símbolo '{simboloActual}' en el estado {estadoActual}.");
                    break;
                }
            }
        }


        public void ImprimirTablaAccionEnArchivo()
        {
            string rutaArchivo = @"C:\tabla_acciones.txt";

            using (StreamWriter writer = new StreamWriter(rutaArchivo))
            {
                foreach (var estado in tablaAccion)
                {
                    writer.WriteLine($"Estado {estado.Key}:");

                    foreach (var transicion in estado.Value)
                    {
                        writer.WriteLine($"  Símbolo: {transicion.Key}, Acción: {transicion.Value.Simbolo}");
                    }

                    writer.WriteLine();
                }

                writer.WriteLine("Tabla de Acciones exportada correctamente.");
            }

            Console.WriteLine($"La tabla de acciones se ha guardado en: {rutaArchivo}");
        }

        public void ImprimirEstadosGeneradosEnArchivo()
        {
            string rutaArchivo = @"C:\estados_generados.txt";

            using (StreamWriter writer = new StreamWriter(rutaArchivo))
            {
                for (int i = 0; i < estados.Count; i++)
                {
                    HashSet<ItemLR1> estado = estados[i];

                    writer.WriteLine($"\nEstado {i}:");
                    foreach (var item in estado)
                    {
                        writer.WriteLine(item.ToString());
                    }

                    writer.WriteLine();
                }

                writer.WriteLine("Estados generados exportados correctamente.");
            }

            Console.WriteLine($"Los estados generados se han guardado en: {rutaArchivo}");
        }
    }

    public class Transicion
    {
        public string Simbolo { get; set; }
        public string Produccion { get; set; }
    }
}
