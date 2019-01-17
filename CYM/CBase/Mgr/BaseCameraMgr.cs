using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static CameraPlay;
using CYM.Highlighting;
using UnityEngine.Internal;

namespace CYM
{
    public class BaseCameraMgr : BaseGlobalCoreMgr
    {
        #region prop
        public Camera MainCamera { get; private set; }
        protected Transform MainCameraTrans { get; set; }
        protected HighlighterRenderer HighlighterRenderer { get; set; }
        public const float TopHight = 300.0f;
        public const float MidHight = 137.0f;
        public float CameraHight { get; private set; }
        public bool IsTopHight { get { return CameraHight >= TopHight; } }
        public bool IsLowHight { get { return CameraHight <= MidHight; } }
        public bool IsMiddleHight { get { return CameraHight > MidHight && CameraHight < TopHight; } }
        public float ZoomPercent { get { return CameraHight / 400; } }
        #endregion

        #region mgr
        protected IBaseSettingsMgr SettingsMgr => SelfBaseGlobal.SettingsMgr;
        protected BaseDBSettingsData BaseSettings => SelfBaseGlobal.SettingsMgr.GetBaseSettings();
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        /// <summary>
        /// 组建被添加到mono的时候
        /// </summary>
        /// <param name="mono"></param>
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            MainCamera = Mono.GetComponentInChildren<Camera>();
            MainCameraTrans = MainCamera.transform;
            HighlighterRenderer = Mono.GetComponentInChildren<HighlighterRenderer>();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            CameraHight = MainCameraTrans.position.y;
        }
        #endregion

        #region get
        public Vector3 CameraPos
        {
            get { return MainCameraTrans.position; }
        }

        #endregion

        #region set
        public void EnableSkyBox(bool b)
        {
            if (b)
                MainCamera.clearFlags = CameraClearFlags.Skybox;
            else
                MainCamera.clearFlags = CameraClearFlags.SolidColor;
        }
        public virtual void EnableHUD(bool b)
        {
            MainCamera.allowHDR = b;
            BaseSettings.EnableHUD = b;
        }
        public virtual void EnableMSAA(bool b)
        {
            MainCamera.allowMSAA = b;
            BaseSettings.EnableMSAA = b;
        }
        public virtual void EnableBloom(bool b)
        {
            //if (Beautify != null)
            //    Beautify.bloom = b;
            BaseSettings.EnableBloom = b;
        }
        public virtual void EnableSSAO(bool b)
        {
            BaseSettings.EnableSSAO = b;
        }
        public virtual void EnableShadow(bool b)
        {
            BaseSettings.EnableShadow = b;
        }
        #endregion

