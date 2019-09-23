namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        private FailedCounter _failedCounter;

        public NLogAdapter(FailedCounter failedCounter)
        {
            _failedCounter = failedCounter;
        }

        public void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}