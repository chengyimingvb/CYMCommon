////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    public static bool RainDrop_Switch = false;
    private static CameraPlay_RainDrop CamRainDrop;

    /// <summary>
    ///  Active Rain Drop FX. Add a Rain drop effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    public static void RainDrop_ON(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamRainDrop != null) return;
        if (RainDrop_Switch) return;
        CamRainDrop = CurrentCamera.gameObject.AddComponent<CameraPlay_RainDrop>() as CameraPlay_RainDrop;
        if (CamRainDrop.CamTurnOff) return;
        RainDrop_Switch = true;
        CamRainDrop.Duration = Time;
        CamRainDrop.Zoom = 0.2f;
        CamRainDrop.Distortion = 1;
    }
 
    /// <summary>
    /// Active Rain Drop FX. Add a Rain drop effect to the current camera.
    /// </summary>
    public static void RainDrop_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamRainDrop != null) return;
        if (RainDrop_Switch) return;
        CamRainDrop = CurrentCamera.gameObject.AddComponent<CameraPlay_RainDrop>() as CameraPlay_RainDrop;
        if (CamRainDrop.CamTurnOff) return;
        RainDrop_Switch = true;
        CamRainDrop.Zoom = 0.2f;
        CamRainDrop.Distortion = 1;
        CamRainDrop.Duration = 1;
    }
	/// <summary>
    /// Turn off the Rain Drop Fx
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    public static void RainDrop_OFF(float Time)
    {
        if (CamRainDrop == null) return;
        if (!RainDrop_Switch) return;
        CamRainDrop.Duration = Time;
        CamRainDrop.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off the Rain Drop Fx
    /// </summary>
    public static void RainDrop_OFF()
    {
        if (CamRainDrop == null) return;
        if (!RainDrop_Switch) return;
        CamRainDrop.Duration = 1;
        CamRainDrop.flashy = 1;
        CamRainDrop.CamTurnOff = true;
    }
    
 
}