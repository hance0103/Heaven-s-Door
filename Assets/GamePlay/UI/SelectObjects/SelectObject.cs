using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.SelectObjects
{
    [RequireComponent(typeof(Outline))]
    public abstract class SelectObject : MonoBehaviour
    {
        private Outline outline;

        protected virtual void Awake()
        {
            outline = GetComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(5, -5);
        }

        public void Select()
        {
            outline.effectColor = Color.yellow;
        }

        public void Deselect()
        {
            outline.effectColor = Color.white;
        }
        
        public abstract void Execute();
    }
}
