using Microsoft.Office.Interop.Word;
using StatementGenerator54.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using word = Microsoft.Office.Interop.Word;

namespace StatementGenerator54.ClassHelper
{
    public class WordHelper
    {
        private FileInfo _fileInfo;
        public WordHelper(string fileName)
        {
            //Application app = new word.Application();
            fileName = System.IO.Path.GetFullPath(fileName);
            if (File.Exists(fileName))
            {
                _fileInfo = new FileInfo(fileName);
            }
            else
            {
                throw new ArgumentException("File not found");
            }
        }

        public void Process(Dictionary<string,string> items, List<Student> students)
        {
            int studCount = students.Count;
            int lastIndex = 1;
            Dictionary<string, string> studs = new Dictionary<string, string>{ };

            for (int i = 1; i <= studCount; i++)
            {
                items.Add($"<student{i}>", students[i - 1].FullName);
                lastIndex = i;
            }

            for (int i = lastIndex + 1; i <= 26; i++)
            {
                items.Add($"<student{i}>", "");
            }

            ProcessKiller processKiller = new ProcessKiller();
            processKiller.CreateDontKillProcess();

            word.Application app = null;
            try
            {
                app = new word.Application();

                Object file = _fileInfo.FullName;
                Object missing = Type.Missing;

                app.Documents.Open(file);

                foreach (var item in items)
                {
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

                

                Object newFileName = Path.Combine(_fileInfo.DirectoryName!, DateTime.Now.ToString("yyyMMdd HHmmss") + _fileInfo.Name);
                app.ActiveDocument.SaveAs2(newFileName);
                app.ActiveDocument.Close();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            finally
            {
                if (app != null)
                {
                    app.Quit();
                }
                processKiller.KillProcess(app);
            }
        }
    }
}
