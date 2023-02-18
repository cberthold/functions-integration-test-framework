using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Integration.Test.Framework.Builders
{
    public class TaskBuilder
    {
        private TaskBuilder() { }

        public Func<Task<bool>> Condition { get; private set; }
        public Func<Func<Task>, Task> BuildWhen { get; private set; }

        public Func<Task> When { get; private set; }

        public static TaskBuilder While(Func<Task<bool>> condition)
        {
            var builder = new TaskBuilder();
            builder.Condition = condition;
            builder.BuildWhen = async (t) =>
            {
                while (!await condition())
                {
                    await t();
                }
            };
            return builder;
        }

        public TaskBuilder Poll(int intervalMillis = 2 * 1000)
        {
            When = async () =>
            {
                await Task.Delay(intervalMillis);

            };

            return this;
        }

        public TaskBuilder UntilTimeout(int timeoutMillis = 60 * 1000)
        {
            bool timeStarted = false;
            DateTime startTime = DateTime.Now;

            // TODO: this could be chained before or after
            // start of time should really be before since time can elapse
            // this is good enough for right now
            When += () =>
            {
                // reset the time for the first loop
                if (timeStarted == false)
                {
                    startTime = DateTime.Now;
                    timeStarted = true;
                }

                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMillis)
                {
                    throw new ApplicationException("Condition not met before timeout");
                }
                return Task.CompletedTask;
            };

            return this;
        }

        public Task Go()
        {
            return BuildWhen(When);
        }
    }
}
