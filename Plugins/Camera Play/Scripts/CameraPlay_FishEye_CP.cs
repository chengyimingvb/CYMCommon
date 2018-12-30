////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

 
   /// <summary>
    /// Active Fish Eye FX. Add a Fish Eye effect to the current camera. By default, the distortion is set at 1.
    /// </summary>
    /// <param name="Time">Set the time effect in second. </param>
    public static void FishEye(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FishEye CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FishEye>() as CameraPlay_FishEye;
        CP.Duration = Time;
        CP.PosX = 0.5f;
        CP.PosY = 0.5f;
        CP.Distortion = 1;
    }
    /// <summary>
    /// Active Fish Eye FX. Add a Fish Eye effect to the current camera. By default, the timer apparition duration is set to 4 seconds and the Pitch Distortion to 1.
    /// </summary>
    public static void FishEye()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FishEye CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FishEye>() as CameraPlay_FishEye;
        CP.Duration = 4;
        CP.PosX = 0.5f;
        CP.PosY = 0.5f;
        CP.Distortion = 1;
    }
    /// <summary>
    /// Active Fish Eye FX. Add a Fish Eye effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void FishEye(float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FishEye CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FishEye>() as CameraPlay_FishEye;
        CP.Duration = Time;
        CP.Duration = 4;
        CP.PosX = 0.5f;
        CP.PosY = 0.5f;
        CP.Distortion = dist;
    }
    /// <summary>
    /// Active Fish Eye FX. Add a Fish Eye effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void FishEye(float sx, float sy, float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FishEye CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FishEye>() as CameraPlay_FishEye;

        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Distortion = dist;
    }
    /// <summary>
    /// Active Fish Eye FX. Add a Fish Eye effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the time effect in second</param>
    public static void FishEye(float sx, float sy, float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FishEye CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FishEye>() as CameraPlay_FishEye;
        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Distortion = 1;
    }
}