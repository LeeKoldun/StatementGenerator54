using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementGenerator54.Model {
    public class SavePaths {
        public string StudentsXlsxPath { get; set; } = "";
        public List<string> StudentsLists { get; set; } = new List<string> { };
        public string TariffXlsxPath { get; set; } = "";
        public List<string> TariffsLists { get; set; } = new List<string>{ };
    }
}
