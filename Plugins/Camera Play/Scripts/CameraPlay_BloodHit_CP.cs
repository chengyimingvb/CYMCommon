////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    /// <summary>
    /// Active Blood Hit FX. Add a Blood Hit effect to the current camera. By default, the distortion is set at 1.
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    public static void BloodHit(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BloodHit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BloodHit>() as CameraPlay_BloodHit;
        CP.Duration = Time;
        CP.Distortion = 1;
        CP.color = Color.red;
    }
    /// <summary>
    /// Active Blood Hit. Add a Blood Hit to the current camera. By default, the timer apparition duration is set to 4 seconds and the Pitch Distortion to 1.
    /// </summary>
    public static void BloodHit()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BloodHit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BloodHit>() as CameraPlay_BloodHit;
        CP.Duration = 4;
        CP.Distortion = 1;
        CP.color = Color.red;
    }
    /// <summary>
    /// Active Blood Hit. Add a Blood Hit to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void BloodHit(float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BloodHit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BloodHit>() as CameraPlay_BloodHit;
        CP.Duration = Time;
        CP.Distortion = dist;
        CP.color = Color.red;
    }
    /// <summary>
    /// Active Blood Hit. Add a Blood Hit to the current camera.
    /// </summary>
    /// <param name="color">Set the color effect.</param>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void BloodHit(Color color, float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BloodHit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BloodHit>() as CameraPlay_BloodHit;
        CP.Duration = Time;
        CP.Distortion = dist;
        CP.color = color;
    }
}