        #region Camera Play
        /// <summary>
        /// 关闭摄像机特效
        /// </summary>
        public void Off_Filter()
        {
            CameraPlay.NightVision_OFF(0.5f);
            CameraPlay.Drunk_OFF(0.5f);
            CameraPlay.SniperScope_OFF();
            CameraPlay.BlackWhite_OFF();
            CameraPlay.FlyVision_OFF();
            CameraPlay.Fade_OFF();
            CameraPlay.Pixel_OFF();
            CameraPlay.Colored_OFF();
            CameraPlay.Thermavision_OFF();
            CameraPlay.Infrared_OFF();
            CameraPlay.WidescreenH_OFF();
            CameraPlay.RainDrop_OFF();
            CameraPlay.Inverse_OFF();
        }
        /// <summary>
        /// 水滴特效
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="time"></param>
        /// <param name="size"></param>
        public void DropWater(Vector3 pos, float time, float size)
        {
            CameraPlay.DropWater(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), 1.5f, 2f);
        }
        /// <summary>
        /// 夜视效果
        /// </summary>
        public void NightVision_ON(NightVision_Preset Preset, float time)
        {
            CameraPlay.NightVision_ON(Preset, time);
        }
        /// <summary>
        /// 地震
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="Speed"></param>
        /// <param name="Size"></param>
        public void EarthQuakeShake(float duration, float Speed, float Size)
        {
            CameraPlay.EarthQuakeShake(duration,  Speed,  Size);
        }
        /// <summary>
        /// 醉酒效果
        /// </summary>
        /// <param name="Preset"></param>
        /// <param name="time"></param>
        public void Drunk_ON(Drunk_Preset Preset = Drunk_Preset.Default, float time=1.0f)
        {
            CameraPlay.Drunk_ON(Preset, time);
        }
        public void Drunk_OFF(float time = 1.0f)
        {
            CameraPlay.Drunk_OFF(time);
        }
        /// <summary>
        /// 模糊效果
        /// </summary>
        /// <param name="Time"></param>
        public void Blur(float Time)
        {
            CameraPlay.Blur(Time);
        }
        /// <summary>
        /// 噪点效果
        /// </summary>
        /// <param name="Time"></param>
        public void Noise(float Time)
        {
            CameraPlay.Noise(Time);
        }
        /// <summary>
        /// 辐射效果
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <param name="Time"></param>
        /// <param name="size"></param>
        public void Radial(Vector3 pos, float Time, float size)
        {
            CameraPlay.Radial(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), Time, size);
        }
        /// <summary>
        /// 闪瞎狗眼效果
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <param name="Time"></param>
        /// <param name="SpeedFPS"></param>
        /// <param name="color"></param> 2f, 5, new Color(1, 1, 1, 1)
        public void MangaFlash(float x,float y, float Time, int SpeedFPS,  Color color)
        {
            CameraPlay.MangaFlash(x, y, Time, SpeedFPS, color);
        }
        public void MangaFlash(Vector3 pos, float Time, int SpeedFPS, Color color)
        {
            CameraPlay.MangaFlash(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), Time, SpeedFPS, color);
        }
        public void MangaFlash(Vector2 pos, float Time, int SpeedFPS, Color color)
        {
            CameraPlay.MangaFlash(pos.x,pos.y, Time, SpeedFPS, color);
        }
        /// <summary>
        /// 望远镜效果
        /// </summary>
        public void SniperScope_ON()
        {
            CameraPlay.SniperScope_ON();
        }
        /// <summary>
        /// 关闭望远镜效果
        /// </summary>
        public void SniperScope_OFF()
        {
            CameraPlay.SniperScope_OFF();
        }
        /// <summary>
        /// 黑白效果
        /// </summary>
        public void BlackWhite_ON()
        {
            CameraPlay.BlackWhite_ON();
        }
        /// <summary>
        /// 关闭黑白效果
        /// </summary>
        public void BlackWhite_OFF()
        {
            CameraPlay.BlackWhite_OFF();
        }
        /// <summary>
        /// 偏移效果
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <param name="Time"></param>
        /// <param name="dist"></param>
        public void Pitch(Vector3 pos, float Time, float dist)
        {
            CameraPlay.Pitch(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), Time, dist);
        }
        /// <summary>
        /// 飞行效果
        /// </summary>
        public void FlyVision_ON()
        {
            CameraPlay.FlyVision_ON();
        }
        /// <summary>
        /// 关闭飞行效果
        /// </summary>
        public void FlyVision_OFF()
        {
            CameraPlay.FlyVision_OFF();
        }
        /// <summary>
        /// 死鱼眼
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <param name="Time"></param>
        /// <param name="dist"></param>
        public void FishEye(Vector3 pos, float Time, float dist)
        {
            CameraPlay.FishEye(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), Time, dist);
        }
        /// <summary>
        /// 葛丽琪效果
        /// </summary>
        /// <param name="Time"></param>
        public void Glitch(float Time)
        {
            CameraPlay.Glitch(Time);
        }
        /// <summary>
        /// 渐隐效果
        /// </summary>
        /// <param name="col"></param>
        /// <param name="time"></param>
        public void Fade_ON(Color col, float time)
        {
            CameraPlay.Fade_ON(col, time);
        }
        /// <summary>
        /// 关闭渐隐效果
        /// </summary>
        /// <param name="col"></param>
        /// <param name="time"></param>
        public void Fade_OFF()
        {
            CameraPlay.Fade_OFF();
        }
        /// <summary>
        /// 像素
        /// </summary>
        /// <param name="size"></param>
        /// <param name="time"></param>
        public void Pixel_ON(float size, float time)
        {
            CameraPlay.Pixel_ON(size, time);
        }
        /// <summary>
        /// 关闭像素效果
        /// </summary>
        public void Pixel_OFF()
        {
            CameraPlay.Pixel_OFF();
        }
        /// <summary>
        /// 颜色效果
        /// </summary>
        /// <param name="col"></param>
        /// <param name="time"></param>
        public void Colored_ON(Color col, float time)
        {
            CameraPlay.Colored_ON(col, time);
        }
        /// <summary>
        /// 关闭颜色效果
        /// </summary>
        /// <param name="col"></param>
        /// <param name="time"></param>
        public void Colored_OFF()
        {
            CameraPlay.Colored_OFF();
        }
        /// <summary>
        /// 萨尔玛效果
        /// </summary>
        public void Thermavision_ON()
        {
            CameraPlay.Thermavision_ON();
        }
        /// <summary>
        /// 关闭萨尔玛效果
        /// </summary>
        public void Thermavision_OFF()
        {
            CameraPlay.Thermavision_OFF();
        }
        /// <summary>
        /// 红外线效果
        /// </summary>
        public void Infrared_ON()
        {
            CameraPlay.Infrared_ON();
        }
        /// <summary>
        /// 关闭红外线效果
        /// </summary>
        public void Infrared_OFF()
        {
            CameraPlay.Infrared_OFF();
        }
        /// <summary>
        /// 水平边框效果
        /// </summary>
        /// <param name="col"></param>
        /// <param name="time"></param>
        public void WidescreenH_ON(Color col, float time)
        {
            CameraPlay.WidescreenH_ON(col, time);
        }
        /// <summary>
        /// 关闭水平边框效果
        /// </summary>
        /// <param name="col"></param>
        /// <param name="time"></param>
        public void WidescreenH_OFF()
        {
            CameraPlay.WidescreenH_OFF();
        }
        /// <summary>
        /// 击中效果
        /// </summary>
        /// <param name="col"></param>
        /// <param name="Time"></param>
        public void Hit(Color col, float Time)
        {
            CameraPlay.Hit(col, Time);
        }
        /// <summary>
        /// 套色效果
        /// </summary>
        /// <param name="Time">Set the time effect in second</param>
        public void Chromatical(float Time)
        {
            CameraPlay.Chromatical(Time);
        }
        /// <summary>
        /// 闪光弹效果
        /// </summary>
        /// <param name="color">Set the Color</param>
        /// <param name="Time">Set the time effect in second</param>
        public void FlashLight(Color color, float Time)
        {
            CameraPlay.FlashLight(color, Time);
        }
        /// <summary>
        /// 发达效果
        /// </summary>
        /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
        /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
        /// <param name="Time">Set the time effect in second.</param>
        /// <param name="dist">Set the distorsion (1.0 = normal)</param>
        public void Zoom(Vector3 pos, float Time, float dist)
        {
            CameraPlay.Zoom(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), Time, dist);
        }
        /// <summary>
        /// 葛丽池3效果
        /// </summary>
        /// <param name="Time"></param>
        public void Glitch3(float Time)
        {
            CameraPlay.Glitch3(Time);
        }
        /// <summary>
        /// 子弹效果
        /// </summary>
        /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
        /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
        /// <param name="Time">Set the apparition time in sec</param>
        /// <param name="dist">Set the distorsion (1.0 = normal)</param>
        public void BulletHole(Vector3 pos, float Time, float dist)
        {
            CameraPlay.BulletHole(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), Time, dist);
        }
        /// <summary>
        /// 见血效果
        /// </summary>
        /// <param name="Time">Set the time effect in second</param>
        /// <param name="dist">Set the distorsion (1.0 = normal)</param>
        public void BloodHit(float Time, float dist)
        {
            CameraPlay.BloodHit(Time, dist);
        }
        /// <summary>
        /// 下雨效果
        /// </summary>
        /// <param name="Time">Set the time effect in second.</param>
        public void RainDrop_ON(float Time)
        {
            CameraPlay.RainDrop_ON(Time);
        }
        /// <summary>
        /// 关闭下雨效果
        /// </summary>
        /// <param name="Time">Set the time effect in second.</param>
        public void RainDrop_OFF()
        {
            CameraPlay.RainDrop_OFF();
        }
        /// <summary>
        /// 震动波效果
        /// </summary>
        /// <param name="x">The x position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
        /// <param name="y">The y position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
        /// <param name="time">Time duration of the effect animation in second.</param>
        /// <param name="size">Size of the Distortion FX</param>
        public void Shockwave(Vector3 pos, float time, float size)
        {
            CameraPlay.Shockwave(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), time, size);
        }
        /// <summary>
        /// 淡出效果
        /// </summary>
        /// <param name="color">Set the color of the fading effect</param>
        /// <param name="Time">Set the time effect in second</param>
        public void FadeInOut(Color color, float time)
        {
            CameraPlay.FadeInOut(color, time);
        }
        /// <summary>
        /// 反色
        /// </summary>
        /// <param name="time">Set the apparition time in sec</param>
        public void Inverse_ON(float time)
        {
            CameraPlay.Inverse_ON(time);
        }
        /// <summary>
        /// 关闭反色
        /// </summary>
        /// <param name="time">Set the apparition time in sec</param>
        public void Inverse_OFF()
        {
            CameraPlay.Inverse_OFF();
        }
        /// <summary>
        /// 捏动效果
        /// </summary>
        /// <param name="x">The x position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
        /// <param name="y">The y position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
        /// <param name="time">Time duration of the effect animation in second.</param>
        /// <param name="size">Size of the Distortion FX</param>
        public void Pinch(Vector3 pos, float time, float size)
        {
            CameraPlay.Pinch(CameraPlay.PosScreenX(pos), CameraPlay.PosScreenY(pos), time, size);
        }
        #endregion

    }

}