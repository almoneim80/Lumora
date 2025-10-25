﻿using Medallion.Threading.Postgres;
using Serilog;

namespace Lumora.Infrastructure;

public class LockManager
{
    public static (PostgresDistributedLockHandle?, bool) GetNoWaitLock(string lockKey, string connectionString)
    {
        Log.Information("GetNoWaitLock: " + lockKey);

        try
        {
            var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), connectionString);

            // pg_try_advisory_lock - Get the lock or skip if not available.
            var res = secondaryLock.TryAcquire();
            return (res, true);
        }
        catch (Exception ex)
        {
            LogError(ex, lockKey);
            return (null, false);
        }
    }

    public static PostgresDistributedLockHandle? GetWaitLock(string lockKey, string connectionString)
    {
        Log.Information("GetWaitLock: " + lockKey);

        try
        {
            var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), connectionString);

            // pg_advisory_lock - Get or Wait for lock.
            return secondaryLock.Acquire();
        }
        catch (Exception ex)
        {
            LogError(ex, lockKey);
            return null;
        }
    }

    private static void LogError(Exception ex, string lockKey)
    {
        Log.Error(ex, "Error when acquiring lock:" + lockKey);
    }
}
