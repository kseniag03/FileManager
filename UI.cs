using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    /// <summary>
    /// Вывод правил и ветвление команд.
    /// </summary>
    partial class Program
    {
        // Строки с группами команд.
        static readonly string s_backCommand = "0 – вернуться к выбору команд" + '\n';
        static readonly string s_mainCommands = "1 – просмотр списка дисков компьютера и выбор диска"
            + '\n' + "2 – переход в другую директорию"
            + '\n' + "3 – вывод списка файлов в текущей директории" + '\n'
            + "4 – вывод списка файлов и поддиректорий в текущей директории" + '\n'
            + "5 – выбор кодировки консоли" + '\n';
        static readonly string s_fileCommands = "1 – вывод содержимого текстового файла в консоль в кодировке UTF-8"
            + '\n' + "2 – вывод содержимого текстового файла в консоль в выбранной " +
            "пользователем кодировке" + '\n' + "3 – копирование файла в выбранную пользователем директорию" + '\n' +
            "4 – перемещение файла в выбранную пользователем директорию" + '\n' + "5 – удаление файла"
            + '\n' + "6 – создание простого текстового файла в кодировке UTF-8"
            + '\n' + "7 – создание простого текстового файла в выбранной " +
            "пользователем кодировке" + '\n' +
            "8 – конкатенация содержимого двух или более текстовых файлов и " +
            "вывод результата в консоль в кодировке UTF-8" + '\n';
        static readonly string s_extraCommands = "1 – выполнить вывод файлов в текущей директории по заданной маске"
                + '\n' + "2 – выполнить вывод файлов и поддиректорий в текущей директории по заданной маске" + '\n';

        /// <summary>
        /// Чистит экран, когда пользователь нажимает любую клавишу.
        /// </summary>
        static void ClearScreen()
        {
            Console.WriteLine("Нажмите любую клавишу, чтобы продолжить.");
            Console.ReadKey();
            Console.Clear();
        }

        /// <summary>
        /// Выводит пользователю правила программы.
        /// </summary>
        static void ProgramRules()
        {
            Console.WriteLine("  Добро пожаловать в программу «Файловый менеджер»." + '\n');
            Console.WriteLine("  Чуть более подробное описание посмотрите в файле \"ReadMe.txt\"" + '\n' + '\n' + 
                "  Некоторые ограничения:" + '\n' + '\n' + "• Вывод всех файлов и поддиректорий директории работает " +
                $"через рекурсию. Если число шагов рекурсии \nпревысит {RecursionLimit}, то она " +
                $"завершится принудительно, и программа будет выводить неполный список. " +
                "\nВ таком случае вы можете выбрать либо другую папку, либо меньшую глубину обхода." + '\n' +
                "\n• В некоторых частях кода не предусмотрен повторный ввод значения в случае неправильного вывода" +
                " \n(например, выбор номера диска или кодировки). " +
                "Вместо этого программа использует значения по умолчанию."
                + '\n' + "\n• При перемещении и копировании файла будет учитываться его размер. " +
                "Чтобы не пришлось долго ждать завершения операции, " +
                $"\nучитываются только файлы размера не более {FileSizeLimit} байт." + '\n' +
                $"\n• Программа не будет выводить более {LinesLimit} строк файла или результата конкатенации файлов."
                + '\n' + "\n• Вывод определённых файлов будет осуществляться по маскам, а не регулярным выражениям." +
                "\nВ масках всего два служебных символа: \"?\" – ноль символов или ровно один произвольный символ; " +
                '\n' + "\"*\" – любая последовательность символов длины от 0 и более. " + '\n' + '\n' 
                + " Важное уточнение: программа была написана на Windows." + '\n');
            ClearScreen();
        }

        /// <summary>
        /// Начало программы, выбор группы команд.
        /// Можно завершить программу и посмотреть список всех команд, а также текущий путь.
        /// </summary>
        static void ProgramStart()
        {
            Console.WriteLine("0 – завершить работу с программой" + '\n' + "1 – посмотреть список команд" + '\n' 
                + "2 – выбрать одну из основных команд" + '\n' + "3 – выбрать одну из команд по работе с файлами"
                + '\n' + "4 – выбрать одну из дополнительных команд" + '\n' + "5 – посмотреть текущий путь" + '\n');
            Console.Write("Введите номер команды: ");
            string commandInput = Console.ReadLine();
            Console.WriteLine();
            switch (commandInput)
            {
                case "0":
                    s_startFlag = false;
                    Console.WriteLine("finish");
                    break;
                case "1":
                    Console.Clear();
                    CommandList();
                    break;
                case "2":
                    Console.Clear();
                    MainCommands();
                    break;
                case "3":
                    Console.Clear();
                    FileCommands();
                    break;
                case "4":
                    Console.Clear();
                    ExtraCommands();
                    break;
                case "5":
                    PrintCurrentPath();
                    break;
                default:
                    Console.WriteLine("Выбрана несуществующая команда." + '\n');
                    ClearScreen();
                    break;
            }
        }

        /// <summary>
        /// Выводит список всех команд.
        /// </summary>
        static void CommandList()
        {
            Console.WriteLine(s_backCommand);
            Console.WriteLine("Основные команды:");
            Console.WriteLine(s_mainCommands);
            Console.WriteLine("Команды для работы с файлами:");
            Console.WriteLine(s_fileCommands);
            Console.WriteLine("Дополнительные команды:");
            Console.WriteLine(s_extraCommands);
            ClearScreen();
        }

        /// <summary>
        /// Выводит список основных команд и осуществляет переход к ним.
        /// </summary>
        static void MainCommands()
        {
            Console.WriteLine(s_backCommand + s_mainCommands);
            Console.Write("Введите номер команды: ");
            string commandInput = Console.ReadLine();
            Console.WriteLine();
            switch (commandInput)
            {
                case "0":
                    Console.Clear();
                    ProgramStart();
                    break;
                case "1":
                    DisplayAllDrives();
                    s_currentPath = s_currentDisk.ToString();
                    ClearScreen();
                    break;
                case "2":
                    DirectoryChoice();
                    break;
                case "3":
                    DisplayFilesInDirectory(s_currentPath, "");
                    break;
                case "4":
                    DirectoryContent(s_currentPath, "");
                    break;
                case "5":
                    ChooseInputOutputEncoding();
                    break;
                default:
                    Console.WriteLine("Выбрана несуществующая команда." + '\n');
                    ClearScreen();
                    break;
            }
        }

        /// <summary>
        /// Выводит список команд работы с файлами и осуществляет переход к ним.
        /// </summary>
        static void FileCommands()
        {
            Console.WriteLine(s_backCommand + s_fileCommands);
            Console.Write("Введите номер команды: ");
            string commandInput = Console.ReadLine();
            Console.WriteLine();
            switch (commandInput)
            {
                case "0":
                    Console.Clear();
                    ProgramStart();
                    break;
                case "1":
                    AskFilePath(Encoding.UTF8);
                    break;
                case "2":
                    AskFilePath(DisplayAllEncodings());
                    break;
                case "3":
                    AskToCopy("copy");
                    break;
                case "4":
                    AskToCopy("move");
                    break;
                case "5":
                    AskToDelete();
                    break;
                case "6":
                    AskToCreateFile(Encoding.UTF8);
                    break;
                case "7":
                    AskToCreateFile(DisplayAllEncodings());
                    break;
                case "8":
                    JoinFiles();
                    break;
                default:
                    Console.WriteLine("Выбрана несуществующая команда." + '\n');
                    ClearScreen();
                    break;
            }
        }

        /// <summary>
        /// Выводит список дополнительных команд и осуществляет переход к ним.
        /// </summary>
        static void ExtraCommands()
        {
            Console.WriteLine(s_backCommand + s_extraCommands);
            Console.Write("Введите номер команды: ");
            string commandInput = Console.ReadLine();
            Console.WriteLine();
            switch (commandInput)
            {
                case "0":
                    Console.Clear();
                    ProgramStart();
                    break;
                case "1":
                    DisplayFilesInDirectory(s_currentPath, GetMask());
                    break;
                case "2":
                    DirectoryContent(s_currentPath, GetMask());
                    break;
                default:
                    Console.WriteLine("Выбрана несуществующая команда." + '\n');
                    ClearScreen();
                    break;
            }
        }
    }
}