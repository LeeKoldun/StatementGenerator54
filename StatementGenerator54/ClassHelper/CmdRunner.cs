 using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementGenerator54.ClassHelper
{
    public class CmdRunner
    {
        public const string STUDENT_PARSER = "./Parser/StudentParser/main.exe";
        public const string TEACHER_PARSER = "./Parser/TeacherAndSubjectsParser/main.exe";
        public static void Execute(string cmdPath, string filePath, string listSheet)
        {
            if(!File.Exists(cmdPath)) return;

            filePath = Uri.UnescapeDataString(filePath);
            var proc = Process.Start(cmdPath, $"\"{filePath}\" \"{listSheet}\"");
            proc.WaitForExit();
            proc.Close();
        }

    }
}
