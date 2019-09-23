namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Info(string accountId);
    }

    public class NLogAdapter : ILogger
    {
        private readonly IFailedCounter _failedCounter;

        public NLogAdapter(IFailedCounter failedCounter)
        {
            _failedCounter = failedCounter;
        }

        public void Info(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}