using System;
using TMPro;
using UnityEngine;

namespace GamePlay.UI.SelectObjects.Start
{
    public abstract class StartSelectObject : SelectObject
    {
        public abstract override void Execute();

        [SerializeField] private TMP_Text tmp;

        private void Start()
        {
            if (tmp == null)
                tmp = GetComponent<TMP_Text>();
        }

        public override void Select(Color color)
        {
            tmp.color = color;
        }
        
        public override void Deselect(Color color)
        {
            tmp.color = color;
        }
    }
}
