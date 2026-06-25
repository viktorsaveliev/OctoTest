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
    }
}
