namespace Disruptor.ReadModel.Tests.Infrastructure.FaultHandling
{
    public static class RetryExtensions
    {
        public static Retries Retries  (this int input) => (Retries) input;
    }
}
