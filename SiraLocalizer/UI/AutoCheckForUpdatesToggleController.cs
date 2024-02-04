using BGLib.Polyglot;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class AutoCheckForUpdatesToggleController : MonoBehaviour
    {
        private Settings _config;
        private Toggle _toggle;

        public static AutoCheckForUpdatesToggleController Create(DiContainer container, Transform parent)
        {
            GameObject template = container.Resolve<SettingsNavigationController>().transform.Find("GraphicSettings/ViewPort/Content/Fullscreen").gameObject;

            GameObject root = container.InstantiatePrefab(template, parent);
            root.name = "AutoCheckForUpdatesToggle";

            Destroy(root.GetComponent<BoolSettingsController>());

            var rootTransform = (RectTransform)root.transform;
            rootTransform.SetParent(parent, false);

            var text = rootTransform.Find("NameText").GetComponent<LocalizedTextMeshProUGUI>();
            text.Key = "AUTOMATICALLY_DOWNLOAD_UPDATES_ON_STARTUP";

            var toggle = rootTransform.Find("SwitchView").GetComponent<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();

            AutoCheckForUpdatesToggleController controller = container.InstantiateComponent<AutoCheckForUpdatesToggleController>(root);
            controller._toggle = toggle;

            return controller;
        }

        [Inject]
        [UsedImplicitly]
        private void Construct(Settings config)
        {
            _config = config;
        }

        private void OnEnable()
        {
            _toggle.isOn = _config.automaticallyDownloadLocalizations;
            _toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool value)
        {
            _config.automaticallyDownloadLocalizations = value;
        }
    }
}
