using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Consoler
{
    public class Runner
    {
        private readonly ILogger<Runner> _logger;

        public Runner(ILogger<Runner> logger)
        {
            _logger = logger;
        }

        public void Log(string name)
        {
            _logger.LogDebug(20, "Doing hard work! {Action}", name);
        }
    }
}
