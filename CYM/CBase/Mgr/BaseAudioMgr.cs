using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CYM.Audio;
namespace CYM
{
    public class BaseAudioMgr : BaseCoreMgr
    {
        public const float DefaultMaxDistance = 18.0f;
        protected HashSet<AudioClip> CacheAudioClip = new HashSet<AudioClip>();
        protected IBaseSettingsMgr SettingsMgr => SelfBaseGlobal.SettingsMgr;

        #region prop
        public bool IsEnableSFX { get; protected set; } = true;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void Birth()
        {
            base.Birth();
            CacheAudioClip.Clear();
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if(CacheAudioClip.Count>0)
                CacheAudioClip.Clear();
        }
        #endregion

        #region set
        /// <summary>
        /// 关闭音效
        /// 和MuteSFX 的区别在于这个用于临时处理,不会保存到数据库
        /// </summary>
        public void EnableSFX(bool b)
        {
            IsEnableSFX = b;
        }
        public AudioSource PlayUI(AudioClip clip, bool isLoop = false, bool isCache = false)
        {
            if (isCache)
            {
                if (!CacheAudioClip.Contains(clip))
                    CacheAudioClip.Add(clip);
                else
                    return null;
            }
            var temp= SoundManager.PlaySFX(clip, isLoop);
            if (temp != null)
                temp.spatialBlend = 0.0f;
            return temp;
        }
        public AudioSource PlaySFX(AudioClip clip,Vector3? pos=null, bool isLoop = false, bool isCache = false,bool isForce=false,float volume= float.MaxValue)
        {
            if (clip == null)
                return null;
            if (!isForce&&!IsEnableSFX)
                return null;
            if (isCache)
            {
                if (!CacheAudioClip.Contains(clip))
                    CacheAudioClip.Add(clip);
                else
                    return null;
            }
            var ret = SoundManager.PlaySFX(clip, isLoop,0.0f, volume,float.MaxValue, pos.HasValue?pos.Value: default);
            if (ret != null)
            {
                ret.rolloffMode = AudioRolloffMode.Linear;
                ret.spatialBlend = 1.0f;
                ret.maxDistance = DefaultMaxDistance;
            }
            return ret;
        }
        public void PlayPlayerUI(string clipName, BaseUnit player, bool isLoop = false)
        {
            AudioSource temp = PlayPlayerSFX(clipName, player, SelfBaseGlobal.Pos, isLoop);
            if (temp != null)
                temp.spatialBlend = 0.0f;
        }
        public AudioSource PlayPlayerSFX(string clipName, BaseUnit player, Vector3? pos = null, bool isLoop = false, float maxDis = 60.0f)
        {
            if (clipName == null)
                return null;
            if (player == null)
                return null;
            if (!player.IsLocalPlayer())
                return null;
            return PlaySFX(clipName, pos, isLoop, maxDis);
        }
        public void PlayMusic(AudioClip clip, bool isLoop = false)
        {
             SoundManager.Play(clip,isLoop);
        }
        public void Stop()
        {
            SoundManager.Stop();
        }
        public virtual void Mute(bool toggle = true)
        {
            SoundManager.Mute(toggle);
            SettingsMgr.GetBaseSettings().Mute = toggle;
        }
	    public virtual void MuteMusic(bool toggle = true)
	    {
	    	SoundManager.MuteMusic(toggle);
            SettingsMgr.GetBaseSettings().MuteMusic = toggle;
        }
	    public virtual void MuteSFX(bool toggle = true)
	    {
	    	SoundManager.MuteSFX(toggle);
            SettingsMgr.GetBaseSettings().MuteSFX = toggle;
        }
        public virtual void MuteVoice(bool toggle = true)
        {
            SettingsMgr.GetBaseSettings().MuteVoice = toggle;
        }
        public virtual void SetVolumeMusic(float volume)
        {
            SoundManager.SetVolumeMusic(volume);
            SettingsMgr.GetBaseSettings().VolumeMusic = volume;
        }
        public virtual void SetVolumeSFX(float volume)
        {
            SoundManager.SetVolumeSFX(volume);
            SettingsMgr.GetBaseSettings().VolumeSFX = volume;
        }
	    public virtual void SetVolume(float volume)
	    {
	    	SoundManager.SetVolume(volume);
            SettingsMgr.GetBaseSettings().Volume = volume;
        }
        public virtual void SetVolumeVoice(float volume)
        {
            SettingsMgr.GetBaseSettings().VolumeVoice = volume;
        }
        public AudioSource PlayUI(string clipName, bool isLoop = false)
        {
            if (clipName.IsInvStr())
                return null;
            AudioSource temp = PlaySFX(clipName, SelfBaseGlobal.CameraMgr.CameraPos, isLoop,18,false,true);
            if (temp != null)
                temp.spatialBlend = 0.0f;
            return temp;
        }
        public AudioSource PlaySFX(string clipName, Vector3? pos = null, bool isLoop = false, float maxDis = DefaultMaxDistance, bool isCache = false, bool isForce = false, float volume = float.MaxValue)
        {
            if (!isForce && !IsEnableSFX)
                return null;
            if (clipName == null)
                return null;
            if (clipName.IsInvStr())
                return null;
            AudioSource temp = null;
            var clip = SelfBaseGlobal.GRMgr.GetAudioClip(clipName);
            if (isCache)
            {
                if (!CacheAudioClip.Contains(clip))
                    CacheAudioClip.Add(clip);
                else
                    return null;
            }

            if (pos.HasValue)
                temp = SoundManager.PlaySFX(clip, isLoop, 0.0f, volume, float.MaxValue, pos.Value);
            else
                temp = SoundManager.PlaySFX(clip, isLoop, 0.0f, volume, float.MaxValue);
            if (temp != null)
            {
                temp.rolloffMode = AudioRolloffMode.Linear;
                temp.spatialBlend = 1.0f;
                temp.maxDistance = maxDis;
            }
            return temp;
        }
        public void PlaySFX(string[] clipName, Vector3? pos = null, bool isLoop = false, float maxDis = 60.0f)
        {
            if (clipName == null)
                return;
            foreach (var item in clipName)
                PlaySFX(item,  pos ,  isLoop ,  maxDis);
        }


