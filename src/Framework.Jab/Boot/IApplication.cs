﻿namespace Framework.Jab.Boot;

public interface IApplication
{
    Task RunAsync(CancellationToken cancellationToken);
}