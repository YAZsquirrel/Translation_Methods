using System;
namespace Lab_1
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
      }   
   }
}
