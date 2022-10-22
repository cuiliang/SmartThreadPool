using System;
using System.Threading;
using System.Threading.Tasks;

namespace Amib.Threading.Internal
{
    public abstract class WorkItemsGroupBase : IWorkItemsGroup
    {
        #region Private Fields

        /// <summary>
        /// Contains the name of this instance of SmartThreadPool.
        /// Can be changed by the user.
        /// </summary>
        private string _name = "WorkItemsGroupBase";

        protected WorkItemsGroupBase()
        {
            IsIdle = true;
        }

        #endregion

        #region IWorkItemsGroup Members

        #region Public Methods

        /// <summary>
        /// Get/Set the name of the SmartThreadPool/WorkItemsGroup instance
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion

        #region Abstract Methods

        public abstract int Concurrency { get; set; }
        public abstract int WaitingCallbacks { get; }
        public abstract int InUseThreads { get; }

        public abstract object[] GetStates();
        public abstract WIGStartInfo WIGStartInfo { get; }
        public abstract void Start();
        public abstract void Cancel(bool abortExecution);
        public abstract bool WaitForIdle(int millisecondsTimeout);
        public abstract event WorkItemsGroupIdleHandler OnIdle;

        internal abstract void Enqueue(WorkItem workItem);
        internal abstract void Requeue(WorkItem workItem);
        internal virtual void PreQueueWorkItem() { }

#if _ASYNC_SUPPORTED
        public abstract Task WaitForIdleAsync(CancellationToken? cancellationToken = null);

        public Task WaitForIdleAsync(TimeSpan timeout) =>
            WaitForIdleAsync(new CancellationTokenSource(timeout).Token);

        public Task WaitForIdleAsync(int millisecondsTimeout) =>
            WaitForIdleAsync(new CancellationTokenSource(millisecondsTimeout).Token);
#endif

        #endregion

        #region Common Base Methods

        /// <summary>
        /// Cancel all the work items.
        /// Same as Cancel(false)
        /// </summary>
        public virtual void Cancel()
        {
            Cancel(false);
        }

        /// <summary>
        /// Wait for the SmartThreadPool/WorkItemsGroup to be idle
        /// </summary>
        public void WaitForIdle()
        {
            WaitForIdle(Timeout.Infinite);
        }

        /// <summary>
        /// Wait for the SmartThreadPool/WorkItemsGroup to be idle
        /// </summary>
        public bool WaitForIdle(TimeSpan timeout)
        {
            return WaitForIdle((int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// IsIdle is true when there are no work items running or queued.
        /// </summary>
        public bool IsIdle { get; protected set; }

        #endregion

        #region QueueWorkItem

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="workItemPriority">The priority of the work item</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, WorkItemPriority workItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, workItemPriority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="workItemInfo">Work item info</param>
        /// <param name="callback">A callback to execute</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, workItemInfo, callback);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, state);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, WorkItemPriority workItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, state, workItemPriority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="workItemInfo">Work item information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback, object state)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, workItemInfo, callback, state);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(
            WorkItemCallback callback,
            object state,
            PostExecuteWorkItemCallback postExecuteWorkItemCallback)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, state, postExecuteWorkItemCallback);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(
            WorkItemCallback callback,
            object state,
            PostExecuteWorkItemCallback postExecuteWorkItemCallback,
            WorkItemPriority workItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, state, postExecuteWorkItemCallback, workItemPriority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(
            WorkItemCallback callback,
            object state,
            PostExecuteWorkItemCallback postExecuteWorkItemCallback,
            CallToPostExecute callToPostExecute)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, state, postExecuteWorkItemCallback, callToPostExecute);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(
            WorkItemCallback callback,
            object state,
            PostExecuteWorkItemCallback postExecuteWorkItemCallback,
            CallToPostExecute callToPostExecute,
            WorkItemPriority workItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, state, postExecuteWorkItemCallback, callToPostExecute, workItemPriority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        #endregion

        #region QueueWorkItem(Action<...>)

        public IWorkItemResult QueueWorkItem(Action action, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                delegate
                {
                    action.Invoke();
                    return null;
                }, 
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T>(Action<T> action, T arg, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state =>
                {
                    action.Invoke(arg);
                    return null;
                },
                WIGStartInfo.FillStateWithArgs ? new object[] { arg } : null, 
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state =>
                {
                    action.Invoke(arg1, arg2);
                    return null;
                },
                WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2 } : null, priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state =>
                {
                    action.Invoke(arg1, arg2, arg3);
                    return null;
                },
                WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2, arg3 } : null, 
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                           this,
                           WIGStartInfo,
                           state =>
                           {
                               action.Invoke(arg1, arg2, arg3, arg4);
                               return null;
                           },
                           WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2, arg3, arg4 } : null, 
                           priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        #endregion

#if _ASYNC_SUPPORTED
        #region RunTask(Func<Task<T>)  ==> async Task<T> DoWork(..)

        public Task RunTask(
            Action action, 
            CancellationToken? cancellationToken = null,
            WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            if (cancellationToken?.IsCancellationRequested ?? false)
                return Task.FromCanceled(cancellationToken.Value);

            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                _ =>
                {
                     action();
                     return null;
                },
                priority);
            Enqueue(workItem);

            var wir = workItem.GetWorkItemResult();
            cancellationToken?.Register(() => wir.Cancel());

            return wir.GetResultAsync();
        }

