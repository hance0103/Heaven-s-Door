using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class NovelVariable
    {
        [SerializeReference]
        public NovelParameter parameter;
    }
}
