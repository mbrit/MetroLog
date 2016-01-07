﻿using System.Threading.Tasks;
using MetroLog.Internal;
using Xunit;

namespace MetroLog.Tests
{
    public class BrokenTargetTests
    {

        [Fact]
        public async Task TestBrokenTarget()
        {
            var testTarget = new TestTarget();

            var config = new LoggingConfiguration();
            config.AddTarget(LogLevel.Trace, LogLevel.Fatal, new BrokenTarget());
            config.AddTarget(LogLevel.Trace, LogLevel.Fatal, testTarget);

            var target = new LogManagerBase(config);

            // this should ignore errors in the broken target and flip down to the working target...
            var logger = (Logger)target.GetLogger("Foobar");
            await logger.TraceAsync("Hello, world.");

            // check...
            Assert.Equal(1, testTarget.NumWritten);
            Assert.Equal(LogLevel.Trace, testTarget.LastWritten.Level);
            Assert.Null(testTarget.LastWritten.Exception);
        }
    }
}
