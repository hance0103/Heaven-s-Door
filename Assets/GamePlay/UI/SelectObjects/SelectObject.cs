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
            //outline.effectColor = Color.white;
            //outline.effectDistance = new Vector2(5, -5);
        }

        public void SetOutlineWeight(Vector2 weight)
        {
            outline.effectDistance = weight;
        }
        public void Select(Color color)
        {
            outline.effectColor = color;
        }

        public void Deselect(Color color)
        {
            outline.effectColor = color;
        }
        
        public abstract void Execute();
    }
}
