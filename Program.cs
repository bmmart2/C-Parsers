using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace lexer
{
    public class SRP
    {
        private static int tokenCount = 0;
        private static string nextToken;
        private static string[] tokens;
        private static string[,] ActionMatrix = new string[12, 6]
        {   //id    +    *    (     )     $
            { "S5",null,null,"S4",null,null },
            { null,"S6",null,null,null,"accept" },
            { null,"R2","S7",null,"R2","R2" },
            { null,"R4","R4",null,"R4","R4" },
            { "S5",null,null,"S4",null,null },
            { null,"R6","R6",null,"R6","R6" },
            { "S5",null,null,"S4",null,null },
            { "S5",null,null,"S4",null,null },
            { null,"S6",null,null,"S11",null },
            { null,"R1","S7",null,"R1","R1" },
            { null,"R3","R3",null,"R3","R3" },
            { null,"R5","R5",null,"R5","R5" }
        };
        private static int[,] GotoMatrix = new int[12, 3]
        {
            { 1,2,3 },
            { 0,0,0 },
            { 0,0,0 },
            { 0,0,0 },
            { 8,2,3 },
            { 0,0,0 },
            { 0,9,3 },
            { 0,0,10 },
            { 0,0,0 },
            { 0,0,0 },
            { 0,0,0 },
            { 0,0,0 }
        };
        private static Dictionary<String, int> ActionColumn = new Dictionary<String, int>() {
            { "id", 0 },
            { "+", 1 },
            { "*", 2 },
            { "(", 3 },
            { ")", 4 },
            { "$", 5 }
         };
        private static Stack<String> stack = new Stack<String>();
        private static ArrayList StackHistory = new ArrayList();
        private static ArrayList ParseSteps = new ArrayList();

        public static void ShiftReduceParse(String[] input)
        {


            int column;
            int row; 
            string actionResult;
            bool parseComplete = false;
            nextToken = tokens[0];
            stack.Push("0");
            while (!parseComplete) {
                AddStackHistory(stack);
                row = int.Parse(stack.Peek());
                column = GetColumn(nextToken);
                actionResult = ActionMatrix[row, column];
                ParseSteps.Add(actionResult);
                if (actionResult == null)
                {
                    Console.WriteLine("Error: Invalid ActionMatrix value.");
                    break;
                }
                if (actionResult.StartsWith("S")) // Shift case
                {
                    stack.Push(nextToken);
                    if (actionResult == "S11")
                        stack.Push("11");
                    else
                        stack.Push(actionResult[1].ToString());
                    lex();
                }
                else if (actionResult.StartsWith("R")) // reduce case
                {
                    Reduce(int.Parse(actionResult[1].ToString()));

                }
                else //must be accept
                {
                    parseComplete = true;
                }

                


            }
        }
        // This method reduces the stack per the called production rule.
        public static void Reduce(int prodRule)
        {
            int row;
            string temp;
            Boolean rightID = false;
            Boolean leftID = false;
            switch (prodRule)
            {
                case 1: // E -> E + T

                    Boolean plus = false;

                    while (!rightID || !plus || !leftID)
                    {
                        if (stack.Peek() == null)
                        {
                            Console.WriteLine("Error: Reduce 1");
                            break;
                        }
                        temp = stack.Pop();
                        if (temp == "T")
                            rightID = true;
                        else if (temp == "+")
                            plus = true;
                        else if (temp == "E")
                            leftID = true;
                    }
                    temp = stack.Peek();
                    row = int.Parse(temp);
                    stack.Push("E");
                    stack.Push(GotoMatrix[row, 0].ToString());
                    break;

                case 2: //E -> T

                    stack.Pop();
                    if (stack.Peek() == "T")
                    {
                        stack.Pop();
                        row = int.Parse(stack.Peek());
                        stack.Push("E");
                        stack.Push(GotoMatrix[row, 0].ToString());
                    }
                    else
                        Console.WriteLine("Error: Reduce 2");
                    break;

                case 3: // T -> T * F

                    Boolean times = false;
                    while (!rightID || !times || !leftID)
                    {
                        if (stack.Peek() == null)
                        {
                            Console.WriteLine("Error: Reduce 3");
                            break;
                        }
                        temp = stack.Pop();
                        if (temp == "T")
                            leftID = true;
                        else if (temp == "*")
                            times = true;
                        else if (temp == "F")
                            rightID = true;
                    }
                    temp = stack.Peek();
                    row = int.Parse(temp);
                    stack.Push("T");
                    stack.Push(GotoMatrix[row, 1].ToString());
                    break;

                case 4: // T -> F

                    stack.Pop();
                    if (stack.Peek() == "F")
                    {
                        stack.Pop();
                        row = int.Parse(stack.Peek());
                        stack.Push("T");
                        stack.Push(GotoMatrix[row, 1].ToString());
                    }
                    else
                        Console.WriteLine("Error: Reduce 4");
                    break;

                case 5: // F -> (E)

                    Boolean rightParen = false;
                    Boolean id = false;
                    Boolean leftParen = false;
                    while (!rightParen || !id || !leftParen)
                    {
                        if (stack.Peek() == null)
                        {
                            Console.WriteLine("Error: Reduce 5");
                            break;
                        }
                        temp = stack.Pop();
                        if (temp == ")")
                            rightParen = true;
                        else if (temp == "E")
                            id = true;
                        else if (temp == "(")
                            leftParen = true;
                    }
                    temp = stack.Peek();
                    row = int.Parse(temp);
                    stack.Push("F");
                    stack.Push(GotoMatrix[row, 2].ToString());
                    break;

                case 6: // F -> id

                    stack.Pop();
                    if (stack.Peek() == "id")
                    {
                        stack.Pop();
                        row = int.Parse(stack.Peek());
                        stack.Push("F");
                        stack.Push(GotoMatrix[row, 2].ToString());
                    }
                    else
                        Console.WriteLine("Error: Reduce 6");
                    break;

                default:
                    break;

            }
        }

        public static int GetColumn(string s)
        {
            int column;
            if (ActionColumn.TryGetValue(s, out column)) {
                return column;
            }
            Console.WriteLine("ERROR: Invalid column for token: " + s);
            return -1;
        } 


        // This method advances to the next token in the Tokenized array.
        public static void lex()
        {
            tokenCount++;
            nextToken = tokens[tokenCount];
        }

        /*  This method takes a "snapshot" of the stack at the given point
         *  and adds to an array of stack histories.
         */       
        public static void AddStackHistory(Stack<string> input) {
            string[] temp = new string[input.Count()];
            input.CopyTo(temp,0);
            string result = "";
            for (int i = temp.Length - 1; i >= 0; i--)
                result += temp[i];
            StackHistory.Add(result);
        }

        public static void Main(string[] args)
        {

            Console.WriteLine("Enter your expression:");
            string input = Console.ReadLine();
            Console.WriteLine("-------------------------");
            tokens = Lexer.Tokenizer(input);
            SRP.ShiftReduceParse(tokens);
            Console.WriteLine("-------------------------");
            Console.WriteLine("Parse Steps");
            Console.WriteLine("-------------------------");
            foreach (string step in ParseSteps)
                Console.WriteLine(step);
            Console.WriteLine("-------------------------");
            Console.WriteLine("Stack History");
            Console.WriteLine("-------------------------");
             foreach (string step in StackHistory)
                Console.WriteLine(step);
            Console.WriteLine("-------------------------");
 
        }
    }

    class Lexer 
    { 
        /*
         * This method separates out the statement into a string array of tokens.
         * This method also checks for unbalanced parenthesis before allowing the program to parse.
         */       
        public static string[] Tokenizer(string input)
        {
            input += " $"; //append $ at end
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

            bool error = false;

            //Dictionary for operators and parenthesis
            List<string> dict = new List<string>();
            dict.Add("$");
            dict.Add("*");
            dict.Add("+");
            dict.Add("(");
            dict.Add(")");

            Console.WriteLine("Calling Lexer:");
            Console.WriteLine("-------------------------");
            for (int i = 0; i < tokens.Length; i++)
            {
                Console.Write(tokens[i] + ": " ); 
                if (dict.Contains(tokens[i])) //If operator...
                {
                    result = tokens[i];
                    Console.WriteLine(result);
                    if (result == "(")
                    {
                        parenthesis++;
                    }
                    else if (result == ")")
                    {
                        if (parenthesis != 0)
                        {
                            parenthesis--;
                        }
                        else
                        {
                            Console.WriteLine("ERROR: No corresponding left-parenthesis for current right-parenthesis.");
                            error = true;
                            return null;
                        }
                    }

                }
                else if (int.TryParse(tokens[i], out n))  // If integer...
                {
                    tokens[i] = "id";
                    Console.WriteLine(tokens[i]);
                }
                else if (tokens[i].All(c => char.IsLetterOrDigit(c) || c == '_')) //INDENT must only contain ONLY letters/digits/underscores
                {
                    tokens[i] = "id";
                    Console.WriteLine(tokens[i]);
                }
                else   //Else, must not be valid....
                {
                    Console.WriteLine("ERROR: Invalid token.");
                    return null;
                }
            }
            if (parenthesis == 0 && error == false) //if parenthesis == 0, then they are balanced.
                return tokens;
            else
            {
                Console.WriteLine("ERROR: Unbalanced parenthesis.");
                error = true;
                return null;
            }
        }
    }
}
