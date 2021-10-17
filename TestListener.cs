using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MartenProjectionListenerRepro
{
    public class TestListener : DocumentSessionListenerBase
    {
        readonly ILogger<TestListener> _logger;

        public TestListener(ILogger<TestListener> logger)
        {
            _logger = logger;
        }

        public override Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
        {
            _logger.LogInformation("Deleted: {Deleted}", JsonConvert.SerializeObject(commit.Deleted));
            _logger.LogInformation("Inserted: {Inserted}", JsonConvert.SerializeObject(commit.Inserted));
            _logger.LogInformation("Updated: {Updated}", JsonConvert.SerializeObject(commit.Updated));
            _logger.LogInformation("EventAdded: {Events}", JsonConvert.SerializeObject(commit.GetEvents()));
            return Task.CompletedTask;
        }
    }
}