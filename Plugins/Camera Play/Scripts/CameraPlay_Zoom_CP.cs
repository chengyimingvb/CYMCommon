////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{


    /// <summary>
    /// Active Zoom FX. Add a Zoom effect to the current camera. By default, the distortion is set at 1 ( 0 to 1 ).
    /// </summary>
    /// <param name="Time">Set the time effect in second. </param>
    public static void Zoom(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Zoom CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Zoom>() as CameraPlay_Zoom;
        CP.Duration = Time;
        CP.PosX = 0.5f;
        CP.PosY = 0.5f;
        CP.Zoom = 1;
    }
    /// <summary>
    /// Active Zoom FX. Add a Zoom effect to the current camera. By default, the timer apparition duration is set to 4 seconds and the Pitch Distortion to 1.
    /// </summary>
    public static void Zoom()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Zoom CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Zoom>() as CameraPlay_Zoom;
        CP.Duration = 4;
        CP.PosX = 0.5f;
        CP.PosY = 0.5f;
        CP.Zoom = 1;
    }
    /// <summary>
    /// Active Zoom FX. Add a Zoom effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void Zoom(float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Zoom CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Zoom>() as CameraPlay_Zoom;
        CP.Duration = Time;
        CP.PosX = 0.5f;
        CP.PosY = 0.5f;
        CP.Zoom = dist;
    }
    /// <summary>
    /// Active Zoom FX. Add a Zoom effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the time effect in second.</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void Zoom(float sx, float sy, float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Zoom CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Zoom>() as CameraPlay_Zoom;
        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Zoom = dist;
    }
    /// <summary>
    /// Active Zoom FX. Add a Zoom effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the time effect in second.</param>
    public static void Zoom(float sx, float sy, float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Zoom CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Zoom>() as CameraPlay_Zoom;
        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Zoom = 1;
    }
}