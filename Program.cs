using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Logger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var loger = new TestService(new LoggerFactory().CreateLogger<TestService>());
            loger.DoWork();
        }
    }
}
