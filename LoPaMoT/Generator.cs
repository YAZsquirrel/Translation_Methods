using System;
using System.Collections.Generic;
using System.IO;

namespace MT
{
   class Generator
   {
      string[] OPZ = { };
      int N_new_var = 0;
      int num_ST = 0;
      int num_db = 0;
      VariableTable Constants;
      VariableTable Identificators;
      ConstantTable Operations;
      StreamWriter fOut;
      public Generator(string name_file, ConstantTable Operations, VariableTable Constants, VariableTable Identificators)
      {
         ReadOPZ(name_file);
         this.Constants = Constants;
         this.Identificators = Identificators;
         this.Operations = Operations;
         fOut = new StreamWriter($"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\program.asm");
      }

      // Перед чтением ОПЗ необходимо очистить файл от ошибочных ОПЗ
      void ReadOPZ(string name_file)
      {
         OPZ = File.ReadAllLines(name_file);
      }

      void beginProgram()
      {
         Console.WriteLine(".386");
         Console.WriteLine(".MODEL FLAT");
         fOut.WriteLine(".386");
         fOut.WriteLine(".MODEL FLAT");
         Console.WriteLine();
         fOut.WriteLine();
      }

      void dataProgram()
      {
         Console.WriteLine(".DATA");
         fOut.WriteLine(".DATA");
         Console.WriteLine("; Переменные из С++");
         fOut.WriteLine("; Переменные из С++");
         foreach(var variable in Identificators.variable_table.Keys)
         {
            Console.Write(variable + " ");
            fOut.Write(variable + " ");
            if (Identificators.GetTypeByName(variable) == "int")
            {
               Console.WriteLine("DD ?");
               fOut.WriteLine("DD ?");
            }
            if (Identificators.GetTypeByName(variable) == "float")
            {
               Console.WriteLine("REAL8 ?");
               fOut.WriteLine("REAL8 ?");
            }
            if (Identificators.GetTypeByName(variable) == "char")
            {
               Console.WriteLine("DB ?");
               fOut.WriteLine("DB ?");
            }
         }

         Console.WriteLine("; Константы из С++");
         fOut.WriteLine("; Константы из С++");
         foreach (var constant in Constants.variable_table.Keys)
         {
            Console.Write("Const" + Constants.SearchIdByName(constant).ToString());
            fOut.Write("Const" + Constants.SearchIdByName(constant).ToString());

            if (Constants.GetTypeByName(constant) == "int")
            {
               Console.WriteLine(" DD " + constant);
               fOut.WriteLine(" DD " + constant);
            }
            if (Constants.GetTypeByName(constant) == "float")
            {
               Console.WriteLine(" REAL8 " + constant);
               fOut.WriteLine(" REAL8 " + constant);
            }
            if (Constants.GetTypeByName(constant) == "char")
            {
               Console.WriteLine(" DB " + constant + ", 0");
               fOut.WriteLine(" DB " + constant + ", 0");
            }
         }
         Console.WriteLine();
         fOut.WriteLine();
      }

      void codeProgram()
      {
         Console.WriteLine(".CODE");
         Console.WriteLine("MAIN PROC");
         Console.WriteLine("FINIT");
         fOut.WriteLine(".CODE");
         fOut.WriteLine("MAIN PROC");
         fOut.WriteLine("FINIT");
         for (int i = 0; i < OPZ.Length; i++)
         {
            Console.WriteLine("\n; Выражение " + i);
            fOut.WriteLine("\n; Выражение " + i);
            Console.WriteLine("; Начало выражения");
            fOut.WriteLine("; Начало выражения");
            string[] OPZ_i = OPZ[i].Split(" ");
            Stack<string> OPZ_stack = new Stack<string>();
            for (int j = 0; j < OPZ_i.Length; j++)
            {
               // Если ОПЗ_i - операция
               if (Operations.SearchIsExist(OPZ_i[j]))
               {
                  string[] operands = { OPZ_stack.Pop(), OPZ_stack.Pop() };
                  OPZ_i[j] = processingOperation(OPZ_i[j], operands);
               }


              
               OPZ_stack.Push(OPZ_i[j]);
            }
            Console.WriteLine("; Конец выражения\n");
            fOut.WriteLine("; Конец выражения\n");
         }


         Console.WriteLine("END MAIN");
         fOut.WriteLine("END MAIN");
      }
      public void startGeneration()
      {
         beginProgram();
         dataProgram();
         codeProgram();
         fOut.Close();
      }

      int writeST()
      {

         if (num_ST > 7)
         {
            Console.WriteLine("Error: переполнение стека ST");
            fOut.WriteLine("Error: переполнение стека ST");
            return -1;
         }
         if (num_ST > 0)
         {
            Console.Write("; ");
            fOut.Write("; ");
            for (int i = 0; i < num_ST; i++)
            {
               Console.Write("ST(" + i + ")");
               fOut.Write("ST(" + i + ")");
            }
         }
         Console.WriteLine();
         fOut.WriteLine();
         
         return 0;
      }

