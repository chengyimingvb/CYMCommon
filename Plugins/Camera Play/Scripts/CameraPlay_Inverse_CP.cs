////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool Inverse_Switch = false;
    private static CameraPlay_Inverse CamInverse;
    /// <summary>
    /// Black and White FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void Inverse_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamInverse != null) return;
        if (Inverse_Switch) return;
        CamInverse = CurrentCamera.gameObject.AddComponent<CameraPlay_Inverse>() as CameraPlay_Inverse;
        if (CamInverse.CamTurnOff) return;
        Inverse_Switch = true;
        CamInverse.Duration = 1;
    }
    /// <summary>
    /// Black and White FX.
    /// </summary>
    /// <param name="time">Set the apparition time in sec</param>
    public static void Inverse_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamInverse != null) return;
        if (Inverse_Switch) return;
        CamInverse = CurrentCamera.gameObject.AddComponent<CameraPlay_Inverse>() as CameraPlay_Inverse;
        if (CamInverse.CamTurnOff) return;
        Inverse_Switch = true;
        CamInverse.Duration = time;
    }
    /// <summary>
    /// Turn Off The Black and White FX.
    /// </summary>
    public static void Inverse_OFF()
    {
        if (CamInverse == null) return;
        if (!Inverse_Switch) return;
        CamInverse.Duration = 1;
        CamInverse.CamTurnOff = true;
    }
    /// <summary>
    /// Turn Off The Black and White FX with a specific timer.
    /// </summary>
    /// <param name="time">Set the apparition time in sec</param>
    public static void Inverse_OFF(float time)
    {
        if (CamInverse == null) return;
        if (!Inverse_Switch) return;
        CamInverse.Duration = time;
        CamInverse.CamTurnOff = true;
    }

}