using System;
using System.Collections.Generic;
using System.IO;

namespace Lab_1
{
   class Scanner
   {
      public ConstantTable Operations;
      public ConstantTable Divisions;
      public ConstantTable KeyWords;
      StreamWriter f;
      VariableTable Constants;
      VariableTable Identificators;
      bool flagBlockComment = false;
      string comment = "";
      public Scanner(ConstantTable Operations, ConstantTable Divisions, ConstantTable KeyWords,
                     VariableTable Constants, VariableTable Identificators)
      {
         this.Operations = Operations;
         this.Divisions = Divisions;
         this.KeyWords = KeyWords;
         this.Constants = Constants;
         this.Identificators = Identificators;
      }
      enum Statuses : int
      {
         Start = 0, ReadName, ReadNum, ReadChar, ReadComment, ReadOperator, End, Error
      }
      public List<int[]> FiniteStateMachine(string line)
      {
         Statuses Status = Statuses.Start;

         char[] letters = line.ToCharArray();
         string name = "";
         bool flag_constant = false;
         List<int[]> Tokens = new List<int[]>();
         char openchar = ' ';
         bool wasPoint = false;
         //bool flag_comment = false;
         bool flagLineComment = false;

         foreach (char letter in letters)
         {

            if (Status == Statuses.End && letter != '\n')
            {
               Status = Statuses.Start;
            }

            if (Status == Statuses.ReadChar)
            {
               if ("\'\"".Contains(letter) && name != "\\")
               {
                  if (name.Length == 0 && letter == '\'')
                  {
                     Status = Statuses.Error;
                  }
                  else
                  {
                     if (letter == openchar && (name.Length <= 1 || name.Length == 2 && name[0] == '\\'))
                        Status = Statuses.End;
                     else
                        Status = Statuses.Error;
                  }
               }
               else
               {
                  name += letter;
               }
            }
            if (Status == Statuses.Start)
            {
               if (flagBlockComment)
               {
                  Status = Statuses.ReadComment;
               }
               else
               {
                  openchar = ' ';
                  name = "";
                  flag_constant = false;
                  wasPoint = false;
                  //flag_comment = false;
                  flagLineComment = false;
                  if ("qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_".Contains(letter))
                  {
                     Status = Statuses.ReadName;
                  }
                  else if ("1234567890.".Contains(letter))
                  {
                     flag_constant = true;

                     Status = Statuses.ReadNum;
                  }
                  else if (letter == '\'' || letter == '\"')
                  {
                     flag_constant = true;
                     Status = Statuses.ReadChar;
                     openchar = letter;
                  }
                  else if (letter == ';' || letter == ',' || letter == '(' || letter == ')' || letter == '}')
                  {
                     name = letter.ToString();
                     Status = Statuses.End;
                  }
                  else if (Operations.SearchIsExist(letter.ToString()) || letter == '!' || letter == '=')
                  {
                     Status = Statuses.ReadOperator;
                  }
                  else if (letter == '/')
                  {
                     Status = Statuses.ReadComment;
                  }
                  else if (letter == '\n')
                  {
                     Status = Statuses.End;
                  }
               }
            }
            if (Status == Statuses.ReadName)
            {
               if (letter != ';' && letter != ' ' && letter != ',' && letter != '\n' || name == "void")
               {
                  if ("qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_".Contains(letter) || "1234567890".Contains(letter) || name == "void" && letter == ' ' 
                  || name == "void main" && letter == '(' || name == "void main(" && letter == ')' 
                  || name == "void main()" && letter == '{')
                     name += letter;
                  else
                     Status = Statuses.Error;
               }
               else
                  Status = Statuses.End;

            }

            if (Status == Statuses.ReadNum)
            {
               if ("1234567890.".Contains(letter))
               {

                  if (name == "0" && letter != '.')
                     name = "";
                  Status = Statuses.ReadNum;
                  name += letter;
                  if (letter == '.')
                     if (wasPoint)
                        Status = Statuses.Error;
                     else
                        wasPoint = true;
               }
               else if (letter == ';' || letter == ' ' || letter == '\n')
               {
                  if (name == ".")
                     Status = Statuses.Error;
                  else
                     Status = Statuses.End;
               }
               else
               {
                  Status = Statuses.Error;
               }
            }

            if (Status == Statuses.ReadComment)
            {
               if (comment == "/")
               {
                  if (letter == '/')
                  {
                     flagLineComment = true;
                  }
                  else if (letter == '*')
                  {
                     flagBlockComment = true;
                  }
                  else
                  {
                     Status = Statuses.Error;
                  }
               }
               else if (flagLineComment && letter == '\n')
               {
                  Status = Statuses.End;
                  flagLineComment = false;
                  comment = "";
               }
               else if (flagBlockComment && comment[^2] == '*' && comment[^1] == '/' && comment.Length > 3)
               {
                  flagBlockComment = false;
                  Status = Statuses.Start;
                  comment = "";
               }
               if (letter == '\n' || (!flagBlockComment && !flagLineComment) && letter == ' ')
                  Status = Statuses.End;
               else 
                  comment += letter;

            }

            if (Status == Statuses.ReadOperator)
            {
               name += letter;
               if (name[0] != '!' && name.Length == 1)
                  Status = Statuses.End;
               else
               if (name.Length == 2)
                  if (name == "!=")
                     Status = Statuses.End;
                  else
                     Status = Statuses.Error;
            }

            if (Status == Statuses.End)
            {
               if (!flagBlockComment && !flagLineComment)
               {
                  if (letter != '\n' || name.Length > 0)
                  {
                     int[] token = GetToken(name, flag_constant);
                     flag_constant = false;
                     if (token[1] != -1)
                     {
                        Tokens.Add(token);
                        if ((letter == ';' || letter == ',') && name != letter.ToString())
                           Tokens.Add(GetToken(letter.ToString(), flag_constant));
                        name = "";
                     }
                  }
               }
            }
         }

         if (Status != Statuses.End)
         {
            Console.WriteLine("ERROR");
            comment = "";
         }


         return Tokens;
      }

      private int[] GetToken(string name, bool flag)
      {
         int id = -1;
         if (!flag)
         {
            if (name.Length <= 2)
            {
               if ((id = Operations.SearchIdByName(name)) != -1) return new int[] { 0, id };
               if ((id = Divisions.SearchIdByName(name)) != -1) return new int[] { 1, id };
            }

            if ((id = KeyWords.SearchIdByName(name)) != -1) return new int[] { 2, id };
         }
         if (flag)
         {
            id = Constants.SearchIdByName(name); return new int[] { 3, id };
         }
         else
         {
            if (name.Length != 0)
               id = Identificators.SearchIdByName(name); return new int[] { 4, id };
         }
      }

      public void LexicalAnalysis(string filename)
      {
         using (f = new StreamWriter("C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\Tokenы.txt"))
         {
            string[] chains = File.ReadAllLines(filename);
            if (chains.Length == 0)
            {
               Console.WriteLine("File is empty");
            }
            foreach (var line in chains)
            {
               WriteTokens(FiniteStateMachine(line + "\n"));
            }
         }
         f.Close();
      }

      private void WriteTokens(List<int[]> Tokens)
      {
         foreach (var token in Tokens)
         {
            f.Write("(" + token[0] + "," + token[1] + ")|");
            //Console.Write("(" + token[0] + "," + token[1] + ")|");
         }
         f.Write("\n");
         //Console.Write('\n');
      }
   }
}
