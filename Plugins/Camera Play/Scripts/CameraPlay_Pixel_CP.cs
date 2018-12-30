////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool Pixel_Switch = false;
    private static CameraPlay_Pixel CamPixel;
    /// <summary>
    /// Active Pixel FX. Add a Pixel effect to the current camera.
    /// </summary>
    public static void Pixel_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamPixel != null) return;
        if (Pixel_Switch) return;
        CamPixel = CurrentCamera.gameObject.AddComponent<CameraPlay_Pixel>() as CameraPlay_Pixel;
        if (CamPixel.CamTurnOff) return;
        Pixel_Switch = true;
        CamPixel.PixelSize = 4;
        CamPixel.Duration = 1;
    }
    /// <summary>
    /// Active Pixel FX. Add a Pixel effect to the current camera.
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void Pixel_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamPixel != null) return;
        if (Pixel_Switch) return;
        CamPixel = CurrentCamera.gameObject.AddComponent<CameraPlay_Pixel>() as CameraPlay_Pixel;
        if (CamPixel.CamTurnOff) return;
        Pixel_Switch = true;
        CamPixel.PixelSize = 4;
        CamPixel.Duration = time;
    }

    /// <summary>
    /// Active Pixel FX. Add a Pixel effect to the current camera
    /// </summary>
    /// <param name="size">Set the size</param>
    /// <param name="time">Set the time effect in second.</param>
    public static void Pixel_ON(float size, float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamPixel != null) return;
        if (Pixel_Switch) return;
        CamPixel = CurrentCamera.gameObject.AddComponent<CameraPlay_Pixel>() as CameraPlay_Pixel;
        if (CamPixel.CamTurnOff) return;
        Pixel_Switch = true;
        CamPixel.PixelSize = size;
        CamPixel.Duration = time;
    }

    /// <summary>
    /// Turn off the Pixel Fx.
    /// </summary>
    public static void Pixel_OFF()
    {
        if (CamPixel == null) return;
        if (!Pixel_Switch) return;
        CamPixel.Duration = 1;
        CamPixel.CamTurnOff = true;
    }

    /// <summary>
    /// Turn off the Pixel Fx.
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void Pixel_OFF(float time)
    {
        if (CamPixel == null) return;
        if (!Pixel_Switch) return;
        CamPixel.Duration = time;
        CamPixel.CamTurnOff = true;
    }

}