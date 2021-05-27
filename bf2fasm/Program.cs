using System;
using System.IO;

namespace bf2exe
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = File.ReadAllText(args[0]);
            string asmcode = (new bf2fasm(code).compileBf());
            FileInfo f = new FileInfo(args[0]);
            File.WriteAllText($"{f.Name.Remove(f.Name.Length - f.Extension.Length)}.asm", asmcode);
        }
    }
}
