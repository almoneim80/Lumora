﻿using Medallion.Threading.Postgres;
using Lumora.Configuration;
using Lumora.Interfaces;

namespace Lumora.Services;

public class LockService : ILockService
{
    private readonly string connectionSting;

    public LockService(IConfiguration configuration)
    {
        var postgresConfig = configuration.GetSection("Postgres").Get<PostgresConfig>();

        if (postgresConfig == null)
        {
            throw new MissingConfigurationException("Postgres configuration is mandatory.");
        }

        connectionSting = postgresConfig.ConnectionString;
    }

    public ILockHolder Lock(string key)
    {
        throw new NotImplementedException();
    }

    public ILockHolder? TryLock(string key)
    {
        var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(key, true), connectionSting);

        var postgresDistributedLock = secondaryLock.TryAcquire();

        if (postgresDistributedLock is null)
        {
            return null;
        }
        else
        {
            return new PostgresLockHolder();
        }
    }
}

public class PostgresLockHolder : ILockHolder
{
    public PostgresLockHolder()
    {
    }
}
