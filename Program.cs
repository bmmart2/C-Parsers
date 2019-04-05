using System;
using System.Collections.Generic;
using System.Linq;

namespace lexer
{
    public class RDP
    {
        static int tokenNum = 0;
        static string nextToken;
        static string[] tokens;


        static void expr()
        {
            if (tokenNum == 0)
                Console.WriteLine("nextToken: " + nextToken);
            Console.WriteLine("Enter < expr >");
            term();
            while (nextToken == "ADD_OP" || nextToken == "SUB_OP")
            {
                lex();
                term();
            }
            Console.WriteLine("Exit < expr >");
        }

        static void term()
        {
            Console.WriteLine("Enter < term >");
            //Parse the first term
            factor();
            //As long as the next token is * or /, call lex to get the //next token and parse the next factor
            while (nextToken == "MUL_OP" || nextToken == "DIV_OP")
            {
                lex();
                factor();
            }
            Console.WriteLine("Exit < term > ");
        }


        static void factor()
        {
            Console.WriteLine("Enter < factor >");
            if (nextToken == "IDENT" || nextToken == "INT_LIT")
                lex();
            else
            {
                if (nextToken == "LEFT_PAREN")
                {
                    lex();
                    expr();
                    if (nextToken == "RIGHT_PAREN")
                        lex();
                    else
                        error();
                }
                else
                    error();
            } //End of else
            Console.WriteLine("Exit < factor >");
        }

        static void error()
        {
            Console.WriteLine("ERROR: Unknown token.");
        }

        static void lex()
        {
            if (tokenNum < tokens.Length - 1)
            {
                tokenNum++;
                nextToken = tokens[tokenNum];
            }
            else
                nextToken = "EOF";
            Console.WriteLine("NextToken: " + nextToken);
        }

        public static void Main(string[] args)
        {
            RDP rdp = new RDP();

            Console.WriteLine("Enter your expression:");
            string input = Console.ReadLine();
            Console.WriteLine("-----------------------");
            Console.WriteLine("");
            try                                                 //program will terminate and not continue to RDP if Lexer.Tokenizer errors are caught.
            {
                tokens = Lexer.Tokenizer(input);
                nextToken = tokens[0];
            } catch(Exception e) { return; }

            Console.WriteLine("");
            Console.WriteLine("-----------------------");
            Console.WriteLine("Calling Recursive Descent Parser:");
            expr();
            Console.WriteLine("");
            Console.WriteLine("Press any key to close...");
            Console.ReadLine();
        }
    }

    class Lexer
    {
        /*
         * This method separates out the statement into a string array of tokens.
         * This method also checks for unbalanced parenthesis before allowing the program to RDP.
         */
        public static string[] Tokenizer(string input)
        {
            //Spaces out parenthesis. Ex: '(2 + 3)' --> '( 2 + 3 )'
            if (input.Contains("(") || input.Contains(")"))
            {
                input = input.Replace("(", "( ");
                input = input.Replace(")", " )");
            }

            string[] tokens = input.Split(' ');

            string result = "";
            int n;
            int parenthesis = 0; //used to track parenthesis



            //Dictionary for operators and parenthesis
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("/", "DIV_OP");
            dict.Add("*", "MUL_OP");
            dict.Add("+", "ADD_OP");
            dict.Add("-", "SUB_OP");
            dict.Add("(", "LEFT_PAREN");
            dict.Add(")", "RIGHT_PAREN");

            Console.WriteLine("Calling Lexer:");
            for (int i = 0; i < tokens.Length; i++)
            {
                Console.Write(tokens[i] + ": " );
                if (dict.TryGetValue(tokens[i], out result)) //If operator...
                {
                    tokens[i] = result;
                    Console.WriteLine(result);
                    if (result == "LEFT_PAREN")
                    {
                        parenthesis++;
                    }
                    else if (result == "RIGHT_PAREN")
                    {
                        if (parenthesis != 0)
                        {
                            parenthesis--;
                        }
                        else
                        {
                            Console.WriteLine("ERROR: No corresponding left-parenthesis for current right-parenthesis.");
                            return null;
                        }
                    }

                }
                else if (int.TryParse(tokens[i], out n))  // If integer...
                {
                    tokens[i] = "INT_LIT";
                    Console.WriteLine(tokens[i]);
                }
                else if (tokens[i].All(c => char.IsLetterOrDigit(c) || c == '_')) //INDENT must only contain ONLY letters/digits/underscores
                {
                    tokens[i] = "IDENT";
                    Console.WriteLine(tokens[i]);
                }
                else   //Else, must not be valid....
                {
                    Console.WriteLine("ERROR: Invalid token.");
                    return null;
                }
            }
            if (parenthesis == 0) //if parenthesis == 0, then they are balanced.
                return tokens;
            else
            {
                Console.WriteLine("ERROR: Unbalanced parenthesis.");
                return null;
            }
        }
    }
}
