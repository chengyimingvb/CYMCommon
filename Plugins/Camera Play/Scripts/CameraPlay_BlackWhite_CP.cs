////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool BlackWhite_Switch = false;
    private static CameraPlay_BlackWhite CamBlackWhite;
    /// <summary>
    /// Black and White FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void BlackWhite_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamBlackWhite != null) return;
        if (BlackWhite_Switch) return;
        CamBlackWhite = CurrentCamera.gameObject.AddComponent<CameraPlay_BlackWhite>() as CameraPlay_BlackWhite;
        if (CamBlackWhite.CamTurnOff) return;
        BlackWhite_Switch = true;
        CamBlackWhite.Duration = 1;
    }
    /// <summary>
    /// Black and White FX.
    /// </summary>
    /// <param name="time">Set the apparition time in sec</param>
    public static void BlackWhite_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamBlackWhite != null) return;
        if (BlackWhite_Switch) return;
        CamBlackWhite = CurrentCamera.gameObject.AddComponent<CameraPlay_BlackWhite>() as CameraPlay_BlackWhite;
        if (CamBlackWhite.CamTurnOff) return;
        BlackWhite_Switch = true;
        CamBlackWhite.Duration = time;
    }
    /// <summary>
    /// Turn Off The Black and White FX.
    /// </summary>
    public static void BlackWhite_OFF()
    {
        if (CamBlackWhite == null) return;
        if (!BlackWhite_Switch) return;
        CamBlackWhite.Duration = 1;
        CamBlackWhite.CamTurnOff = true;
    }
    /// <summary>
    /// Turn Off The Black and White FX with a specific timer.
    /// </summary>
    /// <param name="time">Set the apparition time in sec</param>
    public static void BlackWhite_OFF(float time)
    {
        if (CamBlackWhite == null) return;
        if (!BlackWhite_Switch) return;
        CamBlackWhite.Duration = time;
        CamBlackWhite.CamTurnOff = true;
    }

}