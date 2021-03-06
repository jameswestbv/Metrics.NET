###0.2.3 / 2014-10-01
* add support for set counter & set meters [details](https://github.com/etishor/Metrics.NET/issues/21)
* cleanup owin adapter
* better & more resilient error handling

###0.2.2 / 2014-09-27
* add support for tagging metrics (not yet used in reports or visualization)
* add support for suppling a string user value to histograms & timers for tracking min / max / last values
* tests cleanup, some refactoring

###0.2.1 / 2014-09-25
* port latest changes from original metrics lib
* port optimization from ExponentiallyDecayingReservoir (https://github.com/etishor/Metrics.NET/commit/1caa9d01c16ff63504612d64771d52e9d7d9de5e)
* other minor optimizations
* add gauges for thread pool stats

###0.2.0 / 2014-09-20
* implement metrics contexts (and child contexts)
* make config more friendly
* most used condig options are now set by default
* add logging based on liblog (no fixed dependency - automaticaly wire into existing logging framework)
* update nancy & owin adapters to use contexts
* add some app.config settings to ease configuration

###0.1.11 / 2014-08-18
* update to latest visualization app (fixes checkboxes being outside dropdown)
* fix json caching in IE
* allow defining custom names for metric registry

###0.1.10 / 2014-07-30
* fix json formating (thanks to Evgeniy Kucheruk @kpoxa)

###0.1.9 / 2014-07-04
* make reporting more extensible

###0.1.8
* remove support for .NET 4.0

###0.1.6
* for histograms also store last value
* refactor configuration ( use Metric.Config.With...() )
* add option to completely disable metrics Metric.Config.CompletelyDisableMetrics() (useful for measuring metrics impact)
* simplify health checks
