using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileManager
{
    /// <summary>
    /// Запускает программу.
    /// </summary>
    partial class Program
    {
        // Флаг для повторного запуска программы.
        static bool s_startFlag = true;

        /// <summary>
        /// Точка входа программы.
        /// </summary>
        static void Main()
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            ProgramRules();
            do
            {
                try
                {
                    ProgramStart();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Ошибка 000: {exception.Message}" + '\n');
                }
            } while (s_startFlag);
        }
    }
}