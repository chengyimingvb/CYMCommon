////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    /// <summary>
    /// Active Flash Light FX. Add a Flash Light effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    public static void FlashLight(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FlashLight CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FlashLight>() as CameraPlay_FlashLight;
        CP.Duration = Time;
    }
    /// <summary>
    /// Active Flash Light FX. Add a Flash Light effect to the current camera. By default, the timer apparition duration is set to 1 seconds.
    /// </summary>
    public static void FlashLight()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FlashLight CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FlashLight>() as CameraPlay_FlashLight;
        CP.Duration = 1;
    }
    /// <summary>
    /// Active Flash Light FX. Add a Flash Light effect to the current camera.
    /// </summary>
    /// <param name="color">Set the Color</param>
    /// <param name="Time">Set the time effect in second</param>
    public static void FlashLight(Color color, float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FlashLight CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FlashLight>() as CameraPlay_FlashLight;
        CP.Duration = Time;
        CP._ColorFade = color;
    }
    /// <summary>
    /// Active Flash Light FX. Add a Flash Light effect to the current camera.
    /// </summary>
    /// <param name="color">Set the Color</param>
    public static void FlashLight(Color color)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FlashLight CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FlashLight>() as CameraPlay_FlashLight;
        CP.Duration = 1;
        CP._ColorFade = color;
    }
}