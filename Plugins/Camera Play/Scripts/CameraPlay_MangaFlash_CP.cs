////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    /// <summary>
    /// Active Manga Flash FX. Add a Manga Flash effect to the current camera.
    /// </summary>
    /// <param name="Time">Set the time effect in second.</param>
    public static void MangaFlash(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_MangaFlash CP = CurrentCamera.gameObject.AddComponent<CameraPlay_MangaFlash>() as CameraPlay_MangaFlash;
        CP.Duration = Time;
    }
    /// <summary>
    /// Active Manga Flash FX. Add a Manga Flash effect to the current camera. By default, the timer apparition duration is set to 4 seconds.
    /// </summary>
    public static void MangaFlash()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_MangaFlash CP = CurrentCamera.gameObject.AddComponent<CameraPlay_MangaFlash>() as CameraPlay_MangaFlash;
        CP.PosX = 0.5f;
        CP.PosY = 0.5f;
        CP.Duration = 4;
    }
    /// <summary>
    /// Active Manga Flash FX. Add a Manga Flash effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the time effect in second.</param>
    /// <param name="SpeedFPS">Set the Speed Frame per second</param>
    /// <param name="color">Set the color</param>
    public static void MangaFlash(float sx, float sy, float Time, int SpeedFPS, Color color)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_MangaFlash CP = CurrentCamera.gameObject.AddComponent<CameraPlay_MangaFlash>() as CameraPlay_MangaFlash;
        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Speed = SpeedFPS;
        CP.Color = color;
    }
    /// <summary>
    /// Active Manga Flash FX. Add a Manga Flash effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the time effect in second.</param>
    public static void MangaFlash(float sx, float sy, float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_MangaFlash CP = CurrentCamera.gameObject.AddComponent<CameraPlay_MangaFlash>() as CameraPlay_MangaFlash;
        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
    }
    /// <summary>
    /// Active Manga Flash FX. Add a Manga Flash effect to the current camera.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    public static void MangaFlash(float sx, float sy)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_MangaFlash CP = CurrentCamera.gameObject.AddComponent<CameraPlay_MangaFlash>() as CameraPlay_MangaFlash;
        CP.Duration = 4;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Speed = 5;
        CP.Color = new Color(1, 1, 1, 1);
    }


}