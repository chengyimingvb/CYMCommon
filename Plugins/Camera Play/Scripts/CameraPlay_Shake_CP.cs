////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    /// <summary>
    /// Active Shake FX. Add a Shake effect to the current camera.
    /// </summary>
    /// <param name="duration">Set the time effect in second.</param>
    /// <param name="Speed">Set the animation speed</param>
    /// <param name="Size">Set the size</param>
    public static void EarthQuakeShake(float duration, float Speed, float Size)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Shake CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Shake>() as CameraPlay_Shake;
        CP.Duration = duration;
        CP.Speed = Speed;
        CP.Size = Size;
    }
    /// <summary>
    /// Active Shake FX. Add a Shake effect to the current camera.
    /// </summary>
    /// <param name="duration">Set the time effect in second.</param>
    /// <param name="Speed">Set the animation speed</param>
    public static void EarthQuakeShake(float duration, float Speed)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Shake CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Shake>() as CameraPlay_Shake;
        CP.Duration = duration;
        CP.Speed = Speed;
        CP.Size = 2;
    }
    /// <summary>
    /// Active Shake FX. Add a Shake effect to the current camera.
    /// </summary>
    /// <param name="duration">Set the time effect in second.</param>
    public static void EarthQuakeShake(float duration)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Shake CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Shake>() as CameraPlay_Shake;
        CP.Duration = duration;
        CP.Speed = 15;
        CP.Size = 2;
    }
    /// <summary>
    /// Active Shake FX. Add a Shake effect to the current camera. By Default the time is set to 1 second
    /// </summary>
    public static void EarthQuakeShake()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Shake CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Shake>() as CameraPlay_Shake;
        CP.Duration = 1;
        CP.Speed = 15;
        CP.Size = 2;
    }
}