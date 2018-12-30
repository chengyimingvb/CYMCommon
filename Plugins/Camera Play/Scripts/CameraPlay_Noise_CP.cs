////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    /// <summary>
    /// Active Noise FX. Add a Noise effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    public static void Noise(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Noise CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Noise>() as CameraPlay_Noise;
        CP.Duration = Time;
    }
    /// <summary>
    /// Active Noise FX. Add a Noise effect to the current camera. By default, the timer apparition duration is set to 3 seconds.
    /// </summary>
    public static void Noise()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Noise CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Noise>() as CameraPlay_Noise;
        CP.Duration = 3;
    }

}