////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    /// <summary>
    /// Active Radial FX. Add a Radial effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    public static void Radial(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Radial CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Radial>() as CameraPlay_Radial;
        CP.Duration = Time;
    }
    /// <summary>
    /// Active Radial FX. Add a Radial effect to the current camera. By default, the timer apparition duration is set to 4 seconds.
    /// </summary>
    public static void Radial()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Radial CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Radial>() as CameraPlay_Radial;
        CP.Duration = 4;
    }
    /// <summary>
    ///  Active Radial FX. Add a Radial effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the time effect in second.</param>
    /// <param name="size">Set the size</param>
    public static void Radial(float sx, float sy, float Time, float size)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Radial CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Radial>() as CameraPlay_Radial;
        CP.Duration = Time;
        CP.MovX = sx;
        CP.MovY = sy;
        CP.Size = size;
    }
}