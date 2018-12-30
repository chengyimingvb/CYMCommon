using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CYM
{
    public class BaseVoiceMgr : BaseCoreMgr
    {
        public const float DefaultMaxDistance = 8.0f;
        protected AudioSource VoiceAudioSource { get; set; }

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            VoiceAudioSource = Mono.EnsureComponet<AudioSource>();
            VoiceAudioSource.playOnAwake = false;
            VoiceAudioSource.rolloffMode = AudioRolloffMode.Linear;
            VoiceAudioSource.spatialBlend = 1.0f;
            VoiceAudioSource.minDistance = 1.0f;
            VoiceAudioSource.maxDistance = DefaultMaxDistance;
        }
        #endregion

        #region set
        public void PlayPlayerSFX(string sfx)
        {
            if (!SelfBaseUnit.IsPlayerCtrl())
                return;
            SelfBaseGlobal.AudioMgr.PlaySFX(sfx, SelfBaseUnit.Pos, false, 0.1f);
        }
        public void PlaySFX(string sfx, float volume = 1.0f)
        {
            SelfBaseGlobal.AudioMgr.PlaySFX(sfx, SelfBaseUnit.Pos, false, 0.1f,false,false, SelfBaseGlobal.AudioMgr.GetRealVolumeSFX()* volume);
        }
        public void PlayVoice(string sfx, float volume = 1.0f,bool interrupt=true)
        {
            if (sfx.IsInvStr())
                return;
            if (!SelfBaseGlobal.AudioMgr.IsEnableSFX)
                return;
            if (SelfBaseGlobal.AudioMgr.IsMuteVoice())
                return;
            VoiceAudioSource.volume = volume * SelfBaseGlobal.AudioMgr.GetRealVolumeVoice();
            var clip = SelfBaseGlobal.GRMgr.GetAudioClip(sfx);
            if (!SelfBaseGlobal.AudioMgr.IsInCache(clip))
            {
                if (VoiceAudioSource.isPlaying && !interrupt)
                    return;
                SelfBaseGlobal.AudioMgr.AddToCache(clip);
                VoiceAudioSource.Stop();
                VoiceAudioSource.PlayOneShot(clip);
            }
        }
        public void PlayUI(string id)
        {
            SelfBaseGlobal.AudioMgr.PlayUI(id);
        }
        #endregion


    }

}