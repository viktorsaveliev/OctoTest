using System;
using UnityEngine;
using UnityEngine.Events;

namespace OctoTest.PopupSystem
{
    [Serializable]
    public struct PopupButtonData
    {
        public string Label;
        public Color Color;
        public UnityEvent OnClickAction;

        /*public PopupButtonData(string label, Action onClick, Color color)
        {
            Label = label;
            OnClickAction = onClick;
            Color = color;
        }

        public PopupButtonData(string label, Action onClick)
        {
            Label = label;
            OnClickAction = onClick;
            Color = Color.white;
        }*/
    }
}
