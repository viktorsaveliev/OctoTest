using TMPro;
using UnityEngine;

namespace OctoTest.PopupSystem
{
    public class PopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _bodyText;

        [SerializeField] private PopupButton[] _buttons;
        
        public void Show(PopupData data)
        {
            if (data.popupButtonDatas.Length > _buttons.Length)
            {
                Debug.LogError($"Popup supports maximum {_buttons.Length} buttons");
                return;
            }

            _titleText.text = data.Title;
            _bodyText.text = data.Body;

            HideAllButtons();

            for (int i = 0; i < data.popupButtonDatas.Length; i++)
            {
                _buttons[i].Show(data.popupButtonDatas[i]);
            }

            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HideAllButtons()
        {
            foreach (PopupButton button in _buttons)
            {
                button.Hide();
            }
        }
    }
}
