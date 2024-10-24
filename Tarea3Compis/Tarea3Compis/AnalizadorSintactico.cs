using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Compiladores
{
    public class SyntaxAnalyzer
    {
        private List<Lexema> tokens;
        private int currentTokenIndex;
        private Lexema currentToken;

        public static int RESULT_ACCEPT = 1;
        public static int RESULT_FAILED = 0;

        // Constructor que recibe la lista de tokens
        public SyntaxAnalyzer(List<Lexema> tokens)
        {
            this.tokens = tokens;
            this.currentTokenIndex = 0;
            this.currentToken = tokens[currentTokenIndex];  // Inicia con el primer token
        }

        // Método para avanzar al siguiente token
        private void NextToken()
        {
            if (currentTokenIndex < tokens.Count - 1)
            {
                currentTokenIndex++;
                currentToken = tokens[currentTokenIndex];
            }
        }

        // Método principal de análisis que inicia con la declaración de variables
        public int Parse()
        {
            return VarDeclaration();
        }

        // <var_declaration> -> 'VAR' identifier ':' <type> ';' <var_declaration> | ε
        private int VarDeclaration()
        {
            if (currentToken.Tipo == "VAR")
            {
                NextToken();  // Consumir 'VAR'

                if (currentToken.Tipo == "identifier")
                {
                    NextToken();  // Consumir identificador

                    if (currentToken.Valor == ":")
                    {
                        NextToken();  // Consumir ':'

                        if (TypeRule() == RESULT_ACCEPT)  // Comprobar si es un tipo válido
                        {
                            if (currentToken.Valor == ";")
                            {
                                NextToken();  // Consumir ';'

                                // Recursivamente comprobar la siguiente declaración o ε
                                return VarDeclaration();
                            }
                            else
                            {
                                Console.WriteLine("Error: Se esperaba ';'");
                                return RESULT_FAILED;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: Tipo no válido");
                            return RESULT_FAILED;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Se esperaba ':'");
                        return RESULT_FAILED;
                    }
                }
                else
                {
                    Console.WriteLine("Error: Se esperaba un identificador");
                    return RESULT_FAILED;
                }
            }

            // ε (epsilon), producción vacía
            return RESULT_ACCEPT;
        }

        // <type> -> 'INTEGER' | 'REAL' | 'BOOLEAN' | 'STRING'
        private int TypeRule()
        {
            if (currentToken.Tipo == "INTEGER" || currentToken.Tipo == "REAL" ||
                currentToken.Tipo == "BOOLEAN" || currentToken.Tipo == "STRING")
            {
                NextToken();  // Consumir el tipo
                return RESULT_ACCEPT;
            }

            return RESULT_FAILED;
        }
    }
}
