using log4net.Core;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Logger
{
    public class TestService
    {
        private readonly ILogger<TestService> _logger;

        public TestService(ILogger<TestService> logger)
        {
            _logger = logger;
        }
        public void DoWork()
        {
            _logger.LogInformation("I started working");

            _logger.LogWarning("This is just a warning");

            _logger.LogError("Something went wrong");
        }
    }
}
