using TMPro;
using UnityEngine;

namespace GamePlay.UI.SelectObjects
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class SelectObject : MonoBehaviour
    {
        private TMP_Text text;

        protected virtual void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        public void Select(int fontSize)
        {
            if (text == null) Debug.Log("널");
            
            text.color = Color.yellow;
            text.fontSize = fontSize;
        }

        public void Deselect(int fontSize)
        {
            text.color = Color.white;
            text.fontSize = fontSize;
        }
        
        public abstract void Execute();
    }
}
