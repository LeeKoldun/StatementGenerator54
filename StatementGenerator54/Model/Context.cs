using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace StatementGenerator54.Model
{
    public class Context
    {
        public List<Teacher> Teachers(string jsonPath)
        {
            string finalJson = "";
            using (StreamReader streamReader = new StreamReader(jsonPath))
            {
                finalJson = streamReader.ReadToEnd();
            }

            List<Teacher> teachers = new List<Teacher>();
            teachers = JsonConvert.DeserializeObject<List<Teacher>>(finalJson);


            return teachers;


        }

        public List<Student> Students(string jsonPath)
        {
            string finalJson = "";
            using (StreamReader streamReader = new StreamReader(jsonPath))
            {
                finalJson = streamReader.ReadToEnd();
            }

            List<Student> teachers = new List<Student>();
            teachers = JsonConvert.DeserializeObject<List<Student>>(finalJson);


            return teachers;
        }

        public List<Student> GroupList(string jsonPath)
        {
            string finalJson = "";
            using (StreamReader streamReader = new StreamReader(jsonPath))
            {
                finalJson = streamReader.ReadToEnd();
            }

            var groups = JsonConvert.DeserializeObject<List<Student>>(finalJson).GroupBy(x => x.Group);

            List<Student> groupsList = new List<Student>();

            foreach (var group in groupsList)
            {
                groupsList.Add(group);
            }

            return groupsList;
        }
    }
}
