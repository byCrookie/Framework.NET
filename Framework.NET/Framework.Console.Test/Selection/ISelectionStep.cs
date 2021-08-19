﻿using Framework.Workflow;

namespace Framework.Console.Test.Selection
{
    internal interface ISelectionStep<in TContext, in TOptions> : 
        IWorkflowOptionsStep<TContext, TOptions>
        where TContext : WorkflowBaseContext
    {
    }
}