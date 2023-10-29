using StatementGenerator54.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using word = Microsoft.Office.Interop.Word;

namespace StatementGenerator54.ClassHelper {
    public class WordHelper {
        public enum StatementType {
            Exam, // Экзамен
            ComplexExam, // Комплексный экзамен
            Coursework, // Курсовая
            Test // Зачёт
        }

        private FileInfo _fileInfo;
        public WordHelper(string fileName) {
            //Application app = new word.Application();
            fileName = System.IO.Path.GetFullPath(fileName);
            if(File.Exists(fileName)) {
                _fileInfo = new FileInfo(fileName);
            }
            else {
                throw new ArgumentException("File not found");
            }
        }

        public static void TextChanger(
            string savePath,
            string teacher, 
            string subject, 
            string group, 
            string special, 
            string course, 
            List<Student> students, 
            StatementType statementType,
            string teacher2 = "",
            string subject2 = ""
        ) {
            string file = "./Statements/";
            switch(statementType) {
                case StatementType.Exam:
                    file += "Экзаменационная ведомость.doc";
                break;
                case StatementType.ComplexExam:
                    file += "Экзаменационная ведомость (комплексный экзамен).doc";
                break;
                case StatementType.Coursework:
                    file += "Курсовая ведомость.docx";
                break;
                case StatementType.Test:
                    file += "Зачётная ведомость.doc";
                break;

                default:
                    throw new Exception("Invalid statement type!");
            }
            var helper = new WordHelper(file);

            var items = new Dictionary<string, string>()
            {
                { "<teacher>", teacher },
                { "<group>", group },
                { "<special>", special },
                { "<courseNumber>", course },
                { "<subject>", subject },
            };

            helper.Process(items, students, savePath);
        }

        public void Process(Dictionary<string, string> items, List<Student> students, string savePath) {
            int studCount = students.Count;
            int lastIndex = 1;
            int offset = 0;
            Dictionary<string, string> studs = new Dictionary<string, string> { };

            for(int i = 1; i <= studCount; i++) {
                if(!students[i - 1].AddToList) {
                    offset++;
                    continue;
                }

                items.Add($"<student{i - offset}>", students[i - 1].FullName);
                lastIndex = i - offset;
            }

            for(int i = lastIndex + 1; i <= 26; i++) {
                items.Add($"<student{i}>", "");
            }

            ProcessKiller processKiller = new ProcessKiller();
            processKiller.CreateDontKillProcess();

            word.Application? app = null;
            try {
                app = new word.Application();

                Object file = _fileInfo.FullName;
                Object missing = Type.Missing;

                app.Documents.Open(file);

                foreach(var item in items) {
                    word.Find find = app.Selection.Find;
                    find.Text = item.Key;
                    find.Replacement.Text = item.Value;

                    Object wrap = word.WdFindWrap.wdFindContinue;
                    Object replace = word.WdReplace.wdReplaceAll;

                    find.Execute(FindText: missing,
                        MatchCase: false,
                        MatchWholeWord: false,
                        MatchWildcards: false,
                        MatchSoundsLike: missing,
                        MatchAllWordForms: false,
                        Forward: true,
                        Wrap: wrap,
                        Format: false,
                        ReplaceWith: missing, Replace: replace);
                }

                savePath = Uri.UnescapeDataString(savePath);
                app.ActiveDocument.SaveAs2(savePath);
                app.ActiveDocument.Close();

            }
            catch(Exception ex) {
                Console.WriteLine(ex.Message.ToString());
            }
            finally {
                if(app != null) {
                    app.Quit();
                }
                processKiller.KillProcess(app!);
            }
        }
    }
}
