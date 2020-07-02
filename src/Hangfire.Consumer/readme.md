# Hangfire.Consumer

## Description
This is a simple example of a generic process that can attach to a given Hangfire queue and process jobs from that queue.

It has implementations for the two interfaces that represent Hangfire job contracts.

## Behavior
On startup, will attach to the queue specified by the setting "HangfireQueue" and start to process jobs. Each time a job is processed the start and end times are recorded by the JobTimingReporter.

A seperate process checks every 5 seconds to see how long the Hangfire.Consumer has been idle for since the last job was completed, if not currently processing a job.

Once the idle time, specified by the setting IdleTimeoutSeconds has been met, the Hangfire server will gracefully exit and the container will shutdown.
