using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proyecto_Compiladores
{
    public class LexicalAnalyzer
    {
        // Terminales definidos en la gramática
        private static readonly string[] TERMINALS = {
        "VAR", "INTEGER", "REAL", "BOOLEAN", "STRING", "BEGIN", "END",
        "PROCEDURE", "IF", "THEN", "ELSE", "WHILE", "DO", "PRINTLN", "READLN",
        "TRUE", "FALSE", "OR", "AND", ":=", "=", "<>", "<", "<=", ">", ">=",
        "+", "-", "*", "/", ";", ".", "(", ")", ",", ":"
    };

        // Patrón regex para reconocer los tokens
        private static readonly Regex TOKEN_PATTERN = new Regex(
            @"\b(VAR|INTEGER|REAL|BOOLEAN|STRING|BEGIN|END|PROCEDURE|IF|THEN|ELSE|WHILE|DO|PRINTLN|READLN|TRUE|FALSE|OR|AND)\b" +  // Palabras reservadas
            @"|(:=|=|<>|<|<=|>|>=|\+|\-|\*|/|;|\.|\(|\)|,|:)" +  // Operadores y signos de puntuación
            @"|([a-zA-Z_][a-zA-Z0-9_]*)" +  // Identificadores
            @"|([0-9]+(\.[0-9]+)?)" +  // Números
            @"|""([^""]*)""",  // Strings
            RegexOptions.Compiled
        );

        public static List<Lexema> GetLexicalTokens(string input)
        {
            var tokens = new List<Lexema>();
            var matches = TOKEN_PATTERN.Matches(input);

            foreach (Match match in matches)
            {
                string token = match.Value;

                // Verificar si el token es uno de los terminales definidos en la gramática
                if (Array.Exists(TERMINALS, t => t == token))
                {
                    tokens.Add(new Lexema(token, token));  // Añadir token como terminal
                }
                else if (Regex.IsMatch(token, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))  // Identificadores
                {
                    tokens.Add(new Lexema("identifier", token));
                }
                else if (Regex.IsMatch(token, @"^[0-9]+(\.[0-9]+)?$"))  // Números
                {
                    tokens.Add(new Lexema("number", token));
                }
                else if (Regex.IsMatch(token, @"""([^""]*)"""))  // Strings
                {
                    tokens.Add(new Lexema("string", token));
                }
            }

            tokens.Add(new Lexema("$", "eof"));  // Añadir token de fin de archivo (EOF)
            return tokens;
        }
    }
}

