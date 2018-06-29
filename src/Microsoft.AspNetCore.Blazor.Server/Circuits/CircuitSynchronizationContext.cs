// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Server.Circuits
{
    [DebuggerDisplay("{_state,nq}")]
    internal class CircuitSynchronizationContext : SynchronizationContext
    {
        private static readonly ContextCallback ExecutionContextThunk = (object state) =>
        {
            var item = (WorkItem)state;
            item.Callback(item.State);
        };

        private static readonly WaitCallback BackgroundWorkThunk = (object state) =>
        {
            var context = (CircuitSynchronizationContext)state;
            context.ExecuteBackgroundWork();
        };

        private State _state;

        public event UnhandledExceptionEventHandler UnhandledException;

        public CircuitSynchronizationContext()
            : this(new State())
        {
        }

        private CircuitSynchronizationContext(State state)
        {
            _state = state;
        }

        public Task InvokeAsync(Action action)
        {
            var completion = new TaskCompletionSource<object>();
            Post(_ =>
            {
                try
                {
                    action();
                    completion.SetResult(null);
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }
            }, null);

            return completion.Task;
        }

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> function)
        {
            var completion = new TaskCompletionSource<TResult>();
            Post(_ =>
            {
                try
                {
                    var result = function();

                    completion.SetResult(result);
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }
            }, null);

            return completion.Task;
        }

        // asynchronously runs the callback
        public override void Post(SendOrPostCallback d, object state)
        {
            bool taken = false;
            Monitor.TryEnter(_state.Monitor, ref taken);
            if (taken)
            {
                // We can execute this synchronously because nothing is currently running.
                try
                {
                    _state.IsBusy = true;
                    ExecuteSynchronously(d, state);
                }
                finally
                {
                    _state.IsBusy = false;
                    Monitor.Exit(_state.Monitor);
                }

                return;
            }

            // If we get here is means that a callback is being queued while something is currently executing
            // in this context. Let's instead add it to the queue and yield.
            //
            // We use our own queue here to maintain the execution order of the callbacks scheduled here. Also
            // we need a queue rather than just scheduling an item in the thread pool - those items would immediately
            // block and hurt scalability.
            //
            // We need to capture the execution context so we can restore it later. This code is similar to
            // the call path of ThreadPool.QueueUserWorkItem and System.Threading.QueueUserWorkItemCallback.
            ExecutionContext context = null;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                context = ExecutionContext.Capture();
            }

            NotifyPendingWork(new WorkItem()
            {
                Context = context,
                Callback = d,
                State = state,
            });
        }

        // synchronously runs the callback
        public override void Send(SendOrPostCallback d, object state)
        {
            try
            {
                Monitor.Enter(_state.Monitor);
                _state.IsBusy = true;

                ExecuteSynchronously(d, state);
            }
            finally
            {
                _state.IsBusy = false;
                Monitor.Exit(_state.Monitor);
            }
        }

        // shallow copy
        public override SynchronizationContext CreateCopy()
        {
            return new CircuitSynchronizationContext(_state);
        }

        private void NotifyPendingWork(WorkItem item)
        {
            lock (_state.WorkerLock)
            {
                _state.Queue.Enqueue(item);

                if (!_state.IsWorkerActive)
                {
                    _state.IsWorkerActive = true;

                    // Perf - using a static thunk here to avoid a delegate allocation.
                    ThreadPool.UnsafeQueueUserWorkItem(BackgroundWorkThunk, this);
                }
            }
        }

        private void ExecuteSynchronously(SendOrPostCallback d, object state)
        {
            d(state);
        }

        private void ExecuteSynchronously(WorkItem item)
        {
            if (item.Context == null)
            {
                item.Callback(item.State);
                return;
            }

            // Perf - using a static thunk here to avoid a delegate allocation.
            ExecutionContext.Run(item.Context, ExecutionContextThunk, item);
        }

        private void ExecuteBackgroundWork()
        {
            Monitor.Enter(_state.Monitor);
            _state.IsBusy = true;

            var original = Current;
            try
            {
                SetSynchronizationContext(this);

                while (_state.Queue.TryDequeue(out var item))
                {
                    try
                    {
                        ExecuteSynchronously(item);
                    }
                    catch (Exception ex)
                    {
                        DispatchException(ex);
                    }
                }
            }
            finally
            {
                SetSynchronizationContext(original);

                _state.IsBusy = false;
                Monitor.Exit(_state.Monitor);
            }

            // Now that we've processed the queue we need to figure out if more work items were queued
            // while we were running. Since IsWorkerActive is protected by a lock, we know that it's true,
            // which means that no one else coul have started a new batch of background work.
            //
            // So our task is simply to check if there is still work and start a new background worker
            // job.
            lock (_state.WorkerLock)
            {
                if (_state.Queue.IsEmpty)
                {
                    _state.IsWorkerActive = false;
                    return;
                }

                // Perf - using a static thunk here to avoid a delegate allocation.
                ThreadPool.UnsafeQueueUserWorkItem(BackgroundWorkThunk, this);
            }
        }

        private void DispatchException(Exception ex)
        {
            var handler = UnhandledException;
            if (handler != null)
            {
                handler(this, new UnhandledExceptionEventArgs(ex, isTerminating: false));
            }
        }

        private class State
        {
            public bool IsBusy; // Just for debugging
            public object Monitor = new object();

            public bool IsWorkerActive;
            public ConcurrentQueue<WorkItem> Queue = new ConcurrentQueue<WorkItem>();
            public object WorkerLock = new object();

            public override string ToString()
            {
                return $"{{ Queue: {Queue.Count}, Busy: {IsBusy}, Worker Active: {IsWorkerActive} }}";
            }
        }

        private class WorkItem
        {
            public ExecutionContext Context;
            public SendOrPostCallback Callback;
            public object State;
        }
    }
}
