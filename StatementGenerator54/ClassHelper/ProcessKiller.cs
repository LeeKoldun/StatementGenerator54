using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StatementGenerator54.ClassHelper
{
    internal class ProcessKiller
    {

        public List<Process> dontkill = new List<Process>();
        public Process[] procs = Process.GetProcessesByName("WINWORD");

        public void CreateDontKillProcess()
        {            
            foreach (Process p in procs)
            {
                dontkill.Add(p);
            }
        }
        public void KillProcess(Application app)
        {
            procs = Process.GetProcessesByName("WINWORD");
            foreach (Process p in procs)
            {
                if (!dontkill.Contains(p))
                {
                    p.Kill();
                }
            }
        }
    }
    
    
    
}