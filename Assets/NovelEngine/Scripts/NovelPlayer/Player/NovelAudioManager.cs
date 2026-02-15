using UnityEngine;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;
namespace novel
{

    public class NovelAudioManager
    {
        private AudioMixer audioMixer;
        private readonly AudioSource[] _audioSources = new AudioSource[(int)NovelSound.MaxCount];

        public async UniTask AudioManagerInitAsync()
        {
            try
            {
                await UniTask.Yield();

                audioMixer = NovelManager.Data.audio.GetMixer();

                if (audioMixer != null)
                {

                    AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups("Master");
                    GameObject root = new GameObject { name = "@Sound" };
                    root.transform.SetParent(NovelManager.Instance.transform, false);

                    string[] soundNames = System.Enum.GetNames(typeof(NovelSound));
                    for (int i = 0; i < soundNames.Length - 1; i++)
                    {
                        GameObject go = new GameObject { name = soundNames[i] };
                        _audioSources[i] = go.AddComponent<AudioSource>();
                        go.transform.parent = root.transform;
                        go.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroups[i + 1];
                    }
                }
                else
                {
                    Debug.Log("믹서 없음");
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
                throw;
            }

        }
        public void Play(string audioName, float volume, NovelSound soundType = NovelSound.Effect)
        {
            AudioClip clip = GetAudioClip(audioName, soundType);
            switch (soundType)
            {
                case NovelSound.Bgm:
                    PlayBGM(clip, volume);
                    break;
                case NovelSound.Effect:
                    PlaySfx(clip, volume);
                    break;
            }
        }
        private void PlayBGM(AudioClip clip, float volume)
        {
            AudioSource audioSource = _audioSources[(int)NovelSound.Bgm];
            if (audioSource.isPlaying)
                audioSource.Stop();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }
        private void PlaySfx(AudioClip clip, float volume)
        {
            AudioSource audioSource = _audioSources[(int)NovelSound.Effect];
            audioSource.volume = volume;
            audioSource.PlayOneShot(clip);
        }
        public void StopBGM()
        {
            AudioSource bgmSource = _audioSources[(int)NovelSound.Bgm];
            if (bgmSource.isPlaying)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            }
        }
        private AudioClip GetAudioClip(string name, NovelSound soundType = NovelSound.Effect)
        {
            switch (soundType)
            {
                case NovelSound.Bgm:
                    return NovelManager.Data.audio.GetBGMByName(name);
                case NovelSound.Effect:
                    return NovelManager.Data.audio.GetSEByName(name);
                default:
                    return null;
            }
        }
    }

}
