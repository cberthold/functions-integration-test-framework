using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Functions.Integration.Test.Framework.Loggers
{
    public class TestOutputLoggerProvider : ILoggerProvider
    {
        public TestOutputLoggerProvider(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(OutputHelper);
        }

        public void Dispose()
        {
        }
    }
}
