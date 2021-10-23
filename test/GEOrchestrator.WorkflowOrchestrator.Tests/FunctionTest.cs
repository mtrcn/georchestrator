using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GEOrchestrator.WorkflowOrchestrator.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task TestSQSEventLambdaFunction()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.QueueHandler(sqsEvent, context);

            Assert.Contains("Processed message foobar", logger.Buffer.ToString());
        }
    }
}
