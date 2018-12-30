//**********************************************
// Class Name	: CYMBaseSettingsManager
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using CYM.UI;

namespace CYM
{
    [Serializable]
    public class BaseDBSettingsData
    {
        /// <summary>
        /// 语言类型
        /// </summary>
        public LanguageType LanguageType = LanguageType.English;
        /// <summary>
        /// 禁止背景音乐
        /// </summary>
        public bool MuteMusic = false;
        /// <summary>
        /// 禁止音效
        /// </summary>
        public bool MuteSFX = false;
        /// <summary>
        /// 静止语音效果
        /// </summary>
        public bool MuteVoice = false;
        /// <summary>
        /// 静止所有音乐
        /// </summary>
        public bool Mute = false;
        /// <summary>
        /// 是否静止环境音效
        /// </summary>
        public bool MuteAmbient = false;
        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float VolumeMusic = 0.2f;
        /// <summary>
        /// 音效音量
        /// </summary>
        public float VolumeSFX = 0.8f;
        /// <summary>
        /// 主音量
        /// </summary>
        public float Volume = 1.0f;
        /// <summary>
        /// 语音音量
        /// </summary>
        public float VolumeVoice = 0.5f;
        /// <summary>
        /// 自动存储类型
        /// </summary>
        public AutoSaveType AutoSaveType = AutoSaveType.None;
        /// <summary>
        /// 开启HUD
        /// </summary>
        public bool EnableHUD = true;
        /// <summary>
        /// 开启MSAA
        /// </summary>
        public bool EnableMSAA = true;
        /// <summary>
        /// 开启后期效果
        /// </summary>
        public bool EnableBeautify = true;
        /// <summary>
        /// 开启SSAO
        /// </summary>
        public bool EnableSSAO = false;
        /// <summary>
        /// 显示FPS
        /// </summary>
        public bool ShowFPS = false;
        /// <summary>
        /// 开启Bloom效果
        /// </summary>
        public bool EnableBloom = true;
        /// <summary>
        /// 开启shadow
        /// </summary>
        public bool EnableShadow = true;
        /// <summary>
        /// 游戏画质
        /// </summary>
        public GamePropType Quality = GamePropType.Hight;
        /// <summary>
        /// 游戏分辨率,通常选择小一号的窗口模式
        /// </summary>
        public int Resolution = 1;
        /// <summary>
        /// 全屏
        /// </summary>
        public bool FullScreen = false;
    }

    [Serializable]
    public class BaseDevSettingsData
    {
        /// <summary>
        /// 没有剧情
        /// </summary>
        public bool NoPlot = false;
    }

    public class BaseSettingsMgr<TDBSetting,TDevSetting> : BaseGlobalCoreMgr, IBaseSettingsMgr where  TDBSetting: BaseDBSettingsData,new() where TDevSetting: BaseDevSettingsData, new()
    {
        #region prop
        public TDBSetting Settings { get; protected set; } = new TDBSetting();
        public TDevSetting DevSettings { get; protected set; }
        public bool IsFirstCreateSettings { get; set; } = false;
        #endregion

        #region Callback Val
        /// <summary>
        /// 还原设置
        /// </summary>
        public event Callback<TDBSetting> Callback_OnRevert;
        /// <summary>
        /// 设置初始化
        /// </summary>
        public event Callback Callback_OnInitSettings;
        /// <summary>
        /// 第一次创建设置文件回调
        /// </summary>
        public event Callback<TDBSetting> Callback_OnFirstCreateSetting;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            InitAllResolutions();
            Callback_OnRevert += OnRevert;
            Callback_OnInitSettings += OnInitSetting;
            Callback_OnFirstCreateSetting += OnFirstCreateSetting;
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);

            if (IsEnableDevSetting)
            {
                DevSettings = BaseFileUtils.LoadJsonOrDefault(BaseConstMgr.Path_DevSettings, CreateDefaultDevSettings());
            }
            else
            {
                DevSettings = CreateDefaultDevSettings();
            }

