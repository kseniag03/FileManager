using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    /// <summary>
    /// Команды запрашивают у пользователя ввод пути или выбор опции 
    /// для дальнейшей работы с данными.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Печатает текущий путь.
        /// </summary>
        static void PrintCurrentPath()
        {
            Console.WriteLine($"Текущий путь: {s_currentPath}" + '\n');
            ClearScreen();
        }

        /// <summary>
        /// Печатает список дисков и просит пользователя выбрать диск по номеру.
        /// </summary>
        static void DisplayAllDrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            int num = 1;
            foreach (DriveInfo drive in drives)
            {
                if (!s_disks.ContainsKey(num))
                {
                    s_disks.Add(num, drive);
                }
                Console.WriteLine($"{num} - {drive.Name}");
                num++;
            }
            Console.WriteLine();
            Console.WriteLine("Выберите номер диска, в котором хотите продолжить работу.");
            Console.WriteLine("Если введено значение не из списка, " +
                $"программа выберет диск по умолчанию ({s_currentDisk})");
            string numInput = Console.ReadLine();
            int key = int.TryParse(numInput, out _) ? int.Parse(numInput) : 1;
            SelectDrive(key);
        }

        /// <summary>
        /// Выводит список доступных кодировок и просит пользователя выбрать кодировку по номеру.
        /// </summary>
        /// <returns> Выбранную пользователем кодировку </returns>
        static Encoding DisplayAllEncodings()
        {
            foreach (KeyValuePair<int, Encoding> pair in s_encodings)
            {
                Console.WriteLine($"{pair.Key} - {pair.Value.WebName}");
            }
            Console.WriteLine();
            Console.WriteLine("Выберите номер кодировки, в которой хотите читать файлы.");
            Console.WriteLine("Если введено значение не из списка, " +
                $"программа выберет кодировку по умолчанию ({Encoding.UTF8.WebName})");
            string numInput = Console.ReadLine();
            int key = int.TryParse(numInput, out _) ? int.Parse(numInput) : 1;
            return SelectEncoding(key);
        }

        /// <summary>
        /// Задаёт кодировку консоли, используемую при чтении входных данных и при записи выходных данных.
        /// </summary>
        static void ChooseInputOutputEncoding()
        {
            try
            {
                Console.InputEncoding = DisplayAllEncodings();
                Console.OutputEncoding = Console.InputEncoding;
            }
            catch (Exception exception)
            {
                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;
                Console.WriteLine($"Ошибка 110: {exception.Message}" + '\n');
            }
            Console.WriteLine($"Кодировка для чтения файлов с консоли: {Console.InputEncoding.WebName}");
            Console.WriteLine($"Кодировка для записи файлов с консоли: {Console.OutputEncoding.WebName}");
            ClearScreen();
        }

        /// <summary>
        /// Просит пользователя ввести путь к нужной директории.
        /// </summary>
        static void DirectoryChoice()
        {
            Console.WriteLine("Введите путь к нужной директории");
            s_currentPath = s_currentDisk.ToString();
            Console.Write($"{s_currentDisk}");
            string pathInput = Console.ReadLine();
            string path = Path.Combine(s_currentPath, pathInput);
            try
            {
                if (!Directory.Exists(path))
                {
                    CreateDirectoryByPath(path);
                }
                try
                {
                    s_currentPath = path;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Ошибка 101: {exception.Message}" + '\n');
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 100: {exception.Message}" + '\n');
            }
            ClearScreen();
        }

        /// <summary>
        /// Выводит в выходной файл и в консоль все файлы директории.
        /// </summary>
        /// <param name="path"> Путь к директории </param>
        /// <param name="pattern"> Маска, по которой выводятся файлы </param>
        static void DisplayFilesInDirectory(string path, string pattern)
        {
            s_output = "";
            Console.WriteLine("Текущий путь:" + '\n' + $"{s_currentPath}" + '\n');
            try
            {
                DirectoryInfo dirInfo = new(path.Length > 0 ? path : "../../../");
                GetFiles(dirInfo, pattern);
                Console.WriteLine("Проход завершён, проверьте выходной файл." + '\n');
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Недопустимые символы в пути" + '\n');
                s_output += "Недопустимые символы в пути" + '\n';
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 200: {exception.Message}" + '\n');
                s_output += $"Ошибка 200: {exception.Message}" + '\n';
            }
            File.WriteAllText("../../../output.txt", s_output);
            Console.WriteLine(s_output);
            ClearScreen();
        }

        /// <summary>
        /// Выводит в выходной файл и в консоль файлы и поддиректории директории.
        /// </summary>
        /// <param name="path"> Путь к директории </param>
        /// <param name="pattern"> Маска, по которой выводятся файлы </param>
        static void DirectoryContent(string path, string pattern)
        {
            s_recStartCount = 0;
            s_output = "";
            Console.WriteLine("Текущий путь:" + '\n' + $"{s_currentPath}" + '\n');
            Console.WriteLine("Глубина обхода:");
            string depthInput = Console.ReadLine();
            s_depth = int.TryParse(depthInput, out s_depth) ? int.Parse(depthInput) : 1;
            Console.WriteLine();
            try
            {
                DirectoryInfo dirInfo = new(path.Length > 0 ? path : "../../../");
                int length = s_depth;
                DriveInfo driveInfo = new(path);
                Console.WriteLine("Пожалуйста, подождите...");
                GoThroughDirectories(dirInfo, length, pattern);
                Console.WriteLine("Проход завершён, проверьте выходной файл." + '\n');
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Недопустимые символы в пути" + '\n');
                s_output += "Недопустимые символы в пути" + '\n';
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 300: {exception.Message}" + '\n');
                s_output += $"Ошибка 300: {exception.Message}" + '\n';
            }
            ClearScreen();
            File.WriteAllText("../../../output.txt", s_output);
            Console.WriteLine(s_output);
            ClearScreen();
        }

        /// <summary>
        /// Просит пользователя ввести путь к файлу.
        /// </summary>
        /// <returns> Строку, содержащую путь к файлу </returns>
        static string GetFilePath()
        {
            Console.WriteLine("Введите путь к файлу");
            s_currentPath = s_currentDisk.ToString();
            Console.Write($"{s_currentDisk}");
            string input = Console.ReadLine();
            string path = Path.Combine(s_currentPath, input);
            return path;
        }

        /// <summary>
        /// Получает данные из файла и печатает построчно в заданной кодировке.
        /// </summary>
        /// <param name="encoding"> Заданная кодировка </param>
        static void AskFilePath(Encoding encoding)
        {
            string path = GetFilePath();
            Console.Clear();
            string[] fileContent = DisplayFileContent(path, encoding);
            foreach (string line in fileContent)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
            ClearScreen();
        }

        /// <summary>
        /// Просит пользователя ввести место, куда скопировать или переместить файл.
        /// </summary>
        /// <param name="action"> Действие: копирование или перемещение </param>
        static void AskToCopy(string action)
        {
            string path = GetFilePath();
            FileInfo fileInfo = new(path);
            if (fileInfo.Length <= FileSizeLimit)
            {
                Console.WriteLine($"Введите путь к директории, в которую хотите "
                + ((action == "copy") ? "скопировать" : "переместить") +
                $" {fileInfo.Name}");
                string inputPath = Console.ReadLine();
                Console.WriteLine();
                if (action == "copy")
                {
                    CopyFile(fileInfo, inputPath);
                }
                else
                {
                    MoveFile(fileInfo, inputPath);
                }
            }
            else
            {
                Console.WriteLine($"Операция будет долго выполняться. Выберите файл меньшего размера." + '\n');
            }
            ClearScreen();
        }

        /// <summary>
        /// Спрашивает пользователя, стоит ли заменить файл или отменить копирование, 
        /// если в выбранной директории уже существует файл с таким названием.
        /// </summary>
        /// <param name="fileInfo"> Информация о файле </param>
        /// <returns> <c>true</c>, если выбрано копирование с заменой, иначе <c>false</c> </returns>
        static bool AskToReplace(FileInfo fileInfo)
        {
            Console.WriteLine($"Присутствует файл с таким же названием: {fileInfo.Name}" +
                +'\n' + "1 – Заменить файл" + '\n' + "2 – Отменить копирование" + '\n');
            bool commandChoiceFlag = true;
            bool resultFlag = false;
            while (commandChoiceFlag)
            {
                Console.Write("Введите номер команды: ");
                string commandInput = Console.ReadLine();
                Console.WriteLine();
                switch (commandInput)
                {
                    case "1":
                        commandChoiceFlag = false;
                        resultFlag = true;
                        break;
                    case "2":
                        commandChoiceFlag = false;
                        resultFlag = false;
                        break;
                    default:
                        commandChoiceFlag = true;
                        Console.WriteLine("Выбрана несуществующая команда." + '\n');
                        break;
                }
            }
            return resultFlag;
        }

        /// <summary>
        /// Осуществляет переход к удалению файла, выбранного пользователем.
        /// </summary>
        static void AskToDelete()
        {
            string path = GetFilePath();
            FileInfo file = new(path);
            try
            {
                DeleteFile(file);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Нет доступа к {path}." + '\n');
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Недопустимые символы в пути" + '\n');
                s_output += "Недопустимые символы в пути" + '\n';
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 310: {exception.Message}" + '\n');
                s_output += $"Ошибка 310: {exception.Message}" + '\n';
            }
            ClearScreen();
        }

        /// <summary>
        /// Просит пользователя ввести маску.
        /// </summary>
        /// <returns> Строку, содержащую маску </returns>
        static string GetMask()
        {
            Console.WriteLine("Введите маску.");
            string maskInput = Console.ReadLine();
            Console.WriteLine($"Введённая маска: \"{maskInput}\"");
            return maskInput;
        }

        /// <summary>
        /// Просит пользователя ввести текст построчно для записи в файл.
        /// </summary>
        /// <returns> Введённый текст или значение по умолчанию, 
        /// если пользователь ничего не ввёл </returns>
        static string GetText()
        {
            string text = "";
            bool addTextFlag = true;
            while (addTextFlag)
            {
                Console.WriteLine("Введите текст");
                string lineInput = Console.ReadLine();
                text += lineInput;
                Console.Clear();
                Console.WriteLine("Добавить ещё строку?" + '\n' + "1 – Да" + '\n' + "2 – Нет" + '\n');
                string commandInput = Console.ReadLine();
                Console.WriteLine();
                switch (commandInput)
                {
                    case "1":
                        text += '\n';
                        break;
                    case "2":
                        addTextFlag = false;
                        break;
                    default:
                        addTextFlag = true;
                        break;
                }
            }
            if (text != "")
            {
                return text;
            }
            return @"какой-то text ¯\_(ツ)_/¯ йЁ _to file#\+~ お前はもう死んでいる ᕕ( ᐛ )ᕗ";
        }

        /// <summary>
        /// Создаёт файл в выбранной пользователем директории в заданной кодировке.
        /// </summary>
        /// <param name="encoding"> Заданная кодировка </param>
        static void AskToCreateFile(Encoding encoding)
        {
            Console.WriteLine("Введите путь к нужной директории");
            s_currentPath = s_currentDisk.ToString();
            Console.Write($"{s_currentDisk}");
            string input = Console.ReadLine();
            string path = Path.Combine(s_currentPath, input);
            Console.Clear();
            try
            {
                if (!Directory.Exists(path))
                {
                    CreateDirectoryByPath(path);
                }
                try
                {
                    Console.WriteLine("Введите имя файла");
                    string inputFileName = Console.ReadLine();
                    path = Path.Combine(path, inputFileName);
                    string text = GetText();
                    using var file = new StreamWriter(path, true, encoding);
                    Console.WriteLine(path);
                    file.WriteLine(text);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Ошибка 401: {exception.Message}" + '\n');
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 400: {exception.Message}" + '\n');
            }
            ClearScreen();
        }

        /// <summary>
        /// Выводит в выходной файл и консоль данные из нескольких файлов, введённых пользователем.
        /// </summary>
        static void JoinFiles()
        {
            s_output = "";
            List<string> fileConcatenation = new();
            bool addFileFlag = true;
            while (addFileFlag)
            {
                string path = GetFilePath();
                Console.Clear();
                string[] fileContent = DisplayFileContent(path, Encoding.UTF8);
                fileConcatenation.Add(string.Join('\n', fileContent));
                Console.WriteLine("Добавить ещё один файл?" + '\n' + "1 – Да" + '\n' + "2 – Нет" + '\n');
                string commandInput = Console.ReadLine();
                Console.WriteLine();
                switch (commandInput)
                {
                    case "1":
                        fileConcatenation.Add("\n");
                        break;
                    case "2":
                        addFileFlag = false;
                        break;
                    default:
                        addFileFlag = true;
                        break;
                }
                if (string.Join('\n', fileConcatenation).Length > LinesLimit)
                {
                    Console.WriteLine("Слишком много строк в файле. Принудительный выход");
                    break;
                }
            }
            ClearScreen();
            foreach (string line in fileConcatenation)
            {
                s_output += line;
            }
            File.WriteAllText("../../../output.txt", s_output);
            Console.WriteLine(s_output);
            ClearScreen();
        }
    }
}