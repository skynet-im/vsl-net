using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSLTest
{
    public static class Program
    {
        public const ushort Port = 32761;
        public const string Keypair = "<RSAKeyValue><Modulus>qBQQScN/+A2Tfi971dmOyPwSXpoq3XVwQBJNzbCCk1ohGVoOzdNK87Csw3thZyrynfaDzujW555S4HkWXxLR5dzo8rj/6KAk0yugYtFMt10XC1iZHRQACQIB3j+lS5wK9ZHfbsE4+/CUAoUdhYa9cad/xEbYrgkkyY0TuZZ1w2piiE1SdOXB+U6NF1aJbkUtKrHU2zcp5YzhYlRePvx7e+GQ5GMctSuT/xFzPpBZ5DZx1I/7lQicq7V21M/ktilRQIeqIslX98j4jLuFriinySwW+oi0s+8hantRwZ9jgAIIEao9+tbDSj8ePHb0Li6hhuoMmLeImLaoadDG39VnFQ==</Modulus><Exponent>AQAB</Exponent><P>z5tqnHWJ4X0lZVBXtVxJNI/h5NLBrwDoS0bvFL0Mx1SkYjF4Rjy8EGrLA51W+C8TzEY7ddBiC+eQPXw1PlbzHg+h0hal2gp5iUj+QEvCw1vDVXGoGTeP6UBL8ixYTbLQaVG70rWPm7j2nR7sQSQgJHX4ppvKQ4Mo9DI1RnJ1/2U=</P><Q>z0HXU22CFiUhoxuWuePjtkJ2gtopsZu5x6F/I+EqBqnq8KVVp+qRKOHm34xbh3gTQjDcBtJXu+FGgKRvQWj5ArhMt2QtNKIhmRBIuRQoHWSwg0deMPzD9IUHDU8D4xwkoZWuAGFjWW5KrkW6TX6SMHM8GUMnGzGP50MbIrEHBfE=</Q><DP>zvoJbfcZAb+82qcg6mUZbtfLxFACXTEwZmxPy4M3DDtsr6DWYmAGtu9hezcQD9sPh+a1PR4FwgyZF1OP2ZjiRSQcltGRhDJRPPeS1BM0F4SS18q6Znmodklt7gEcAEq30Wh1MvtkM0JSTA8aR0925CLhRWmoW2qWF+8+gf93eKk=</DP><DQ>U+5p8NMsFyO6V39YrrbnBGwt6hfHQrG5rmpsPm90wXYWOpX59iI73r587JK+jkHGKsv2jpyoAuHb10S/+VE1ZjCUgMAEvofZ60545NqQ1DZudPt13Yi/Ikqs7GrPPC2td/JRoL3PqevMOn7qT2+ubAh+kgxrzctoZ1L5rjbajUE=</DQ><InverseQ>o/VbhG+A+MtSe1qNCsgv41bCSVVJyzJH+lC/j3hYksjwFJEimDu6D+MheFU/PcBER1IoomUnyUwqYfK7YLmb3JHt9nCmnUUx+OrOT81TRhs63kGm2UKMwY7vNOIvhjfsbmoeTr0Of0Mc/Pf62lp1PzJaJtCao67zC5VTLt+e16I=</InverseQ><D>BkuXSMmYzvr9/n17gajwCZqZYVY1/n/1NM0kTizLIzo+hmzPV6NPMB2HejXlkf/mwO0roCt4tLzcshnCJJleAVV65/AI071ymHJoNwAYXVjQMcvyeWD9pFi6wBVTSCe/m4i7nRiBg7w0MWKR41jgQRpeAhIjCcrmLnwvrcvGVhiXLys4vw/XEPEc5Yk7ZWUVHRDr/2f1+AEL1T7kkDPY002qIDrP2NJbRGMpNulDt1xB1qcnK0VLgQ87zOTzZEUQviYCgvZjf3xnkYG1j87acaFQlNMN6pqJGAdD158rATy99OzScORgKbYNXtx1GGc1Yzj+alaszH3xBOpghTSscQ==</D></RSAKeyValue>";
        public const string PublicKey = "<RSAKeyValue><Modulus>qBQQScN/+A2Tfi971dmOyPwSXpoq3XVwQBJNzbCCk1ohGVoOzdNK87Csw3thZyrynfaDzujW555S4HkWXxLR5dzo8rj/6KAk0yugYtFMt10XC1iZHRQACQIB3j+lS5wK9ZHfbsE4+/CUAoUdhYa9cad/xEbYrgkkyY0TuZZ1w2piiE1SdOXB+U6NF1aJbkUtKrHU2zcp5YzhYlRePvx7e+GQ5GMctSuT/xFzPpBZ5DZx1I/7lQicq7V21M/ktilRQIeqIslX98j4jLuFriinySwW+oi0s+8hantRwZ9jgAIIEao9+tbDSj8ePHb0Li6hhuoMmLeImLaoadDG39VnFQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        public static VSL.ThreadSafeList<Client> Clients;
        public static FrmMain MainInstance;
        public static int Connects = 0;
        public static int Disconnects = 0;
        public static string TempPath;

        static Program()
        {
            Clients = new VSL.ThreadSafeList<Client>();
            if (Directory.Exists(Path.Combine("D:", "ProgramData")))
                TempPath = Path.Combine("D:", "ProgramData", "VSLTest");
            else
                TempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "VSLTest");
            Directory.CreateDirectory(TempPath);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainInstance = new FrmMain();
            Application.Run(MainInstance);
        }
    }
}