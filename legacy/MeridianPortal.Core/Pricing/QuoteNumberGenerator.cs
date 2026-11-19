using System;
using System.Threading;

namespace MeridianPortal.Core.Pricing
{
    public static class QuoteNumberGenerator
    {
        private static int _sequence;

        // Q-20141103-00042. Sequence resets when the app pool recycles.
        public static string Next()
        {
            int seq = Interlocked.Increment(ref _sequence);
            return "Q-" + DateTime.Now.ToString("yyyyMMdd") + "-" + seq.ToString("D5");
        }
    }
}
