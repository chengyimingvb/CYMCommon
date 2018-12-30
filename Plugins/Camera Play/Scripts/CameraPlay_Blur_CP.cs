////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    /// <summary>
    /// Active Blur FX. Add a Blur effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    public static void Blur(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Blur CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Blur>() as CameraPlay_Blur;
        CP.Duration = Time;
    }

    /// <summary>
    /// Active Blur FX. By default, the timer apparition duration is set to 3 seconds
    /// </summary>
    public static void Blur()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Blur CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Blur>() as CameraPlay_Blur;
        CP.Duration = 3;
    }

}