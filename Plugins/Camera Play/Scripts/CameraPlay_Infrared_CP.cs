////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool Infrared_Switch = false;
    private static CameraPlay_Infrared CamInfrared;
    /// <summary>
    /// Active Infrared FX. Add a Infrared effect to the current camera.
    /// </summary>
    public static void Infrared_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamInfrared != null) return;
        if (Infrared_Switch) return;
        CamInfrared = CurrentCamera.gameObject.AddComponent<CameraPlay_Infrared>() as CameraPlay_Infrared;
        if (CamInfrared.CamTurnOff) return;
        Infrared_Switch = true;
        CamInfrared.Duration = 1;
    }
    /// <summary>
    /// Active Infrared FX. Add a Infrared effect to the current camera.
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void Infrared_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamInfrared != null) return;
        if (Infrared_Switch) return;
        CamInfrared = CurrentCamera.gameObject.AddComponent<CameraPlay_Infrared>() as CameraPlay_Infrared;
        if (CamInfrared.CamTurnOff) return;
        Infrared_Switch = true;
        CamInfrared.Duration = time;
    }
    /// <summary>
    /// Turn off Infrared FX.
    /// </summary>
    public static void Infrared_OFF()
    {
        if (CamInfrared == null) return;
        if (!Infrared_Switch) return;
        CamInfrared.Duration = 1;
        CamInfrared.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off Infrared FX.
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void Infrared_OFF(float time)
    {
        if (CamInfrared == null) return;
        if (!Infrared_Switch) return;
        CamInfrared.Duration = time;
        CamInfrared.CamTurnOff = true;
    }

}