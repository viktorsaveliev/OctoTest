using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoTest.PopupSystem
{
    [RequireComponent(typeof(Button))]
    public class PopupButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private Button _button;

        private UnityEvent _onClick;

        private void OnValidate()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        public void Show(PopupButtonData buttonData)
        {
            _labelText.text = buttonData.Label;
            _labelText.color = buttonData.Color;
            _onClick = buttonData.OnClickAction;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _onClick = null;
            gameObject.SetActive(false);
        }

        private void OnClick()
        {
            _onClick?.Invoke();
        }
    }
}
