﻿using System.Diagnostics;
using Framework.Extensions.List;
using Framework.Validation;
using Serilog;

namespace Framework.Throttle;

public class TaskThrottler : ITaskThrottler
{
    public async Task<IEnumerable<T>> ThrottleAsync<T>(IEnumerable<Func<Task<T>>> tasks,
        ThrottleLimit throttleLimit, CancellationToken cancellationToken)
    {
        Log.Debug($"Throttle with limit {throttleLimit.Limit} and {throttleLimit.Weight} in period {throttleLimit.Period}");
        var maxGroupSize = throttleLimit.Limit / throttleLimit.Weight;
        Log.Debug($"Throttle with group size {maxGroupSize}");
        Validate.IsTrue(maxGroupSize >= 1, nameof(maxGroupSize));
        var groups = tasks.Group(maxGroupSize).ToList();

        var results = new List<T>();

        foreach (var group in groups)
        {
            var groupList = group.ToList();
            var timer = new Stopwatch();
            timer.Start();
            var groupResults = await groupList.Select(f => f()).WhenAllAsync().ConfigureAwait(false);
            timer.Stop();
            results.AddRange(groupResults);

            if (timer.Elapsed.Seconds < throttleLimit.Period)
            {
                await WaitAsync(throttleLimit, groupList.Count, maxGroupSize, timer).ConfigureAwait(false);
            }
        }

        return results;
    }

    public async Task ThrottleAsync(IEnumerable<Func<Task>> tasks, ThrottleLimit throttleLimit, CancellationToken cancellationToken)
    {
        var maxGroupSize = throttleLimit.Limit / throttleLimit.Weight;
        Validate.IsTrue(maxGroupSize >= 1, nameof(maxGroupSize));
        var groups = tasks.Group(maxGroupSize).ToList();

        foreach (var group in groups)
        {
            var groupList = group.ToList();
            var timer = new Stopwatch();
            timer.Start();
            await groupList.Select(f => f()).WhenAllAsync().ConfigureAwait(false);
            timer.Stop();

            if (timer.Elapsed.Seconds < throttleLimit.Period)
            {
                await WaitAsync(throttleLimit, groupList.Count, maxGroupSize, timer).ConfigureAwait(false);
            }
        }
    }

    private Task WaitAsync(ThrottleLimit throttleLimit, int groupSize, int maxGroupSize, Stopwatch timer)
    {
        if (groupSize == maxGroupSize)
        {
            var timeToWait = throttleLimit.Period - timer.Elapsed.Seconds;
            Log.Debug($"Wait for {timeToWait}s");
            return Task.Delay(GetTimeToWait(timeToWait));
        }
        else
        {
            var timeToWait = throttleLimit.Period / (maxGroupSize / groupSize) - timer.Elapsed.Seconds;
            Log.Debug($"Wait for {timeToWait}s");
            return Task.Delay(GetTimeToWait(timeToWait));
        }
    }

    private static int GetTimeToWait(int timeToWait)
    {
        return timeToWait < 0 ? 0 : timeToWait * 1000;
    }
}