        public Task<TResult> RunTask<TResult>(
            Func<TResult> func, 
            CancellationToken? cancellationToken = null,
            WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            if (cancellationToken?.IsCancellationRequested ?? false)
                return Task.FromCanceled<TResult>(cancellationToken.Value);

            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                _ => func(),
                priority);
            Enqueue(workItem);

            var wir = workItem.GetWorkItemResult().GetWorkItemResultT<TResult>();
            cancellationToken?.Register(() => wir.Cancel());

            return wir.GetResultAsync();
        }

        public Task RunTask(
            Func<Task> func, 
            CancellationToken? cancellationToken = null,
            WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            if (cancellationToken?.IsCancellationRequested ?? false)
                return Task.FromCanceled(cancellationToken.Value);

            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                _ => func(),
                priority);
            Enqueue(workItem);

            var wir = workItem.GetWorkItemResult();
            cancellationToken?.Register(() => wir.Cancel());

            return wir.GetResultAsync();
        }

        public Task<TResult> RunTask<TResult>(
            Func<Task<TResult>> func,
            CancellationToken? cancellationToken = null,
            WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            if (cancellationToken?.IsCancellationRequested ?? false)
                return Task.FromCanceled<TResult>(cancellationToken.Value);

            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                _ => func(),
                priority);
            Enqueue(workItem);

            var wir = workItem.GetWorkItemResult().GetWorkItemResultT<TResult>();
            cancellationToken?.Register(() => wir.Cancel());

            return wir.GetResultAsync();
        }

        #endregion
#endif
        #region QueueWorkItem(Func<Task, ...>)  ==> async Task DoWork(..)

        public IWorkItemResult QueueWorkItem(Func<Task> func, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(),
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T>(Func<T, Task> func, T arg, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg),
                WIGStartInfo.FillStateWithArgs ? new object[] { arg } : null,
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T1, T2>(Func<T1, T2, Task> func, T1 arg1, T2 arg2, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg1, arg2),
                WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2 } : null,
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T1, T2, T3>(Func<T1, T2, T3, Task> func, T1 arg1, T2 arg2, T3 arg3, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg1, arg2, arg3),
                WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2, arg3 } : null,
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        public IWorkItemResult QueueWorkItem<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg1, arg2, arg3, arg4),
                WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2, arg3, arg4 } : null,
                priority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        #endregion

        #region QueueWorkItem(Func<Task<T>, ...>)  ==> async Task<T> DoWork(..)

        public IWorkItemResult<TResult> QueueWorkItem<TResult>(Func<Task<TResult>> func, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(),
                priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T, TResult>(Func<T, Task<TResult>> func, T arg, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg),
                priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func, T1 arg1, T2 arg2, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg1, arg2),
                priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg1, arg2, arg3),
                priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state => func(arg1, arg2, arg3, arg4),
                priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        #endregion

        #region QueueWorkItem(Func<...>)

        public IWorkItemResult<TResult> QueueWorkItem<TResult>(Func<TResult> func, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                            this,
                            WIGStartInfo,
                            state =>
                            {
                                return func.Invoke();
                            }, priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T, TResult>(Func<T, TResult> func, T arg, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                this,
                WIGStartInfo,
                state =>
                {
                    return func.Invoke(arg);
                },
                WIGStartInfo.FillStateWithArgs ? new object[] { arg } : null,
                priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                            this,
                            WIGStartInfo,
                            state =>
                            {
                                return func(arg1, arg2);
                            },
                           WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2 } : null,
                           priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T1, T2, T3, TResult>(
            Func<T1, T2, T3, TResult> func, T1 arg1, T2 arg2, T3 arg3, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                            this,
                            WIGStartInfo,
                            state =>
                            {
                                return func.Invoke(arg1, arg2, arg3);
                            },
                           WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2, arg3 } : null,
                           priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        public IWorkItemResult<TResult> QueueWorkItem<T1, T2, T3, T4, TResult>(
            Func<T1, T2, T3, T4, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, WorkItemPriority priority = SmartThreadPool.DefaultWorkItemPriority)
        {
            PreQueueWorkItem();
            WorkItem workItem = WorkItemFactory.CreateWorkItem(
                            this,
                            WIGStartInfo,
                            state =>
                            {
                                return func.Invoke(arg1, arg2, arg3, arg4);
                            },
                           WIGStartInfo.FillStateWithArgs ? new object[] { arg1, arg2, arg3, arg4 } : null,
                           priority);
            Enqueue(workItem);
            return new WorkItemResultTWrapper<TResult>(workItem.GetWorkItemResult());
        }

        #endregion

        #endregion
    }
}