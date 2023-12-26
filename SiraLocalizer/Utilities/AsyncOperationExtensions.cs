using UnityEngine;

namespace SiraLocalizer.Utilities
{
    internal static class AsyncOperationExtensions
    {
        public static AsyncOperationAwaiter<T> GetAwaiter<T>(this T asyncOperation) where T : AsyncOperation => new(asyncOperation);
    }
}
