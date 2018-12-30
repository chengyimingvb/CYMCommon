////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    public static bool FlyVision_Switch = false;
    private static CameraPlay_FlyVision CamFlyVision;
    /// <summary>
    ///  Active Fly Vision Fx. Add a Fly Vision effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="Zoom">Set the zoom </param>
    /// <param name="distortion">Set the distorsion (1.0 = normal)</param>
    public static void FlyVision_ON(float Time, float Zoom, float distortion)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFlyVision != null) return;
        if (FlyVision_Switch) return;
        CamFlyVision = CurrentCamera.gameObject.AddComponent<CameraPlay_FlyVision>() as CameraPlay_FlyVision;
        if (CamFlyVision.CamTurnOff) return;
        FlyVision_Switch = true;
        CamFlyVision.Duration = Time;
        CamFlyVision.Zoom = Zoom;
        CamFlyVision.Distortion = distortion;
    }
    /// <summary>
    /// Active Fly Vision Fx. Add a Fly Vision effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="Zoom">Set the zoom </param>
    public static void FlyVision_ON(float Time, float Zoom)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFlyVision != null) return;
        if (FlyVision_Switch) return;
        CamFlyVision = CurrentCamera.gameObject.AddComponent<CameraPlay_FlyVision>() as CameraPlay_FlyVision;
        if (CamFlyVision.CamTurnOff) return;
        FlyVision_Switch = true;
        CamFlyVision.Duration = Time;
        CamFlyVision.Zoom = Zoom;
        CamFlyVision.Distortion = 1;
    }
    /// <summary>
    ///Active Fly Vision Fx. Add a Fly Vision effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    public static void FlyVision_ON(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFlyVision != null) return;
        if (FlyVision_Switch) return;
        CamFlyVision = CurrentCamera.gameObject.AddComponent<CameraPlay_FlyVision>() as CameraPlay_FlyVision;
        if (CamFlyVision.CamTurnOff) return;
        FlyVision_Switch = true;
        CamFlyVision.Duration = Time;
        CamFlyVision.Zoom = 0.2f;
        CamFlyVision.Distortion = 1;
    }
    /// <summary>
    /// Active Fly Vision Fx. Add a Fly Vision effect to the current camera.
    /// </summary>
    public static void FlyVision_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFlyVision != null) return;
        if (FlyVision_Switch) return;
        CamFlyVision = CurrentCamera.gameObject.AddComponent<CameraPlay_FlyVision>() as CameraPlay_FlyVision;
        if (CamFlyVision.CamTurnOff) return;
        FlyVision_Switch = true;
        CamFlyVision.Zoom = 0.2f;
        CamFlyVision.Distortion = 1;
        CamFlyVision.Duration = 1;
    }
    /// <summary>
    /// Turn off the Fly Vision Fx.
    /// </summary>
    public static void FlyVision_OFF()
    {
        if (CamFlyVision == null) return;
        if (!FlyVision_Switch) return;
        CamFlyVision.Duration = 1;
        CamFlyVision.flashy = 1;
        CamFlyVision.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off the Fly Vision Fx.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    public static void FlyVision_OFF(float Time)
    {
        if (CamFlyVision == null) return;
        if (!FlyVision_Switch) return;
        CamFlyVision.Duration = Time;
        CamFlyVision.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off the Fly Vision Fx.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="Zoom">Set the zoom </param>
    /// <param name="distortion">Set the distorsion (1.0 = normal)</param>
    public static void FlyVision_OFF(float Time, float Zoom, float distortion)
    {
        if (CamFlyVision == null) return;
        if (!FlyVision_Switch) return;
        CamFlyVision.CamTurnOff = true;
        CamFlyVision.Duration = Time;
        CamFlyVision.Zoom = Zoom;
        CamFlyVision.Distortion = distortion;
    }
}