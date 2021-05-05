using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_1
{
   class VariableTable
   {
      struct ATTRIBUTS
      {
         public string type { get; set; }     // Определяет тип идентификатора
         public bool value;      // Определяет имеет ли идентификатор значение
         public bool change;     // Определяет можно ли менять значение идентификатора
      };

      // Словарь. Ключ - идентификатор. Значение - атрибуты
      private readonly Dictionary<string, ATTRIBUTS> variable_table;

      public VariableTable()     // Создание переменной таблицы
      {
         variable_table = new Dictionary<string, ATTRIBUTS>();
      }

      public int Add(string name)   // Добавление элемента в таблицу
      {
         try
         {
            variable_table.Add(name, new ATTRIBUTS());
            return variable_table.Count - 1;    // идентификатор добавлен
         }
         catch
         {
            return -1;   // Такой идентификатор уже есть в таблице
         }
      }

      public bool SetType(string id, string _type)    // Запись типа данного идентификатора
      {
         if (SearchIsExist(id))      // Есть ли такой идентификатор в таблице
         {
            // Запись атрибута
            variable_table.TryGetValue(id, out ATTRIBUTS atr);
            if (atr.type == null)
               variable_table[id] = new ATTRIBUTS { change = atr.change, value = atr.value, type = _type };
            else
            {
               //Console.WriteLine("Данному элементу уже задан тип"); 
               return false;
            }
            return true;
         }
         else
         {
            Console.WriteLine("Данного элемента нет в таблице");
            return false;
         }
      }

      public bool SetValue(string id, bool _value)  // Запись имеет ли значение данный идентификатор
      {
         if (SearchIsExist(id))    // Есть ли такой идентификатор в таблице
         {
            // Запись атрибута
            variable_table.TryGetValue(id, out ATTRIBUTS atr);
            variable_table[id] = new ATTRIBUTS { change = atr.change, value = _value, type = atr.type };
            return true;
         }
         else
         {
            Console.WriteLine("Данного элемента нет в таблице");
            return false;
         }
      }

      public bool SetChange(string id, bool _change)    // Запись может ли быть изменен данный идентификатор
      {
         if (SearchIsExist(id))     // Есть ли такой идентификатор в таблице
         {
            // Запись атрибута
            variable_table.TryGetValue(id, out ATTRIBUTS atr);
            variable_table[id] = new ATTRIBUTS { change = _change, value = atr.value, type = atr.type };
            return true;
         }
         else
         {
            Console.WriteLine("Данного элемента нет в таблице");
            return false;
         }
      }
      public bool SearchIsExist(string id)   // Поиск по идентификатору
      {
         return variable_table.ContainsKey(id);
      }
      public bool HasType(string id)
      {
         variable_table.TryGetValue(id, out ATTRIBUTS atr);
         return atr.type != null;
      }

      // what_search определяет по каким атрибутам производить поиск.
      // Первый символ определяет искать ли по ATTRIBUTS.type (1 - искать; 0 - не искать)
      // Второй символ определяет искать ли по ATTRIBUTS.value (1 - искать; 0 - не искать)
      // Третий символ определяет искать ли по ATTRIBUTS.change (1 - искать; 0 - не искать)
      public List<string> SearchAttribut(string type, bool value, bool change, bool[] what_search)  // Поиск индентификаторов по атрибутам.
      {
         List<string> identifiers = new List<string>();

         foreach (string key in variable_table.Keys)
         {
            ATTRIBUTS atr_in_table = variable_table[key];   // Атрибуты данного ключа таблицы
            string find_key = key;

            if (what_search[0])  // Искать ли по типу?
            {
               if (atr_in_table.type != type)
                  find_key = null;
            }

            if (what_search[1]) // Искать ли по значению(имеется или нет)?
            {
               if (atr_in_table.value != value)
                  find_key = null;
            }

            if (what_search[2]) // Искать по изменению(может меняться значение или нет)
            {
               if (atr_in_table.change != change)
                  find_key = null;
            }

            if (find_key != null)   // Если нашелся идентификатор соответствующий атрибутам
            {
               identifiers.Add(find_key);
            }
         }
         return identifiers;
      }

      public void WriteInfo(string name)     // Выводит информацию по идентификатору
      {
         if (SearchIsExist(name))
         {
            ATTRIBUTS atr = variable_table[name];
            Console.WriteLine($"Имя: {name}. Тип: {atr.type}. Имеет значение? {atr.value}. Можно изменять значение? {atr.change}");
         }
         else
         {
            Console.WriteLine("Данного элемента нет в таблице");
         }
      }

      public void WriteAll()
      {
         Console.WriteLine("\nТаблица содержит в себе: ");
         for (int id = 0; id < variable_table.Count; id++)
         {
            Console.WriteLine(SearchNameById(id)); 
         }
      }
      public string SearchNameById(int id) => variable_table.ElementAt(id).Key;
      public int SearchIdByName(string name)
      {
         for (int id = 0; id < variable_table.Count; id++)
         {
            if (SearchNameById(id) == name) return id;
         }
         return Add(name);
      }

   }
}
