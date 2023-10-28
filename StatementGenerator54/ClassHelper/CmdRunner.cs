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
        public enum ParserType {
            StudentParser,
            TeacherParser,
        }

        public static void Execute(ParserType parserType, string filePath, string listSheet)
        {
            string cmdPath = "./Parser/";
            switch(parserType) {
                case ParserType.StudentParser:
                    cmdPath += "StudentParser/main.exe";
                break;
                case ParserType.TeacherParser:
                    cmdPath += "TeacherAndSubjectsParser/main.exe";
                break;

                default:
                    throw new Exception("Invalid parser type!");
            }

            if(!File.Exists(cmdPath)) throw new Exception($"Can't find parser on path: {cmdPath}");

            filePath = Uri.UnescapeDataString(filePath);
            var proc = Process.Start(cmdPath, $"\"{filePath}\" \"{listSheet}\"");
            proc.WaitForExit();
            proc.Close();
        }

    }
}
