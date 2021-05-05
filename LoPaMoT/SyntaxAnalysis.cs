using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lab_1
{
   class SyntaxAnalysis
   {
      string tokensAll;
      public ConstantTable Operations;
      public ConstantTable Divisions;
      public ConstantTable KeyWords;
      StreamWriter f;
      VariableTable Constants;
      VariableTable Identificators;

      public SyntaxAnalysis(ConstantTable Operations, ConstantTable Divisions, ConstantTable KeyWords,
                     VariableTable Constants, VariableTable Identificators)
      {
         tokensAll = File.ReadAllText("C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\Tokenы.txt");
         f = new StreamWriter("C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\Errors.txt");
         this.Operations = Operations;
         this.Divisions = Divisions;
         this.KeyWords = KeyWords;
         this.Constants = Constants;
         this.Identificators = Identificators;
      }

      public void WorkSyntAn()
      {

         if(tokensAll.Length > 1) tokensAll = tokensAll.Remove(tokensAll.Length - 1).Replace("\r", ""); //.Replace("\n", "");
         string[] tokens = { };
         if (tokensAll.Length > 1) tokens = tokensAll.Remove(tokensAll.Length - 1).Split('|', '\n');
         int[] token;

         bool start = false;
         bool end = false;
         bool init = false;
         bool assignment = false;
         bool oper = false;
         bool type = false;
         bool ID = false;
         bool prevID = false;

         int line = 1;
         Stack<int[]> opers = new Stack<int[]>();
         string type_s = "";
         string id;
         int[] idtoken = { };
         for (int numToken = 0; numToken < tokens.Length; numToken++)
         {
            if (tokens[numToken] == "")
            {
               line++;
               continue;
            }
            string token_s = GetToken(token = ReadToken(tokens, numToken));
            if (end)
            {
               WriteError(line, "Неожиданный символ\n");
            }
            if (token_s == "void main(){")
            {
               start = true;
               continue;
            }
            if (token_s == "}")
            {
               end = true;
               break;
            }
            if (!start)
            {
               WriteError(line, "Неверное начало программы");
               break;
            }
            else
            {
               if (Constants.SearchIsExist(token_s))
                  prevID = true;
               if (token_s == "(" || token_s == ")")
                  prevID = false;
               if (KeyWords.SearchIsExist(token_s))
               {
                  init = true;
                  if (assignment) WriteError(line - 1, "Требуется \";\"");
               }
               else if (Identificators.SearchIsExist(token_s))
               {
                  if (!init && !oper)
                  {
                     assignment = true;
                  }
                  else if (!init && oper && prevID)
                  {
                     oper = false;
                     while (opers.Count != 0)
                     {
                        Console.Write(GetToken(opers.Pop()) + " ");
                     }
                     WriteError(line - 1, "Требуется \";\"");
                  }
                  prevID = !prevID;
               }
               else if (!oper && !type && token_s != ";" && !Operations.SearchIsExist(token_s) && !Constants.SearchIsExist(token_s))
                  WriteError(line, "\"" + token_s + "\" не существует");
               if (Operations.SearchIsExist(token_s))
                  prevID = false;
               //else if (oper)

               if (token_s == ";") // end line
               {
                  init = false;
                  type = false;
                  ID = false;
                  prevID = false;
                  oper = false;
                  assignment = false;
                  while (opers.Count != 0)
                  {
                     Console.Write(GetToken(opers.Pop()) + " ");
                  }
                  Console.WriteLine();
               }
               if (assignment)
               {
                  if (ID)
                  {
                     if (token_s == "=")
                     {
                        oper = true;
                        assignment = false;
                     }
                     else
                     {
                        WriteError(line, "Неожиданный символ \"" + token_s + "\" ");     // ошибка
                     }
                  }
                  if (!ID)
                  {
                     ID = prevID = true;
                     id = token_s;
                     if (!Identificators.HasType(id))
                     {
                        WriteError(line, "Переменная \"" + token_s + "\" не объявлена"); // ошибка
                        assignment = false;
                        ID = false;
                        continue;
                     }
                  }
               }
               if (init) // line
               {
                  if (ID && type) // var asd /////// по идее без type,
                                  // т.к. {var asd = 1} и {asd = 1} -> {TYPE ID = CONST} и {ID = CONST}
                  {
                     if (token_s == "=")
                     {
                        oper = true;
                        type = false;
                        init = false;
                     }
                     else if (!assignment)
                     {
                        if (token_s == ",")
                        {
                           ID = false;
                        }
                        else
                        {
                           WriteError(line, "Неожиданный символ\n");     // ошибка
                        }
                     }
                     else if (assignment)
                     {
                        WriteError(line, "123\n");
                     }
                  }
                  if (!ID && type && token_s != ",")
                  {
                     ID = prevID = true;
                     id = token_s;
                     idtoken = token;
                     if (!Identificators.SetType(id, type_s))
                        WriteError(line, "Повторная инициализация \"" + token_s + "\""); // ошибка
                  }
                  if (!type && !oper)
                  {
                     type = true;
                     type_s = token_s;
                  }
               }
               if (oper)
               {
                  if (ID)
                  {
                     WritePostfix(idtoken, opers);
                     ID = false;
                     prevID = false;
                  }
                  WritePostfix(token, opers);
               }
            }
         }
         if (!end && tokens.Length > 0)
         {
            WriteError(line, "Требуется \"}\"");
         }
         f.Close();
      }
      void WritePostfix(int[] token, Stack<int[]> operations)
      {
         if (token[0] == 3)
            Console.Write(Constants.SearchNameById(token[1]) + " "); // postfix {CONST}    //
         else if (token[0] == 4)                                                         // Выводить лучше как токены
            Console.Write(Identificators.SearchNameById(token[1]) + " "); // postfix {ID}  //
         else
         {
            int priority_token = count_priority(token);
            if (operations.Count != 0)
               while (count_priority(operations.Peek()) >= priority_token)
               {
                  string pop_token = GetToken(operations.Pop());
                  Console.Write(pop_token + " "); // postfix {OPS}  //наверно, тоже как токены лучше выводить
                  if (operations.Count == 0) break;
               }
            operations.Push(token);
         }
      }
      int count_priority(int[] token)
      {
         string name = Operations.SearchNameById(token[1]);
         if (name == "*")
            return 4;
         else if (name == "+" || name == "-")
            return 3;
         else if (name == "<" || name == ">" || name == "!=" || name == "==")
            return 2;
         else if (name == ")" || name == "=")
            return 1;
         else if (name == "(")
            return 0;
         else return -1;
      }
      string GetToken(int[] token)
      {
         if (token[0] == 0)
         {
            return Operations.SearchNameById(token[1]);
         }
         else if (token[0] == 1)
         {
            return Divisions.SearchNameById(token[1]);
         }
         else if (token[0] == 2)
         {
            return KeyWords.SearchNameById(token[1]);
         }
         else if (token[0] == 3)
         {
            return Constants.SearchNameById(token[1]);
         }
         else if (token[0] == 4)
         {
            return Identificators.SearchNameById(token[1]);
         }
         return "";
      }
      int[] ReadToken(string[] tokens, int tokenNum)
      {
         string token = tokens[tokenNum];
         return new int[2]{
         Convert.ToInt32(token[1].ToString()),
         Convert.ToInt32(token.Split(',', ')')[1]) };
      }
      void WriteError(int numString, string ErrorMessage)
      {
         //Console.WriteLine("\n(line " + numString.ToString() + "): \"" + ErrorMessage + "\"");
         f.WriteLine("\n(line " + numString.ToString() + "): \"" + ErrorMessage + "\"");
      }
   }
}
