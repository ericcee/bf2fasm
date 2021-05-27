using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bf2exe
{
    
    public class bf2fasm
    {
        string topcode = "format PE CONSOLE 4.0\ninclude 'include\\win32w.inc'\nentry start\nsection '.text' code readable executable\nstart:\ninvoke GetStdHandle,-11\ninvoke SetConsoleMode,eax,4 or 1\nxor eax,eax\nxor edx, edx\nxor ecx,ecx\nmov ebx, memory\n";
        string bottocode = "prgrmend:\ncinvoke exit, -1\nincmaddr:\n"+
            "add ebx, ecx\nmov dl,[ebx]\nret\ndecmaddr:\n"+
            "sub ebx, ecx\nmov dl, [ebx]\nret\nincdata:\n"+
            "mov dl,[ebx]\nadd dl, cl\nmov [ebx], dl\nxor ecx,ecx\nret\n"+
            "decdata:\nmov dl,[ebx]\nsub dl, cl\nmov [ebx], dl\nxor ecx,ecx\n"+
            "ret\nputchar:\npusha\ncinvoke putch, edx\npopa\nret\ngetchar:\n"+
            "pusha\npush ebx\ninvoke getch\npop ebx\nmov [ebx], al\npopa\nret\n"+
            "section '.data' data readable writeable\nmemory: times 1024*10 db 0\n"+
            "section '.idata' import data readable writeable\nlibrary msvcrt,'MSVCRT.DLL',kernel32,'kernel32.dll'\n"+
            "import msvcrt, putch,'_putch', getch,'_getch', exit,'_exit'\n"+
            "import kernel32,GetStdHandle,'GetStdHandle',SetConsoleMode,'SetConsoleMode'";


        string midcode = string.Empty;

        string code = string.Empty;
        Stack<int> routines = new Stack<int>();
        int newroutcount = 0;
        int codepos = 0;
        List<Tuple<char, int>> compressedBf = new List<Tuple<char, int>>();

        public bf2fasm(string code)
        {
            this.code = code;
        }

        public void compressBF()
        {
            char curr = '\0';
            int times = 0;
            foreach(char x in code)
            {
                if (",.+-[]<>".Contains(x))
                {
                    if (curr == ']' || curr == '[')
                    {
                        compressedBf.Add(new Tuple<char, int>(curr, 1));
                        times = 0;
                    }
                    else if (curr != x && curr != '\0')
                    {
                        compressedBf.Add(new Tuple<char, int>(curr, times));
                        times = 0;
                    }
                    curr = x;
                    times++;
                }
            }
            compressedBf.Add(new Tuple<char, int>(curr, times));
        }

        public string compileBf()
        {
            compressBF();
            while(codepos != compressedBf.Count)
            {
                midcode += getNextCode();
            }
            return topcode + midcode + bottocode;
        }

        private string getNextCode()
        {
            string retstr = string.Empty;
            Tuple<char, int> cblock = compressedBf[codepos];
            switch (cblock.Item1)
            {
                case '+':
                    retstr = $"mov cl,{cblock.Item2}\ncall incdata\n";
                    break;
                case '-':
                    retstr = $"mov cl,{cblock.Item2}\ncall decdata\n";
                    break;
                case ',':
                    retstr = "call getchar\n";
                    break;
                case '.':
                    retstr = "call putchar\n";
                    break;
                case '[':
                    retstr = $"routine_{newroutcount}_begin:\ncmp dl, 0\nje routine_{newroutcount}_end\n";
                    routines.Push(newroutcount);
                    newroutcount++;
                    break;
                case ']':
                    int rc = routines.Pop();
                    retstr = $"cmp dl, 0\njne routine_{rc}_begin\nroutine_{rc}_end:\n";
                    break;
                case '<':
                    retstr = $"mov ecx,{cblock.Item2}\ncall decmaddr\n";
                    break;
                case '>':
                    retstr = $"mov ecx,{cblock.Item2}\ncall incmaddr\n";
                    break;
                default:
                    break;
            }
            codepos++;
            return retstr;
        }

    }
}
