using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace CYM
{
    public class BaseUtils 
    {
        #region get
        /// <summary>
        /// 简单系统信息
        /// </summary>
        public static string SimpleSystemInfo
        {
            get
            {
                string info =
                       "\nOS:" + SystemInfo.operatingSystem +
                       "\nProcessor:" + SystemInfo.processorType +
                       "\nMemory:" + SystemInfo.systemMemorySize +
                       "\nGraphics API:" + SystemInfo.graphicsDeviceType +
                       "\nGraphics Processor:" + SystemInfo.graphicsDeviceName +
                       "\nGraphics Memory:" + SystemInfo.graphicsMemorySize+
                       "\nGraphics Vendor:" + SystemInfo.graphicsDeviceVendor;
                return info;
            }
        }
        /// <summary>
        /// 基本系统信息
        /// </summary>
        public static string BaseSystemInfo
        {
            get
            {
                string systemInfo =
                "\nDeviceModel：" + SystemInfo.deviceModel +
                "\nDeviceName：" + SystemInfo.deviceName +
                "\nDeviceType：" + SystemInfo.deviceType +
                "\nGraphicsDeviceName：" + SystemInfo.graphicsDeviceName +
                "\nGraphicsDeviceVersion:" + SystemInfo.graphicsDeviceVersion +
                "\nGraphicsMemorySize（M）：" + SystemInfo.graphicsMemorySize +
                "\nGraphicsShaderLevel：" + SystemInfo.graphicsShaderLevel +
                "\nMaxTextureSize：" + SystemInfo.maxTextureSize +
                "\nOperatingSystem：" + SystemInfo.operatingSystem +
                "\nProcessorCount：" + SystemInfo.processorCount +
                "\nProcessorType：" + SystemInfo.processorType +
                "\nSystemMemorySize：" + SystemInfo.systemMemorySize;

                return systemInfo;
            }
        }
        /// <summary>
        /// 高级系统信息
        /// </summary>
        public static string AdvSystemInfo
        {
            get
            {
                string systemInfo =
                "\nDeviceModel：" + SystemInfo.deviceModel +
                "\nDeviceName：" + SystemInfo.deviceName +
                "\nDeviceType：" + SystemInfo.deviceType +
                "\nDeviceUniqueIdentifier：" + SystemInfo.deviceUniqueIdentifier +
                "\nGraphicsDeviceID：" + SystemInfo.graphicsDeviceID +
                "\nGraphicsDeviceName：" + SystemInfo.graphicsDeviceName +
                "\nGraphicsDeviceVendor：" + SystemInfo.graphicsDeviceVendor +
                "\nGraphicsDeviceVendorID:" + SystemInfo.graphicsDeviceVendorID +
                "\nGraphicsDeviceVersion:" + SystemInfo.graphicsDeviceVersion +
                "\nGraphicsMemorySize（M）：" + SystemInfo.graphicsMemorySize +
                "\nGraphicsShaderLevel：" + SystemInfo.graphicsShaderLevel +
                "\nMaxTextureSize：" + SystemInfo.maxTextureSize +
                "\nNpotSupport：" + SystemInfo.npotSupport +
                "\nOperatingSystem：" + SystemInfo.operatingSystem +
                "\nProcessorCount：" + SystemInfo.processorCount +
                "\nProcessorType：" + SystemInfo.processorType +
                "\nSupportedRenderTargetCount：" + SystemInfo.supportedRenderTargetCount +
                "\nSupports3DTextures：" + SystemInfo.supports3DTextures +
                "\nSupportsAccelerometer：" + SystemInfo.supportsAccelerometer +
                "\nSupportsComputeShaders：" + SystemInfo.supportsComputeShaders +
                "\nSupportsGyroscope：" + SystemInfo.supportsGyroscope +
                "\nSupportsImageEffects：" + SystemInfo.supportsImageEffects +
                "\nSupportsInstancing：" + SystemInfo.supportsInstancing +
                "\nSupportsLocationService：" + SystemInfo.supportsLocationService +
                "\nSupportsRenderToCubemap：" + SystemInfo.supportsRenderToCubemap +
                "\nSupportsShadows：" + SystemInfo.supportsShadows +
                "\nSupportsSparseTextures：" + SystemInfo.supportsSparseTextures +
                "\nSupportsVibration：" + SystemInfo.supportsVibration +
                "\nSystemMemorySize：" + SystemInfo.systemMemorySize;

                return systemInfo;
            }
        }
        #endregion

        #region other
        public static T CreateGlobalObj<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            GameObject.DontDestroyOnLoad(go);
            T instance = go.AddComponent<T>();
            return instance;
        }
        public static Color FromHex(string hex)
        {
            if (hex == null)
            {
                CLog.Error("字符窜为null");
                return Color.black;
            }
            if (hex.Length < 6)
            {
                CLog.Error("颜色代码格式不对");
                return Color.black;
            }
            float r = (byte)System.Convert.ToInt32(hex.Substring(0, 2), 16) / 255.0f;             //取出高位R的分量
            float g = (byte)System.Convert.ToInt32(hex.Substring(2, 2), 16) / 255.0f;            //取出高位G的分量
            float b = (byte)System.Convert.ToInt32(hex.Substring(4, 2), 16) / 255.0f;            //取出高位B的分量
            return new Color(r, g, b, 1.0f);
        }
        public static bool IsArrayValid<T>(IList<T> data)
        {
            if (data == null) return false;
            if (data.Count <= 0)
                return false;
            return true;
        }
        public static void CopyTextToClipboard(string str)
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = str;
            textEditor.OnFocus();
            textEditor.Copy();
        }
        /// <summary>
        /// 遍历枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumType"></param>
        /// <param name="callback"></param>
        public static void ForeachEnum<T>(Action<T> callback) where T : struct
        {
            for (var type = 0; type < Enum.GetValues(typeof(T)).Length; ++type)
            {
                callback((T)(object)(type));
            }
        }
        #endregion


    }

}