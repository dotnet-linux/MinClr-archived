using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MinClr.Init
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            Console.WriteLine(
                "Hello World from init process based on .NET Runtime.");

            Console.ReadKey();
        }
    }
}
