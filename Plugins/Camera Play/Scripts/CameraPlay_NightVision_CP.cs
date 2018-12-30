////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    // Use this for initialization
    public static bool NightVision_Switch = false;
    private static CameraPlay_NightVision CamNightVision;

    public enum NightVision_Preset
    {
        Night_Vision_FX = 0,
        Night_Vision_Classic = 1,
        Night_Vision_Full = 2,
        Night_Vision_Dark = 3,
        Night_Vision_Sharp = 4,
        Night_Vision_BlueSky = 5,
        Night_Vision_Low_Light = 6,
        Night_Vision_Pinky = 7,
        Night_Vision_RedBurn = 8,
        Night_Vision_PurpleShadow = 9
    };

    /// <summary>
    /// Active Night Vision FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void NightVision_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamNightVision != null) return;
        if (NightVision_Switch) return;
        CamNightVision = CurrentCamera.gameObject.AddComponent<CameraPlay_NightVision>() as CameraPlay_NightVision;
        if (CamNightVision.CamTurnOff) return;
        NightVision_Switch = true;
        CamNightVision.Duration = 1;
    }
    /// <summary>
    /// Active Night Vision FX with a specific apparition timing. 
    /// </summary>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void NightVision_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamNightVision != null) return;
        if (NightVision_Switch) return;
        CamNightVision = CurrentCamera.gameObject.AddComponent<CameraPlay_NightVision>() as CameraPlay_NightVision;
        if (CamNightVision.CamTurnOff) return;
        NightVision_Switch = true;
        CamNightVision.Duration = time;
    }
    /// <summary>
    /// Active Night Vision FX with specific NightVision FX using NightVision_Preset. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    /// <param name="Preset">Use a specific NightVisionFX using NightVision_Preset.</param>
    public static void NightVision_ON(NightVision_Preset Preset)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamNightVision != null) return;
        if (NightVision_Switch) return;
        CamNightVision = CurrentCamera.gameObject.AddComponent<CameraPlay_NightVision>() as CameraPlay_NightVision;
        if (CamNightVision.CamTurnOff) return;
        NightVision_Switch = true;
        CamNightVision.Duration = 1;
        CamNightVision.Preset = (CameraPlay_NightVision.NightVision_Preset)Preset;
    }
    /// <summary>
    /// Active Night Vision FX with specific NightVision FX using NightVision_Preset and with a specific apparition timing. 
    /// </summary>
    /// <param name="Preset">Use a specific NightVisionFX using NightVision_Preset.</param>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void NightVision_ON(NightVision_Preset Preset, float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamNightVision != null) return;
        if (NightVision_Switch) return;
        CamNightVision = CurrentCamera.gameObject.AddComponent<CameraPlay_NightVision>() as CameraPlay_NightVision;
        if (CamNightVision.CamTurnOff) return;
        NightVision_Switch = true;
        CamNightVision.Duration = time;
        CamNightVision.Preset = (CameraPlay_NightVision.NightVision_Preset)Preset;
    }
    /// <summary>
    /// Turn Off The NightVision FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void NightVision_OFF()
    {
        if (CamNightVision == null) return;
        if (!NightVision_Switch) return;
        CamNightVision.Duration = 1;
        CamNightVision.CamTurnOff = true;
    }
    /// <summary>
    /// Turn Off The NightVision FX with a specific timer.
    /// </summary>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void NightVision_OFF(float time)
    {
        if (CamNightVision == null) return;
        if (!NightVision_Switch) return;
        CamNightVision.Duration = time;
        CamNightVision.CamTurnOff = true;
    }
}