using System.Collections;
using HMUI;
using UnityEngine;

namespace SiraLocalizer.UI
{
    internal class ScrollViewContentUpdater : MonoBehaviour
    {
        [SerializeField]
        internal ScrollView scrollView;

        private Coroutine _updateLayoutCoroutine;

        protected void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled && _updateLayoutCoroutine == null)
            {
                _updateLayoutCoroutine = StartCoroutine(UpdateLayoutCoroutine());
            }
        }

        private IEnumerator UpdateLayoutCoroutine()
        {
            yield return null;
            scrollView.UpdateContentSize();
            scrollView.RefreshButtons();
            _updateLayoutCoroutine = null;
        }
    }
}
