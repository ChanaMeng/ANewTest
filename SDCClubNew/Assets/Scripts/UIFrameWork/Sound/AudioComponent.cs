using System.Collections.Generic;
using SDClub.Core;
using UnityEngine;

namespace SDClub.UIFrameWork
{
    public class AudioComponent : Entity, IAwake, IDestroy
    {
        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private Transform audioRoot;
        
        // BGM 和 SFX 音量
        public float BGMVolume { get; set; } = 1f;
        public float SFXVolume { get; set; } = 1f;
        
        public void Initialize(Transform root)
        {
            audioRoot = root;
            
            // 创建 BGM AudioSource
            var bgmGo = new GameObject("BGM");
            bgmGo.transform.SetParent(audioRoot);
            bgmSource = bgmGo.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            
            // 创建 SFX AudioSource
            var sfxGo = new GameObject("SFX");
            sfxGo.transform.SetParent(audioRoot);
            sfxSource = sfxGo.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        public void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;
            bgmSource.clip = clip;
            bgmSource.volume = BGMVolume;
            bgmSource.Play();
        }
        
        public void StopBGM()
        {
            bgmSource?.Stop();
        }
        
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.volume = SFXVolume;
            sfxSource.PlayOneShot(clip);
        }
        
        public void SetBGMVolume(float volume)
        {
            BGMVolume = Mathf.Clamp01(volume);
            if (bgmSource) bgmSource.volume = BGMVolume;
        }
        
        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
        }
    }
}
