using System;
using System.Collections.Generic;
using System.IO;

namespace Lab_1
{
   class SyntaxAnalysis
   {
      string tokensAll;
      // Номера таблиц
      public ConstantTable Operations;  // 0
      public ConstantTable Divisions;   // 1
      public ConstantTable KeyWords;    // 2
      VariableTable Constants;          // 3
      VariableTable Identificators;     // 4
      StreamWriter f;
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
         // Чтение всех токенов (как целый текст)
         if (tokensAll.Length > 1) tokensAll = tokensAll.Remove(tokensAll.Length - 1).Replace("\r", ""); //.Replace("\n", "");
         string[] tokens = { };
         // Чтение токенов в массив, по одному в элементе
         if (tokensAll.Length > 1) tokens = tokensAll.Remove(tokensAll.Length - 1).Split('|', '\n');


         int[] token;   // Хранит два числа: первое - номер таблицы, второе - номер элемента в таблице

         bool start = false;    // Было ли начало программы
         bool end = false;      // Была ли окончена программа
         bool init = false;     // Инизиализация
                                //bool assignment = false;   
         bool oper = false;     // Оператор
         bool type = false;     // Обьявление типа
         bool d_type = false;     // повторное Обьявление типа
         bool ID = false;       // Обьявление идентификатора
         bool isSemiNeeded = false;       // отсутствует ли ";"?
         bool enumeration = false;        // Перечисление
         bool init_end = false;
         bool oper_end = false;

         Stack<int[]> opers = new Stack<int[]>();   // стэк операций

         string type_s = "";    // тип 

         string id = "";             // идентификатор
         int[] idtoken = { };   // идентификатор

         int line = 1;          // номер строки
         for (int numToken = 0; numToken < tokens.Length; numToken++)
         {
            if (tokens[numToken] == "")     // Если нет токена
            {
               line++;
               continue;    // Переходим к следующему токену
            }

            string token_s = GetToken(token = ReadToken(tokens, numToken)); // Определяем имя токена

            if (end)
            {
               WriteError(line, "После конца программы");
               break;
            }

            if (token_s == "void main(){")      // Если начало функции
            {
               start = true;
               continue;    // Переходим к следующему токену
            }

            if (token_s == "}")     // Если конец программы
            {
               end = true;
               break;       // Конец синтаксического анализа
            }

            if (!start)     // Если есть символы перед начальным токеном
            {
               WriteError(line, "Неверное начало программы");
               continue;    // Ищем начало программы
            }

            if (start)   // Программа была начата
            {
               if (token_s == "void main(){")
               {
                  WriteError(line, "Повторное обьявление программы");
                  continue;    // Переходим к следующему символу
               }

               // ???
               //if (Constants.SearchIsExist(token_s))    // Токен - константа?
               //    isSemiNeeded = true;

               // ???
               //if (token_s == "(" || token_s == ")")    // Токен - скобочка?
               //    isSemiNeeded = false;


               if (KeyWords.SearchIsExist(token_s))     // Токен - тип?
               {
                  if (init)
                     d_type = true;
                  init = true;    // Переходим к инициализации
                  
                  if (isSemiNeeded) WriteError(line - 1, "Требуется \";\"");  
               }

               if (init && d_type || oper && init)
               {
                  WriteError(line - 1, "Требуется \";\"");
                  isSemiNeeded = true;
                  init_end = true;
                  oper_end = true;
               }

               if (token_s == ";" && token[0] == 1 || isSemiNeeded) // конец строки
               {
                  if (!init_end && init)
                  {
                     WriteError(line, "Требуется идентификатор");
                  }

                  if (!oper_end && oper)
                  {
                     WriteError(line, "Требуется идентификатор или константа");
                  }
                  init = false;
                  init_end = false;
                  type = false;
                  ID = false;
                  isSemiNeeded = false;
                  oper = false;
                  enumeration = false;

                  while (pop_operations(opers) == 0) ;// Вывод стека операций

                  if (pop_operations(opers) == 2) // Если найдена открывающаяся скобка
                  {
                     WriteError(line, "Ожидается закрывающаяся скобочка");
                     opers.Pop();
                  }
                  while (pop_operations(opers) == 0) ; // Вывод стека операций
                  Console.WriteLine();
                  if (!d_type) continue;
               }

               
               if (init || d_type) // line
               {
                  if (d_type) 
                  {
                     d_type = false;
                     init = true;
                  }
                  // Если при инициализации были заполнены и идентификатор и тип
                  if (ID && type) // var asd /////// по идее без type,
                                  // т.к. {var asd = 1} и {asd = 1} -> {TYPE ID = CONST} и {ID = CONST}
                  {
                     if (token_s == "=" && !enumeration) // ожидание оператора
                     {
                        oper = true;
                        type = false;
                        init_end = true;

                        //init = false;
                     }
                     else 
                     {
                        if (token_s == ",") // ожидание перечисления
                        {
                           enumeration = true;
                           ID = false;
                           init_end = false;
                           continue;
                        }
                        else
                        {
                           WriteError(line, "Неожиданный символ \"" + token_s + "\"");     // ошибка
                           continue;
                        }
                     }
                  }

                  if (enumeration || type)
                  {
                     if (Constants.SearchIsExist(token_s))
                     {
                        WriteError(line, "Присвоение константе типа");
                        continue;
                     }
                     ID = true;
                     id = token_s;
                     init_end = true;
                     idtoken = token;
                     if (!Identificators.SetType(id, type_s))
                        WriteError(line, "Повторная инициализация \"" + token_s + "\""); // ошибка
                  }

                  if (!type)
                  {
                     type = true;
                     type_s = token_s;
                  }
                  
               }

               if (!init && !oper)
               {
                  if (ID)
                  {
                     if (token_s == "=")
                     {
                        oper = true;
                        //ID = false;
                        init = true;
                     }
                     else
                     {
                        WriteError(line, "Неожиданный символ\"" + token_s + "\"");
                     }

                  }

                  if (Identificators.SearchIsExist(token_s) && !ID)
                  {
                     if (Constants.SearchIsExist(token_s))
                     {
                        WriteError(line, "Присвоение константе типа");
                        continue;
                     }
                     ID = true;
                     id = token_s;
                     idtoken = token;
                  }
               }

               if (oper)
               {
                  if (init) // Если прошла инициализация
                  {
                     WritePostfix(idtoken, opers);
                     init = false;
                     Identificators.SetValue(id, true);
                     //isSemiNeeded = false;
                  }

                  if (ID && (Operations.SearchIsExist(token_s) || Divisions.SearchIsExist(token_s)) || !ID)
                  {
                     ID = false;
                     oper_end = false;
                     int error_in_oper = WritePostfix(token, opers);

                     if (error_in_oper == 4)
                        WriteError(line, "Использование переменной без значения\"" + token_s + "\"");
                     if (error_in_oper == 1)
                        WriteError(line, "Использование неинициализированной переменной \"" + token_s + "\"");

                     if (error_in_oper == 2)
                        WriteError(line, "Не найдена открывающаяся скобка");

                     if (error_in_oper == 3 || error_in_oper == 1)
                     {
                        ID = true;
                        oper_end = true;
                     }
                  }
                  else
                  {
                     WriteError(line, "Ожидается знак операции или скобка");
                     continue;
                  }
               }
            }
         }
         if (!end)
         {
            WriteError(line, "Требуется \"}\"");
         }
         f.Close();
      }

