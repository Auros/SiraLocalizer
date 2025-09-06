using UnityEngine.ResourceManagement.AsyncOperations;

namespace SiraLocalizer.Utilities
{
    internal static class AsyncOperationHandleExtensions
    {
        public static AsyncOperationHandleAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> asyncOperation) => new(asyncOperation);
    }
}
