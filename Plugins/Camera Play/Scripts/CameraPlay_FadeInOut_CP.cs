////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    /// <summary>
    /// Fade In Out FX. Add a Fade In Out effect to the current camera.
    /// </summary>
    /// <param name="time">Set the time effect in second</param>
    public static void FadeInOut(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FadeInOut CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FadeInOut>() as CameraPlay_FadeInOut;
        CP.Duration = time;
    }
    /// <summary>
    /// Fade In Out FX. Add a Fade In Out effect to the current camera.
    /// </summary>
    /// <param name="color">Set the color of the fading effect</param>
    /// <param name="Time">Set the time effect in second</param>
    public static void FadeInOut(Color color, float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FadeInOut CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FadeInOut>() as CameraPlay_FadeInOut;
        CP.Duration = time;
        CP.ColorFade = color;
    }
    /// <summary>
    /// Fade In Out FX. Add a Fade In Out effect to the current camera. By default, the timer apparition duration is set to 3 seconds
    /// </summary>
    /// <param name="color">Set the color of the fading effect</param>
    public static void FadeInOut(Color color)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FadeInOut CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FadeInOut>() as CameraPlay_FadeInOut;
        CP.Duration = 3;
        CP.ColorFade = color;
    }
    /// <summary>
    /// Fade In Out. By default, the timer apparition duration is set to 3 seconds
    /// </summary>
    public static void FadeInOut()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_FadeInOut CP = CurrentCamera.gameObject.AddComponent<CameraPlay_FadeInOut>() as CameraPlay_FadeInOut;
        CP.ColorFade = Color.black;
        CP.Duration = 3;
    }

}