using UnityEngine;
using UnityEngine.UI;

namespace I0plus.XdUnityUI
{
    [ExecuteAlways]
    public class ToggleGraphicSwap : MonoBehaviour
    {
        private Toggle toggle;

        private Toggle Toggle => toggle ? toggle : (toggle = GetComponent<Toggle>());

        private void Awake()
        {
            Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnEnable()
        {
            Toggle.targetGraphic.enabled = !toggle.isOn;
        }

        private void OnValueChanged(bool on)
        {
            Toggle.targetGraphic.enabled = !on;
        }
    }
}