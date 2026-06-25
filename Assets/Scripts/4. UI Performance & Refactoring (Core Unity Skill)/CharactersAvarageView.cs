using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace OctoTest.Refactoring
{
    // CHANGES:
    // 1. Incorrect attribute name SerializedField -> SerializeField
    // 2. GetComponents -> GetComponent
    // 3. .Length -> .Count for List
    // 4. (_characters.Count / totalValue) -> (totalValue / _characters.Count)
    // 5. Cache Text
    // 6. Add tick interval
    // 7. Delete Debug.Log from FixedUpdate
    // 8. Using StringBuilder for avoid GC allocation
    // 9. Using Coroutine instead of FixedUpdate for better perfomance (also can use UniTask)
    // 10. Use Character links on List, not a Transform

    public class CharactersAvarageView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private List<Character> _characters;

        [SerializeField, Range(0.2f, 5f)] private float _tickInterval = 0.5f;

        private bool _isActive;
        private Coroutine _coroutine;

        private readonly StringBuilder _sb = new();

        private void OnEnable()
        {
            _coroutine = StartCoroutine(UpdateCoroutine());
            _isActive = true;
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Stop()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
                _isActive = false;
            }
        }

        private IEnumerator UpdateCoroutine()
        {
            WaitForSeconds waitForSeconds = new(_tickInterval);

            while (_isActive)
            {
                yield return waitForSeconds;
                RefreshDisplay();
            }
        }

        private void RefreshDisplay()
        {
            float totalValue = 0f;

            foreach (Character character in _characters)
            {
                if (character == null) continue;
                totalValue += character.Value;
            }

            int count = _characters.Count;
            float avg = count > 0 ? totalValue / count : 0f;

            _sb.Clear();
            _sb.Append("Characters: ").Append(count)
               .Append("  Avg value: ").Append(avg.ToString("F1"));

            _text.SetText(_sb);
        }
    }

}
