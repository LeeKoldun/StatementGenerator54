using StatementGenerator54.ClassHelper;
using StatementGenerator54.Model;
using Microsoft.Office.Interop.Word;
using word = Microsoft.Office.Interop.Word;
using StatementGenerator54.ClassHelper;
using System;
using System.IO;

namespace Testing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CmdRunner.Execute(CmdRunner.ParserType.StudentParser,
            "Список студентов.xlsx",
            "23-24");

            CmdRunner.Execute(CmdRunner.ParserType.TeacherParser,
            "Тарификация на 2022-2023 от  01.02.2023.xlsx",
            "ВБ;ВБ точечники;Бюджет");

            var con = new Context();
            var students = con.Students("students.json");
            var teachers = con.Teachers("teachers.json");

            Console.Write("Выберите ведомость(1-экзаменационная;2-зачетная)");
            int index = Convert.ToInt32(Console.ReadLine());

            Console.Write("Введите группу: ");
            string groupIn = Console.ReadLine();

            Console.Write("Введите имя преподователя: ");
            string teacherIn = Console.ReadLine();

            Console.Write("Введите дисциплину: ");
            string disciplineIn = Console.ReadLine();

            var filteredStudents = students.Where(e => e.Group == groupIn);

            string group = filteredStudents.First().Group;
            string course = filteredStudents.First().Course;
            string specialization = filteredStudents.First().Specialization;

            string teacher = teachers.First(e => e.FullName.Contains(teacherIn)).FullName;

            string subj = teachers.First(e => e.FullSubjectName.Contains(disciplineIn)).FullSubjectName;

            TextChanger(teacher, subj, group, specialization, course, filteredStudents.ToList(), index);

        }
        public static void TextChanger(string teacher, string subject, string group, string special, string course, List<Student> students
            ,int index)
        {
            string file;
            if (index == 1)
            {
                file = "Экзаменационная ведомость.doc";
            }
            else
            {
                file = "Зачётная ведомость.doc";
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

            helper.Process(items, students);
        }
    }
}