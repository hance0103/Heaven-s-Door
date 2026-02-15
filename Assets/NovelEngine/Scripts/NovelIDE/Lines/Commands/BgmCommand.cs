using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public class SoundCommand : CommandLine
    {
        public string soundName;
        public int? volume;
        public bool loop;
        public bool isPlay;
        public NovelSound soundType;

        public SoundCommand(
            int index,
            string bgmName,
            int? volume, 
            bool loop,
            bool isPlay,
            NovelSound soundType,
            IfParameter ifParameter = null)
            : base(index, DialogoueType.CommandLine)
        {
            this.soundName = bgmName;
            this.volume = volume;
            this.loop = loop;
            this.isPlay = isPlay;
            this.soundType = soundType;
            this.ifParameter = ifParameter;
        }

        public override async UniTask Execute()
        {
            //if (!ifParameter)
            //    return;
            Debug.Log("재생");

            var audio = NovelManager.Instance.Audio;
            float volumeFloat = (volume ?? 100f) / 100f;
            // 따로 사운드 관리하는거 만들기
            switch (soundType)
            {
                case NovelSound.Bgm:
                    if (isPlay)
                    {
                        // BGM 재생
                        
                        audio.Play(soundName, volumeFloat, soundType);
                    }
                    else
                    {
                        // BGM 중단
                        audio.StopBGM();
                    }
                    break;
                case NovelSound.Effect:
                    if (isPlay)
                    {
                        // SFX 재생
                        audio.Play(soundName, volumeFloat, NovelSound.Effect);
                       
                    }
                    else
                    {
                        // SFX 중단
                        //SoundManager.Instance.StopSFX(bgmName);
                    }
                    break;
            }

        }
    }

}