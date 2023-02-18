using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Functions.Integration.Test.Framework.Loggers
{
    public class TestOutputLogger : ILogger
    {
        public TestOutputLogger(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            OutputHelper.WriteLine(formatter(state, exception));
        }
    }
}
