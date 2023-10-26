 using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementGenerator54.ClassHelper
{
    public class CmdRunner
    {
        public const string STUDENT_PARSER = "./Parser/StudentParser/main.exe";
        public const string TEACHER_PARSER = "./Parser/StudentParser/main.exe";
        public static void Execute(string cmdPath, string filePath, string listSheet)
        {
            // Создаем новый процесс для командной оболочки cmd
            //Process process = new Process();

            var proc = Process.Start(cmdPath, $"\"{filePath}\" \"{listSheet}\"");
            proc.Start();
            proc.WaitForExit();
            proc.Close();

            // Задаем параметры для процесса
            //ProcessStartInfo startInfo = new ProcessStartInfo
            //{
            //    FileName = "main.exe",  // Имя исполняемого файла (командной оболочки)
            //    RedirectStandardInput = true,
            //    RedirectStandardOutput = true,
            //    RedirectStandardError = true,
            //    UseShellExecute = false,  // Устанавливаем false, чтобы перенаправить ввод/вывод
            //    CreateNoWindow = true,    // Запускать без создания окна cmd
            //    Arguments = $"\"{filePath}\" \"{listSheet}\"",     // Передача аргумента "/C dir" для выполнения команды "dir"
            //};

            //process.StartInfo = startInfo;

            // Запускаем процесс
            //process.Start();

            // Завершаем процесс
            //process.WaitForExit();
            //process.Close();
        }

    }
}
