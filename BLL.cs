using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    /// <summary>
    /// Команды, отвечающие за работу с данными.
    /// </summary>
    partial class Program
    {
        // Растяжение символов для наглядного представления иерархии директории.    
        const int LinesStretch = 5;
        // Ограничение на кол-во шагов рекусии.
        const int RecursionLimit = 10000;
        // Ограничение на размер файла.
        const long FileSizeLimit = 1 * 1024 * 1024 * 1024;
        // Ограничение на кол-во строк в файле.
        const int LinesLimit = 500000;

        // Словарь дисков с порядковым номером.
        private static readonly Dictionary<int, DriveInfo> s_disks = new();
        // Словарь кодировок с порядковым номером.
        private static readonly Dictionary<int, Encoding> s_encodings = new()
        {
            { 1, Encoding.UTF8 },
            { 2, Encoding.Unicode },
            { 3, Encoding.UTF32 },
            { 4, Encoding.ASCII }
        };

        // Текущий диск.
        private static DriveInfo s_currentDisk = DriveInfo.GetDrives().Length > 0 ? DriveInfo.GetDrives()[0] : default;
        // Текущий путь.
        private static string s_currentPath = s_currentDisk.ToString();
        // Глубина обхода директории.
        private static int s_depth = default;
        // Кол-во запусков рекурсии.
        private static int s_recStartCount = default;
        // Пишет сообщение, если в рекурсии слишком много шагов.
        private static bool s_recExitMessageFlag = true;
        // Запись содержимого директории или результат конкатенации файлов.
        private static string s_output = default;

        /// <summary>
        /// Выбирает текущий диск из словаря по ключу.
        /// </summary>
        /// <param name="key"> Ключ </param>
        static void SelectDrive(int key)
        {
            if (s_disks.TryGetValue(key, out DriveInfo value))
            {
                s_currentDisk = value;
            }
            else
            {
                Console.WriteLine($"Значение по ключу {key} не найдено.");
            }
        }

        /// <summary>
        /// Выбирает кодировку из словаря по ключу.
        /// </summary>
        /// <param name="key"> Ключ </param>
        /// <returns> Выбранную кодировку </returns>
        static Encoding SelectEncoding(int key)
        {
            if (!s_encodings.TryGetValue(key, out Encoding value))
            {
                Console.WriteLine($"Значение по ключу {key} не найдено.");
                value = Encoding.UTF8;
            }
            Console.WriteLine('\n' + $"Текущая кодировка – {value.WebName}" + '\n');
            return value;
        }

        /// <summary>
        /// Получает список файлов директории по заданной маске, записывая его в выходной файл.
        /// </summary>
        /// <param name="root"> Корневая папка </param>
        /// <param name="pattern"> Маска, по которой выбираются файлы </param>
        static void GetFiles(DirectoryInfo root, string pattern)
        {
            FileInfo[] files = null;
            try
            {
                files = root.GetFiles(pattern);
            }
            catch (UnauthorizedAccessException)
            {
                s_output += $"Нет доступа к {root}" + '\n';
            }
            catch (DirectoryNotFoundException)
            {
                s_output += $"Директория {root} не найдена" + '\n';
            }
            catch (Exception exception)
            {
                s_output += $"Ошибка 500: {exception.Message}" + '\n';
            }
            foreach (var file in files)
            {
                s_output += file.Name + '\n';
            }
        }

        /// <summary>
        /// Выводит символ заданное кол-во раз.
        /// </summary>
        /// <param name="length"> Кол-во символов </param>
        /// <param name="symbol"> Символ </param>
        /// <returns></returns>
        static string DrawLines(int length, string symbol)
        {
            string lines = "";
            for (var i = 0; i < length * LinesStretch; i++)
            {
                lines += symbol;
            }
            return lines;
        }

        /// <summary>
        /// Совершает обход по директориям, записывая в выходной файл иерархию файлов и поддиректорий.
        /// </summary>
        /// <param name="root"> Корневая папка (начало обхода) </param>
        /// <param name="depth"> Глубина обхода </param>
        /// <param name="pattern"> Маска, по которой выбираются файлы </param>
        static void GoThroughDirectories(DirectoryInfo root, int depth, string pattern)
        {
            s_recStartCount++;
            FileInfo[] files = null;
            DirectoryInfo[] subDirs;
            string lines = (pattern == "") ? DrawLines(s_depth - depth, " ") 
                + (s_depth != depth ? "├" : "") + DrawLines(s_depth - depth, "─") : "";
            try
            {
                files = root.GetFiles(pattern);
            }
            catch (UnauthorizedAccessException)
            {
                s_output += lines + $"Нет доступа к {root}" + '\n';
            }
            catch (DirectoryNotFoundException)
            {
                s_output += lines + $"Директория {root} не найдена" + '\n';
            }
            catch (Exception exception)
            {
                s_output += $"Ошибка 600: {exception.Message}" + '\n';
            }
            if (files != null && depth > 0)
            {
                subDirs = root.GetDirectories();
                foreach (var dirInfo in subDirs)
                {
                    if (pattern == "")
                    {
                        s_output += lines + "[" + dirInfo.Name + "]" + '\n';
                    }
                    if (s_recStartCount <= RecursionLimit)
                    {
                        GoThroughDirectories(dirInfo, depth - 1, pattern);
                    }
                    else if (s_recExitMessageFlag)
                    {
                        s_recExitMessageFlag = false;
                        Console.WriteLine("Слишком много шагов рекурсии. Вывод будет неполным." + '\n');
                    }
                }
                foreach (var file in files)
                {
                    s_output += lines + file.Name + '\n';
                }
            }
        }

        /// <summary>
        /// Читает построчно файл в заданной кодировке.
        /// </summary>
        /// <param name="path"> Путь к файлу </param>
        /// <param name="encoding"> Заданная кодировка </param>
        /// <returns> Массив строк, содержащих данные файла </returns>
        static string[] DisplayFileContent(string path, Encoding encoding)
        {
            List<string> lines = new();
            try
            {
                using StreamReader sReader = new(path, encoding, false);
                string line;
                while ((line = sReader.ReadLine()) != null)
                {
                    lines.Add(line);
                    if (string.Join('\n', lines).Length > LinesLimit)
                    {
                        Console.WriteLine("Слишком много строк в файле." + '\n' + "Вывод будет неполным.");
                        ClearScreen();
                        break;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Нет доступа к директории {path}." + '\n');
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 700: {exception.Message}" + '\n');
            }
            return lines.ToArray();
        }

        /// <summary>
        /// Создаёт директорию по заданному пути.
        /// </summary>
        /// <param name="path"> Путь к директории </param>
        static void CreateDirectoryByPath(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Нет доступа к директории {path}." + '\n');
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Директория {path} не найдена." + '\n');
            }
            catch (IOException)
            {
                Console.WriteLine("Ошибка ввода-вывода." + '\n');
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Недопустимое значение." + '\n');
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 800: {exception.Message}" + '\n');
            }
        }

        /// <summary>
        /// Копирует файл в указанную директорию (если такой директории нет, она создаётся).
        /// </summary>
        /// <param name="file"> Сам файл </param>
        /// <param name="path"> Путь к директории, в которую надо скопировать файл </param>
        static void CopyFile(FileInfo file, string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    CreateDirectoryByPath(path);
                }
                try
                {
                    path = Path.Combine(path, file.Name);
                    FileInfo fileInfo = new(path);
                    if (fileInfo.Exists)
                    {
                        if (AskToReplace(fileInfo))
                        {
                            file.CopyTo(path, true);
                        }
                    }
                    else
                    {
                        file.CopyTo(path);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Ошибка 901: {exception.Message}" + '\n');
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка 900: {exception.Message}" + '\n');
            }
        }

        /// <summary>
        /// Перемещает файл в указанную директорию.
        /// </summary>
        /// <param name="file"> Сам файл </param>
        /// <param name="path"> Путь к директории, в которую надо переместить файл </param>
        static void MoveFile(FileInfo file, string path)
        {
            CopyFile(file, path);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        /// <summary>
        /// Удаляет файл, если такой существует.
        /// </summary>
        /// <param name="file"> Сам файл </param>
        static void DeleteFile(FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
            else
            {
                Console.WriteLine($"Файл {file.Name} не найден.");
            }
        }
    }
}