using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MinClr.Init
{
    class Program
    {
        //[DllImport("libc")]
        //private static extern int fork();

        //[DllImport("libc")]
        //private static extern int execve(
        //    string pathname,
        //    string[] argv,
        //    string[] envp);

        //[DllImport("libc")]
        //private static extern void _exit(
        //    int status);

        //private static int CreateProcessFromInit(
        //    ProcessStartInfo startInfo)
        //{
        //    int pid = fork();

        //    if (pid == 0)
        //    {
        //        Directory.SetCurrentDirectory(startInfo.WorkingDirectory);

        //        List<string> envp = new List<string>();
        //        foreach (var item in startInfo.Environment)
        //        {
        //            envp.Add(string.Format("{0}={1}", item.Key, item.Value));
        //        }
        //        envp.Add(null);

        //        string[] argv = new string[] { "/usr/bin/bash", null };

        //        execve(argv[0], argv, envp.ToArray());

        //        // If failed to call execve, it will return the errno.
        //        _exit(Marshal.GetLastWin32Error());
        //    }

        //    return pid;
        //}


        [DllImport("libc")]
        private static extern int mount(
            string source, 
            string target,
            string filesystemtype,
            ulong mountflags, 
            string data);

        private const ulong MS_REMOUNT = 32;

        [DllImport("libc")]
        private static extern int umount2(
            string target, 
            int flags);

        private const int MNT_DETACH = 2;

        [DllImport("libc")]
        private static extern int wait(
            ref int wstatus);

        [DllImport("libc")]
        private static extern int waitpid(
            int pid, 
            ref int wstatus, 
            int options);

        [DllImport("libc")]
        private static extern int kill(
            int pid,
            int sig);

        static void Main(string[] args)
        {
            Console.Clear();

            Console.WriteLine(
                "MinClr.Init for .NET/Linux 0.1.1");
            Console.WriteLine(
                "(c) .NET/Linux Development Team. All rights reserved.");
            Console.WriteLine("");

            Console.WriteLine("The version of .NET/Linux: 0.1.2");
            Console.WriteLine("");

            Task ExecutorTask = Task.Run(() =>
            {
                Console.WriteLine("Make / writable.");
                if (-1 == mount(null, "/", null, MS_REMOUNT, null))
                {
                    Console.WriteLine("Failed to make / writable.");
                }

                Console.WriteLine("Detach the /tmp mount point from the ram disk.");
                if (-1 == umount2("/tmp", MNT_DETACH))
                {
                    Console.WriteLine(
                        "Failed to detach the /tmp mount point from the ram disk.");
                }

                Console.WriteLine("");

                Console.WriteLine("Starting DHCP deamon...");
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(
                        "/usr/sbin/dhcpcd");
                    Process.Start(startInfo).WaitForExit();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to start DHCP deamon. ({0})", e.Message);
                }

                Console.WriteLine("");

                Console.WriteLine("Starting PowerShell...");
                for (; ; )
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(
                            "/opt/minclr/powershell/pwsh",
                            "-NoLogo");
                        string OldPath = startInfo.Environment["PATH"];
                        startInfo.Environment["PATH"] =
                            "/opt/minclr/powershell:" + OldPath;
                        startInfo.Environment["HOME"] = "/root";
                        startInfo.WorkingDirectory = startInfo.Environment["HOME"];
                        Process.Start(startInfo).WaitForExit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to start PowerShell. ({0})", e.Message);
                    }
                }
            });

            while(true)
            {
                int wstatus = 0;
                int pid = wait(ref wstatus);
                if (pid > 1)
                {
                    kill(pid, 0);
                }
            }
        }
    }
}
