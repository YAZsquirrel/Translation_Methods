using System;
using System.IO;

namespace MT
{
   class Program
   {
      static void Main()
      {
         
         ConstantTable CTOperators = new ConstantTable($"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\Operators.txt");
         ConstantTable CTDivisions = new ConstantTable($"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\Divisions.txt");
         ConstantTable CTKeyWoeds = new ConstantTable($"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\KeyWords.txt");
         VariableTable VTConstants = new VariableTable();
         VariableTable VTIdentificators = new VariableTable();

         Scanner scanner = new Scanner(CTOperators, CTDivisions, CTKeyWoeds, VTConstants, VTIdentificators);
         scanner.LexicalAnalysis($"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\program.txt");

         SyntaxAnalysis analyzator = new SyntaxAnalysis(CTOperators, CTDivisions, CTKeyWoeds, VTConstants, VTIdentificators);
         analyzator.WorkSyntAn();

         CleanOPZ($"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\OPZ.txt", $"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\Errors.txt");
         Generator generator = new Generator($"C:\\Users\\pm82k\\source\\repos\\LoPaMoT\\LoPaMoT\\OPZ.txt", CTOperators, VTConstants, VTIdentificators);
         generator.startGeneration();
      }   

      static void CleanOPZ(string OPZ, string Error)
      {
         string[] OPZm = File.ReadAllLines(OPZ);
         string[] Errorm = File.ReadAllLines(Error);
         StreamWriter OPZf = new StreamWriter(OPZ);
         foreach (string Errori in Errorm)
         {
            if (Errori != "")
            {
               string[] TextinError = Errori.Split(" ");
               string numLine = TextinError[1].Split(")")[0];
               int numL = Convert.ToInt32(numLine);
               OPZm[numL - 1] = "";
            }
         }
         foreach (string OPZi in OPZm)
         {
            if (OPZi != "")
            {
               OPZf.WriteLine(OPZi);
            }
         }
         OPZf.Close();
      }
   }
}