      void PushST(string operand)
      {
         // Если первый операнд int или char
         if (Constants.GetTypeByName(operand) == "int" || Constants.GetTypeByName(operand) == "char" ||
              Identificators.GetTypeByName(operand) == "int" || Identificators.GetTypeByName(operand) == "char")
         {
            if (Constants.SearchIsExist(operand))
               operand = "Const" + Constants.SearchIdByName(operand).ToString();
            Console.Write("FILD " + operand);
            fOut.Write("FILD " + operand);
         }
         else if (Constants.GetTypeByName(operand) == "float" || Identificators.GetTypeByName(operand) == "float")
         {
            if (Constants.SearchIsExist(operand))
               operand = "Const" + Constants.SearchIdByName(operand).ToString();
            Console.Write("FLD " + operand);
            fOut.Write("FLD " + operand);
         }
         else
         {
            Console.Write("Type Error!");
            fOut.Write("Type Error!");
         }
         num_ST++;
         writeST();
      }

      void PopST(string operand)
      {
         // Если первый операнд int или char
         if (Constants.GetTypeByName(operand) == "int" || Constants.GetTypeByName(operand) == "char" ||
              Identificators.GetTypeByName(operand) == "int" || Identificators.GetTypeByName(operand) == "char")
         {
            if (Constants.SearchIsExist(operand))
               operand = "Const" + Constants.SearchIdByName(operand).ToString();
            Console.Write("FISTP " + operand);
            fOut.Write("FISTP " + operand);
         }
         else if (Constants.GetTypeByName(operand) == "float" || Identificators.GetTypeByName(operand) == "float")
         {
            if (Constants.SearchIsExist(operand))
               operand = "Const" + Constants.SearchIdByName(operand).ToString();
            Console.Write("FSTP " + operand);
            fOut.Write("FSTP " + operand);
         }
         else
         {
            Console.Write("Type Error!");
            fOut.Write("Type Error!");
         }
         num_ST--;
         writeST();
      }

      // Операнд всегда 2, поэтому ожидаем что размер массива operands = 2
      string processingOperation(string operation, string[] operands)
      {
         if (operation == "=")
         {
            if (!operands[0].Contains("new_var_"))
               PushST(operands[0]);
            PopST(operands[1]);
            return operands[1];
         }
         if (!operands[1].Contains("new_var_"))
            PushST(operands[1]);
         if (!operands[0].Contains("new_var_"))
            PushST(operands[0]);

         if (operation == "+" || operation == "-" || operation == "*")
         {
            if (operation == "+")
            {
               Console.WriteLine("; Сумма");
               fOut.WriteLine("; Сумма");
               Console.Write("FADD ");
               fOut.Write("FADD ");
               num_ST--;
               writeST();
            }

            if (operation == "-")
            {
               Console.WriteLine("; Разность");
               fOut.WriteLine("; Разность");
               Console.Write("FSUB ");
               fOut.Write("FSUB ");
               num_ST--;
               writeST();
            }

            if (operation == "*")
            {
               Console.WriteLine("; Умножение");
               fOut.WriteLine("; Умножение");
               Console.Write("FMUL ");
               fOut.Write("FMUL ");
               num_ST--;
               writeST();
            }
         }
         else
         {
            Console.Write("FCOMPP");
            fOut.Write("FCOMPP");
            num_ST -= 2;
            writeST();
            Console.WriteLine("FSTSW AX");
            fOut.WriteLine("FSTSW AX");
            Console.WriteLine("SAHF");
            fOut.WriteLine("SAHF");

            if (operation == "==")
            {
               Console.Write("JE ");
               fOut.Write("JE ");
            }

            if (operation == "!=")
            {
               Console.Write("JNE ");
               fOut.Write("JNE ");
            }

            if (operation == ">")
            {
               Console.Write("JG ");
               fOut.Write("JG ");
            }

            if (operation == "<")
            {
               Console.Write("JL ");
               fOut.Write("JL ");
            }

            Console.WriteLine("TRUE_" + num_db);
            fOut.WriteLine("TRUE_" + num_db);
            Console.Write("FLDZ");
            fOut.Write("FLDZ");
            num_ST++;
            writeST();
            Console.WriteLine("JMP END_" + num_db);
            fOut.WriteLine("JMP END_" + num_db);
            Console.WriteLine("TRUE_" + num_db + ":");
            fOut.WriteLine("TRUE_" + num_db + ":");
            Console.Write("FLD1");
            fOut.Write("FLD1");
            writeST();
            Console.WriteLine("END_" + num_db + ":");
            fOut.WriteLine("END_" + num_db + ":");
            num_db++;
         }

         string new_var = "new_var_" + N_new_var;
         N_new_var++;
         writeST();
         return new_var;
      }
   }
}

