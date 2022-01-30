﻿using Autofac;
using Workflow;

namespace Framework.Autofac.Boot.Autofac;

public class BootScope<T> where T : WorkflowBaseContext
{
    public IContainer? Container { get; set; }
    public IWorkflowBuilder<T>? WorkflowBuilder { get; set; }
    public ILifetimeScope? LifeTimeScope { get; set; }
}