        public void AddToCache(AudioClip clip)
        {
            CacheAudioClip.Add(clip);
        }
        public bool IsInCache(AudioClip clip)
        {
            if (CacheAudioClip.Contains(clip))
                return true;
            return false;
        }
        #endregion

        #region get
        public virtual bool IsMute()
        {
            return SettingsMgr.GetBaseSettings().Mute;
        }
        public virtual bool IsMuteMusic()
        {
            return SettingsMgr.GetBaseSettings().MuteMusic;
        }
        public virtual bool IsMuteSFX()
        {
            return SettingsMgr.GetBaseSettings().MuteSFX;
        }
        public virtual bool IsMuteVoice()
        {
            return SettingsMgr.GetBaseSettings().MuteVoice;
        }
        public virtual float GetVolumeMusic()
        {
            return SettingsMgr.GetBaseSettings().VolumeMusic;
        }
        public virtual float GetVolumeSFX()
        {
            return SettingsMgr.GetBaseSettings().VolumeSFX;
        }
        public virtual float GetVolume()
        {
            return SettingsMgr.GetBaseSettings().Volume;
        }
        public virtual float GetVolumeVoice()
        {
            return SettingsMgr.GetBaseSettings().VolumeVoice;
        }
        #endregion

        #region real
        public virtual float GetRealVolumeVoice()
        {
            return SettingsMgr.GetBaseSettings().VolumeVoice * SettingsMgr.GetBaseSettings().Volume;
        }

        public virtual float GetRealVolumeSFX()
        {
            return SoundManager.GetVolumeSFX() * SoundManager.GetVolume();
        }
        #endregion

    }
}