      int pop_operations(Stack<int[]> operations)
      {
         if (operations.Count == 0)
            return 1;   // конец стека (ошибка)

         if (GetToken(operations.Peek()) == "(")
            return 2;

         string pop_token = GetToken(operations.Pop());
         Console.Write(pop_token + " "); // postfix {OPS}
         return 0;   // Успешно вытолкнули элемент
      }
      int WritePostfix(int[] token, Stack<int[]> operations)
      {
         if (token[0] == 3)  // Если константа
         {
            Console.Write(Constants.SearchNameById(token[1]) + " "); // postfix {CONST}    //
            return 3;
         }

         if (token[0] == 4)    // Если идентификатор
         {
            string id = Identificators.SearchNameById(token[1]);
            Console.Write(id + " "); // postfix {ID}  //

            if (!Identificators.HasType(id))    // Если без значения (ошибка)
               return 1;
            if (!Identificators.HasValue(id))
               return 4;
            return 3;
         }

         if (token[0] == 1)  // Если скобки
         {
            string token_s = GetToken(token);

            if (token_s == "(")
            {
               operations.Push(token);
               return 0;
            }


            if (token_s == ")")
            {
               if (operations.Count == 0)
                  return 2;

               // Пока не дошли до открывающейся скобочки
               while (pop_operations(operations) == 0) ;

               if (pop_operations(operations) == 1)
                  return 2;

               operations.Pop();

               return 0;
            }


         }

         int priority_token = count_priority(token);     // Ищем приоритет операции

         int error_pop = 0;
         if (operations.Count != 0)  // Пока стек не пуст
            while (count_priority(operations.Peek()) >= priority_token)// Вывод операций больших по приоритету
            {
               error_pop = pop_operations(operations);
               if (error_pop != 0 || operations.Count == 0)
                  break;
            }


         operations.Push(token);     // добавление операции в стек
         return 0;
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
         else if (name == "=")
            return 1;
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
         f.WriteLine("\n(line " + numString.ToString() + "): \"" + ErrorMessage + "\"\n");
      }
   }
}
