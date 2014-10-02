using System;
using System.Diagnostics;
using System.Linq;

namespace Metrics.Utils
{
   
    public class AverageSampler
    {
    
        private VolatileDouble _intstantRate = new VolatileDouble(0.0);

        private AtomicLong uncounted = new AtomicLong();

        private double interval;

        private int samples;
        
        private CircularBuffer<double> _sampleBuffer;

        public AverageSampler(long interval, TimeUnit intervalUnit, long samplePeriod, TimeUnit sampleUnits)
        {
            this.interval = intervalUnit.ToNanoseconds(interval);

            this.samples = (int)Math.Ceiling(sampleUnits.ToNanoseconds(samplePeriod) / this.interval);

            //Debug.WriteLine(String.Format("Startup - Samples: {0} Interval: {1}", samples, interval));

            _sampleBuffer = new CircularBuffer<double>(samples);
        }

        public void Update(long value)
        {
            uncounted.Add(value);
        }

        public void Tick()
        {
            long count = uncounted.GetAndReset();

            double instantRate = count / interval;

            //Debug.WriteLine(String.Format("Count: {0} Interval: {1}", count, interval));

            _intstantRate.Set(instantRate);
                      
            _sampleBuffer.Add(instantRate);
                       
        }

        public double GetRate(TimeUnit rateUnit)
        {
            //Debug.WriteLine(String.Format("GetRate Count: {0}", _sampleBuffer.Count));
            return _sampleBuffer.Where(o => o > 0.0).DefaultIfEmpty().Average() * (double)rateUnit.ToNanoseconds(1L);
            //if (_sampleBuffer.Count > 1) {
               
            //} else {
            //    return 0;
            //}
        }

        public double GetInstantRate(TimeUnit rateUnit)
        {
            return _intstantRate.Get() * (double)rateUnit.ToNanoseconds(1L);
        }

        public void Reset()
        {
            uncounted.SetValue(0L);
            _sampleBuffer = new CircularBuffer<double>(samples);
            _intstantRate.Set(0.0);
        }
    }
}
