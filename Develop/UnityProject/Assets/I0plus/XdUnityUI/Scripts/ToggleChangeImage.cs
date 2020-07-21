using UnityEngine;
using UnityEngine.UI;

namespace I0plus.XdUnityUI
{
    public class ToggleChangeImage : MonoBehaviour
    {
        private Toggle toggle;

        private void Awake()
        {
            toggle = gameObject.GetComponent<Toggle>();
            if (toggle == null) return;
            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (toggle == null) return;
            OnValueChanged(toggle.isOn);
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnDestroy()
        {
            if (toggle == null) return;
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool on)
        {
            if (toggle == null || toggle.image == null) return;
            toggle.image.enabled = !on;
        }
    }
}