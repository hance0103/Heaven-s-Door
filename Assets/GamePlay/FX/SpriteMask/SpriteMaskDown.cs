using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GamePlay.FX.SpriteMask
{
    public class SpriteMaskDown : MonoBehaviour
    {
        public async UniTask StartMove(float duration = 1)
        {
            await transform.DOMove(Vector3.zero, duration);
            
            Debug.Log("끝까지 내려옴");
        }

    }
}
