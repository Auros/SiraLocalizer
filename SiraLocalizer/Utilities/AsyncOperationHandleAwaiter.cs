using System;
using System.Runtime.CompilerServices;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SiraLocalizer.Utilities
{
#pragma warning disable IDE1006
    internal class AsyncOperationHandleAwaiter<T> : ICriticalNotifyCompletion
    {
        private AsyncOperationHandle<T> asyncOperation;
        private Action continuationAction;

        public AsyncOperationHandleAwaiter(AsyncOperationHandle<T> asyncOperation)
        {
            this.asyncOperation = asyncOperation;
            this.continuationAction = null;
        }

        public bool IsCompleted => asyncOperation.IsDone;

        public AsyncOperationHandle<T> GetResult()
        {
            if (continuationAction != null)
            {
                asyncOperation.Completed -= Continue;
                continuationAction = null;
            }

            AsyncOperationHandle<T> op = asyncOperation;
            asyncOperation = default;

            if (op.Status == AsyncOperationStatus.Failed)
            {
                throw op.OperationException ?? new Exception("Async operation failed");
            }

            return op;
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuationAction != null)
            {
                throw new InvalidOperationException("Continuation is already registered");
            }

            continuationAction = continuation;
            asyncOperation.Completed += Continue;
        }

        private void Continue(AsyncOperationHandle<T> _)
        {
            continuationAction?.Invoke();
        }
    }
}
