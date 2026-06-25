using System;
using UnityEngine;

namespace OctoTest.PopupSystem
{
    [Serializable]
    public class PopupData
    {
        public string Title;    
        public string Body;
        public PopupButtonData[] popupButtonDatas;
    }

    public class _TestPopupShower : MonoBehaviour
    {
        [SerializeField] private PopupView _popup;
        [SerializeField] private PopupData _popupData;

        private void Start()
        {
            ShowPopup();
        }

        [ContextMenu("Show Popup")]
        private void ShowPopup()
        {
            _popup.Show(_popupData);
        }

        public void Hello()
        {
            Debug.Log("HELLO");
        }

        public void Hi()
        {
            Debug.Log("Hi");
        }

        public void Bye()
        {
            _popup.Hide();
            Debug.Log("Bye");
        }

        public void WhereIsGirls()
        {
            Debug.Log("WhereIsGirls?");
        }
    }
}
