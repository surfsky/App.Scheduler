using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace App.Consoler
{
    public static class Sys
    {
        // 是否已经存在进程实例
        public static bool AreadyExistInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            string currentFileName = currentProcess.MainModule.FileName;
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in processes)
                if (process.MainModule.FileName == currentFileName)
                    if (process.Id != currentProcess.Id)
                        return true;
            return false;
        }
    }
}
