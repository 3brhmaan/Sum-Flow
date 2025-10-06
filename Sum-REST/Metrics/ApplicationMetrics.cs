using System.Diagnostics.Metrics;

namespace Sum_REST.Metrics;

public static class ApplicationMetrics
{
    public static readonly Meter Meter = new("Custom-Meter" , "1.0.0");

    public static readonly Histogram<double> QueueWaitingTime = Meter.CreateHistogram<double>(
        name: "queue_waiting_time_seconds" ,
        unit: "s" ,
        description: "Measures the time a message spent waiting in the RabbitMQ queue before being consumed."
    );

    public static readonly Histogram<double> RequestWholeTime = Meter.CreateHistogram<double>(
        name: "whole_request_time_soconds" ,
        unit: "s" ,
        description: "Measures the time a request spend from the start at gRPC until finishing it's work at REST."
    );
}