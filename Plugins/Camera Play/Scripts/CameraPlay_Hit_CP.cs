////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    /// <summary>
    /// Active Hit FX. Add a Hit effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    public static void Hit(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Hit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Hit>() as CameraPlay_Hit;
        CP.Duration = Time;
    }
    /// <summary>
    /// Active Hit FX. Add a Hit effect to the current camera. By default, the timer apparition duration is set to 3 seconds.
    /// </summary>
    public static void Hit()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Hit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Hit>() as CameraPlay_Hit;
        CP.Duration = 3;
    }
    /// <summary>
    /// Active Hit FX. Add a Hit effect to the current camera.
    /// </summary>
    /// <param name="col">Set Color</param>
    /// <param name="Time">Set the time effect in second.</param>
    public static void Hit(Color col, float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Hit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Hit>() as CameraPlay_Hit;
        CP.Duration = Time;
        CP.HitColor = col;
    }

    /// <summary>
    /// Active Hit FX. Add a Hit effect to the current camera.
    /// </summary>
    /// <param name="col">Set the Color</param>
    public static void Hit(Color col)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Hit CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Hit>() as CameraPlay_Hit;
        CP.Duration = 1;
        CP.HitColor = col;
    }

}