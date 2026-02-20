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
            Deselect();
        }

        public void Select()
        {
            text.color = Color.yellow;
        }

        public void Deselect()
        {
            text.color = Color.white;
        }
        
        public abstract void Execute();
    }
}
