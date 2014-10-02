﻿using System;
using Metrics.Core;
using Metrics.PerfCounters;
using Metrics.Sampling;
using Metrics.Utils;

namespace Metrics.Tests
{
    public class TestMetricsBuilder : MetricsBuilder
    {
        private readonly Clock clock;
        private readonly Scheduler scheduler;

        public TestMetricsBuilder(Clock clock, Scheduler scheduler)
        {
            this.clock = clock;
            this.scheduler = scheduler;
        }

        public MetricValueProvider<double> BuildePerformanceCounter(string name, Unit unit, string counterCategory, string counterName, string counterInstance)
        {
            return new PerformanceCounterGauge(counterCategory, counterName, counterInstance);
        }

        public MetricValueProvider<double> BuildGauge(string name, Unit unit, Func<double> valueProvider)
        {
            return new FunctionGauge(valueProvider);
        }

        public CounterImplementation BuildCounter(string name, Unit unit)
        {
            return new CounterMetric();
        }

        public MeterImplementation BuildMeter(string name, Unit unit, TimeUnit rateUnit)
        {
            return new MeterMetric(this.clock, this.scheduler, this.scheduler);
        }

        public SimpleMeterImplementation BuildSimpleMeter(string name, Unit unit, TimeUnit rateUnit)
        {
            return new SimpleMeterMetric(this.clock, this.scheduler, this.scheduler);
        }

        public HistogramImplementation BuildHistogram(string name, Unit unit, SamplingType samplingType)
        {
            if (samplingType == SamplingType.FavourRecent)
            {
                return new HistogramMetric(new ExponentiallyDecayingReservoir(this.clock, this.scheduler));
            }
            return new HistogramMetric(samplingType);
        }

        public HistogramImplementation BuildHistogram(string name, Unit unit, Reservoir reservoir)
        {
            return new HistogramMetric(new ExponentiallyDecayingReservoir(this.clock, this.scheduler));
        }

        public TimerImplementation BuildTimer(string name, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, SamplingType samplingType)
        {
            return new TimerMetric(new HistogramMetric(new ExponentiallyDecayingReservoir(this.clock, this.scheduler)), new MeterMetric(this.clock, this.scheduler, this.scheduler), this.clock);
        }

        public TimerImplementation BuildTimer(string name, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, Histogram histogram)
        {
            return new TimerMetric(new HistogramMetric(new ExponentiallyDecayingReservoir(this.clock, this.scheduler)), new MeterMetric(this.clock, this.scheduler, this.scheduler), this.clock);
        }

        public TimerImplementation BuildTimer(string name, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, Reservoir reservoir)
        {
            return new TimerMetric(new HistogramMetric(new ExponentiallyDecayingReservoir(this.clock, this.scheduler)), new MeterMetric(this.clock, this.scheduler, this.scheduler), this.clock);
        }
              
    }
}
