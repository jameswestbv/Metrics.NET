﻿
using Metrics.PerfCounters;
namespace Metrics
{
    public static class PerformanceCountersConfigExtensions
    {
        /// <summary>
        /// Register all pre-defined performance counters as Gauge metrics.
        /// This includes System Counters, CLR Global Counters and CLR App Counters.
        /// </summary>
        public static MetricsConfig WithAllCounters(this MetricsConfig config, string systemContext = "Machine", string applicationContext = "Application")
        {
            return config.WithSystemCounters(systemContext)
                .WithCLRGlobalCounters(systemContext)
                .WithCLRAppCounters(applicationContext);
        }

        /// <summary>
        /// Register all pre-defined system performance counters as Gauge metrics.
        /// This includes: Available RAM, CPU Usage, Disk Writes/sec, Disk Reads/sec
        /// </summary>
        public static MetricsConfig WithSystemCounters(this MetricsConfig config, string context)
        {
            config.WithConfigExtension((ctx, hs) => PerformanceCounters.RegisterSystemCounters(ctx.Context(context)));
            return config;
        }

        /// <summary>
        /// Register global, CLR related performance counters as Gauge metrics.
        /// This includes: total .NET Mb in all heaps and total .NET time spent in GC
        /// </summary>
        public static MetricsConfig WithCLRGlobalCounters(this MetricsConfig config, string context)
        {
            config.WithConfigExtension((ctx, hs) => PerformanceCounters.RegisterCLRGlobalCounters(ctx.Context(context)));
            return config;
        }

        /// <summary>
        /// Register application level, CLR related performance counters as Gauge metrics.
        /// This includes: Mb in all heaps, time in GC, exceptions per sec, Threads etc.
        /// </summary>
        public static MetricsConfig WithCLRAppCounters(this MetricsConfig config, string context)
        {
            config.WithConfigExtension((ctx, hs) => PerformanceCounters.RegisterCLRAppCounters(ctx.Context(context)));
            return config;
        }
    }
}
