using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SiraLocalizer.Utilities
{
    internal static class GetAwaiterExtensions
    {
        public static AsyncOperationAwaiter<T> GetAwaiter<T>(this T asyncOperation) where T : AsyncOperation => new(asyncOperation);

        public static AsyncOperationHandleAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> asyncOperation) => new(asyncOperation);
    }
}