            string fullpath = BaseConstMgr.Path_Settings;
            TDBSetting settings = default(TDBSetting);
            if (File.Exists(fullpath))
            {
                using (Stream stream = File.OpenRead(fullpath))
                {
                    if (stream != null)
                    {
                        try
                        {
                            settings = BaseFileUtils.LoadJson<TDBSetting>(stream);
                        }
                        catch (Exception e)
                        {
                            settings = default(TDBSetting);
                            CLog.Error("载入settings出错{0}", e);
                        }
                    }

                }
            }
            if (settings == null)
            {
                IsFirstCreateSettings = true;
                settings = new TDBSetting();
                Save();
            }
            SetSettings(settings);
        }
        public override void OnStart()
        {
            base.OnStart();
            if (IsFirstCreateSettings)
                Callback_OnFirstCreateSetting?.Invoke(Settings);
            Callback_OnInitSettings?.Invoke();
        }
        /// <summary>
        /// mono的OnDisable
        /// </summary>
        public override void OnDisable()
        {
            Callback_OnRevert -= OnRevert;
            Callback_OnInitSettings -= OnInitSetting;
            Callback_OnFirstCreateSetting -= OnFirstCreateSetting;
            base.OnDisable();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否启用DevSetting
        /// </summary>
        public bool IsEnableDevSetting
        {
            get
            {
                return BuildConfig.Ins.IsDevBuild && BuildLocalConfig.Ins.IsEnableDevSetting;
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 还原设置
        /// </summary>
        public virtual void Revert()
        {
            Settings = new TDBSetting();
            Callback_OnRevert?.Invoke(Settings);
        }
        /// <summary>
        /// 创建默认的DevSetting
        /// </summary>
        /// <returns></returns>
        public static TDevSetting CreateDefaultDevSettings()
        {
            return new TDevSetting();
        }
        /// <summary>
        /// 设置设置
        /// </summary>
        /// <param name="data"></param>
        public void SetSettings(TDBSetting data)
        {
            Settings = data;
        }
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            //var temp =;
            using (Stream stream = new FileStream(BaseConstMgr.Path_Settings, FileMode.Create) /*Storage.OpenWrite (SettingsPath)*/)
            {
                BaseFileUtils.SaveJson(stream, Settings);
                stream.Close();
            }
        }
        /// <summary>
        /// 载入
        /// </summary>
        void Load()
        {

        }
        /// <summary>
        /// 设置分辨率
        /// </summary>
        public virtual void SetResolution(int index,bool isFullScreen)
        {
            if (Resolutions.Count <= index)
                return;
            Screen.SetResolution(Resolutions[index].width, Resolutions[index].height,isFullScreen);
            Settings.Resolution = index;
            Settings.FullScreen = isFullScreen;
        }
        /// <summary>
        /// 设置全屏
        /// </summary>
        public virtual void SetFullScreen(bool isFullScreen)
        {
            Screen.fullScreen = isFullScreen;
            Settings.FullScreen = isFullScreen;
        }
        /// <summary>
        /// 设置画质
        /// </summary>
        public virtual void SetQuality(int index)
        {
            QualitySettings.SetQualityLevel(index);
            Settings.Quality = (GamePropType)index;
        }
        #endregion

        #region get
        protected HashSet<string> ResolutionsKey = new HashSet<string>();
        protected List<Resolution> Resolutions = new List<Resolution>();
        protected virtual void InitAllResolutions()
        {
            ResolutionsKey.Clear();
            Resolutions.Clear();
            OnAddBuiltInResolution();
            foreach (var item in Screen.resolutions)
            {
                string customKey = string.Format($"{item.width}x{item.height}");
                if (!ResolutionsKey.Contains(customKey))
                {
                    ResolutionsKey.Add(customKey);
                    Resolutions.Add(item);
                }
            }

            Resolutions.Sort((x,y)=> {

                if (x.width > y.width)
                    return -1;
                else
                    return 1;

                #region 
                //if (
                //Is16_9(x) &&
                //Is16_9(y)
                //)
                //{
                //    if (x.width> y.width)
                //        return -1;
                //    else
                //        return 1;
                //}
                //else
                //{
                //    if (Is16_9(x))
                //    {
                //        return -1;
                //    }
                //    else
                //    {
                //        if (
                //        Is16_10(x) &&
                //        Is16_10(y)
                //        )
                //        {
                //            if (x.width > y.width)
                //                return -1;
                //            else
                //                return 1;
                //        }
                //        else
                //        {
                //            if (Is16_10(x))
                //            {
                //                return -1;
                //            }
                //        }
                //    }
                //}
                #endregion
            });
        }
        bool Is16_9(Resolution resolution)
        {
            if (resolution.width % 16 == 0 &&
                resolution.height % 9 == 0)
                return true;
            return false;
        }
        bool Is16_10(Resolution resolution)
        {
            if (resolution.width % 16 == 0 &&
                resolution.height % 10 == 0)
                return true;
            return false;
        }
        protected virtual void OnAddBuiltInResolution()
        {
        }
        protected void AddBuiltInResolution(int width,int height)
        {
            string customKey = string.Format($"{width}x{height}");
            ResolutionsKey.Add(customKey);
            Resolutions.Add(new Resolution {width=width,height=height });
        }
        public virtual string[] GetResolutionStrs()
        {
            return Resolutions.Select(x=>x.ToString()).ToArray();
        }
        public BaseDBSettingsData GetBaseSettings()
        {
            return Settings;
        }
        public BaseDevSettingsData GetBaseDevSettings()
        {
            return DevSettings;
        }
        #endregion

        #region Callback
        protected virtual void OnRevert(TDBSetting data)
        {
            SelfBaseGlobal.AudioMgr.MuteMusic(data.MuteMusic);
            SelfBaseGlobal.AudioMgr.MuteSFX(data.MuteSFX);
            SelfBaseGlobal.AudioMgr.MuteVoice(data.MuteVoice);
            SelfBaseGlobal.AudioMgr.SetVolumeMusic(data.VolumeMusic);
            SelfBaseGlobal.AudioMgr.SetVolumeSFX(data.VolumeSFX);
            SelfBaseGlobal.AudioMgr.SetVolume(data.Volume);

            SelfBaseGlobal.CameraMgr.EnableHUD(data.EnableHUD);
            SelfBaseGlobal.CameraMgr.EnableMSAA(data.EnableMSAA);
            SelfBaseGlobal.CameraMgr.EnableBeautify(data.EnableBeautify);
            SelfBaseGlobal.CameraMgr.EnableBloom(data.EnableBloom);
            SelfBaseGlobal.CameraMgr.EnableSSAO(data.EnableSSAO);

            SelfBaseGlobal.FPSMgr.ShowFPS(data.ShowFPS);

            SetQuality((int)data.Quality);
            SetResolution(data.Resolution, data.FullScreen);
        }
        protected virtual void OnInitSetting()
        {
            OnRevert(Settings);
        }
        protected virtual void OnFirstCreateSetting(TDBSetting arg1)
        {
            
        }
        #endregion
    }
}