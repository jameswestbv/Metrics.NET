
using System;
using System.Collections.Concurrent;
using System.Linq;
using Metrics.Utils;
namespace Metrics.Core
{
    public interface SimpleMeterImplementation : SimpleMeter, MetricValueProvider<MeterValue> { }

    public sealed class SimpleMeterMetric : SimpleMeterImplementation, IDisposable
    {

        public static readonly int InstantRateSampleMilliSeconds = 250;
        public static readonly int TickIntervalSeconds = 2;

        public static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(TickIntervalSeconds);

        public static readonly TimeSpan InstantRateTickInterval = TimeSpan.FromMilliseconds(InstantRateSampleMilliSeconds);

        private class MeterWrapper
        {
         
            private double FifteenMinuteRate { get { return this.m15Rate.GetRate(TimeUnit.Seconds);; } }
            private double FiveMinuteRate { get { return this.m5Rate.GetRate(TimeUnit.Seconds); } }
            private double OneMinuteRate { get { return this.m1Rate.GetRate(TimeUnit.Seconds); } }
            private double InstantRate { get { return this.instantRate.GetInstantRate(TimeUnit.Seconds); } }

            public readonly AverageSampler instantRate = new AverageSampler(InstantRateSampleMilliSeconds, TimeUnit.Milliseconds, 250, TimeUnit.Milliseconds);
            public readonly AverageSampler m1Rate = new AverageSampler(TickIntervalSeconds, TimeUnit.Seconds, 1, TimeUnit.Minutes);
            public readonly AverageSampler m5Rate = new AverageSampler(TickIntervalSeconds, TimeUnit.Seconds, 5, TimeUnit.Minutes);
            public readonly AverageSampler m15Rate = new AverageSampler(TickIntervalSeconds, TimeUnit.Seconds, 15, TimeUnit.Minutes);

            public AtomicLong count = new AtomicLong();

            public void Tick()
            {
                this.m1Rate.Tick();
                this.m5Rate.Tick();
                this.m15Rate.Tick();
            }

            public void InstantRateTick()
            {
                this.instantRate.Tick();
            }

            public void Mark(long count)
            {
                this.count.Add(count);
                this.instantRate.Update(count);
                this.m1Rate.Update(count);
                this.m5Rate.Update(count);
                this.m15Rate.Update(count);
            }

            public void Reset()
            {
                this.count.SetValue(0);
                this.instantRate.Reset();
                this.m1Rate.Reset();
                this.m5Rate.Reset();
                this.m15Rate.Reset();
            }

            public MeterValue GetValue(double elapsed)
            {
                return new MeterValue(this.count.Value, this.GetMeanRate(elapsed), this.InstantRate, this.OneMinuteRate, this.FiveMinuteRate, this.FifteenMinuteRate);
            }

            private double GetMeanRate(double elapsed)
            {
                if (this.count.Value == 0)
                {
                    return 0.0;
                }

                return this.count.Value / elapsed * TimeUnit.Seconds.ToNanoseconds(1);
            }

           
        }


        private readonly ConcurrentDictionary<string, MeterWrapper> setMeters = new ConcurrentDictionary<string, MeterWrapper>();

        private readonly MeterWrapper wrapper = new MeterWrapper();

        private readonly Clock clock;
        private readonly Scheduler tickScheduler;
        private readonly Scheduler instantRateTickScheduler;

        private long startTime;

        public SimpleMeterMetric()
            : this(Clock.Default, new ActionScheduler(), new ActionScheduler())
        { }

        public SimpleMeterMetric(Clock clock, Scheduler scheduler, Scheduler instantRateScheduler)
        {
            this.clock = clock;
            this.startTime = this.clock.Nanoseconds;
            this.tickScheduler = scheduler;
            this.tickScheduler.Start(TickInterval, () => Tick());

            this.instantRateTickScheduler = instantRateScheduler;
            this.instantRateTickScheduler.Start(InstantRateTickInterval, () => InstantRateTick());
        }

        public void Mark()
        {
            Mark(1L);
        }

        public void Mark(long count)
        {
            this.wrapper.Mark(count);
        }

        public void Mark(string item)
        {
            this.Mark(item, 1L);
        }

        public void Mark(string item, long count)
        {
            this.Mark(count);
            this.setMeters.GetOrAdd(item, v => new MeterWrapper()).Mark(count);
        }

        public MeterValue Value
        {
            get
            {
                double elapsed = (clock.Nanoseconds - startTime);
                var value = this.wrapper.GetValue(elapsed);

                var items = this.setMeters
                    .Select(m => new { Item = m.Key, Value = m.Value.GetValue(elapsed) })
                    .Select(m => new MeterValue.SetItem(m.Item, value.Count > 0 ? m.Value.Count / (double)value.Count * 100 : 0.0, m.Value))
                    .OrderBy(m => m.Item)
                    .ToArray();

                return new MeterValue(value.Count, value.MeanRate, value.InstantRate, value.OneMinuteRate, value.FiveMinuteRate, value.FifteenMinuteRate, items);
            }
        }

        private void Tick()
        {
            this.wrapper.Tick();
            foreach (var value in setMeters.Values)
            {
                value.Tick();
            }
        }

        private void InstantRateTick()
        {
            this.wrapper.InstantRateTick();
            foreach (var value in setMeters.Values)
            {
                value.InstantRateTick();
            }
        }

        public void Dispose()
        {
            this.tickScheduler.Stop();
            using (this.tickScheduler) { }
            this.setMeters.Clear();
        }

        public void Reset()
        {
            this.startTime = this.clock.Nanoseconds;
            this.wrapper.Reset();
            foreach (var meter in this.setMeters.Values)
            {
                meter.Reset();
            }
        }
    }
}
