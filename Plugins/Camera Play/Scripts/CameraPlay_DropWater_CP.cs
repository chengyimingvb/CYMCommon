////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
/// <summary>
    /// Add a Water Drop to the current camera, and remove it automatically after the animation is end. By default, the timer duration is set to 1 second.
    /// </summary>
     public static void DropWater()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_DropWater CP = CurrentCamera.gameObject.AddComponent<CameraPlay_DropWater>() as CameraPlay_DropWater;
        CP.PosX = Random.Range(0.1f, 0.9f);
        CP.PosY = Random.Range(0.1f, 0.9f);
        CP.Duration = 1;
    }
    /// <summary>
    /// Add a Water Drop to the current camera, and remove it automatically after the animation is end. By default, the timer duration is set to 1 second.
    /// </summary>
    /// <param name="x">The x position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="y">The y position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    public static void DropWater(float x, float y)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_DropWater CP = CurrentCamera.gameObject.AddComponent<CameraPlay_DropWater>() as CameraPlay_DropWater;
        CP.PosX = x;
        CP.PosY = y;
        CP.Duration = 1;
    }
    /// <summary>
    /// Add a Water Drop to the current camera, and remove it automatically after the animation is end.
    /// </summary>
    /// <param name="x">The x position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="y">The y position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="time">Time duration of the effect animation in second.</param>
    public static void DropWater(float x, float y, float time)
    {

        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_DropWater CP = CurrentCamera.gameObject.AddComponent<CameraPlay_DropWater>() as CameraPlay_DropWater;
        CP.PosX = x;
        CP.PosY = y;
        CP.Duration = time;
    }
    /// <summary>
    /// Add a Water Drop to the current camera, and remove it automatically after the animation is end.
    /// </summary>
    /// <param name="x">The x position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="y">The y position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="time">Time duration of the effect animation in second.</param>
    /// <param name="size">Size of the Distortion FX</param>
    public static void DropWater(float x, float y, float time, float size)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_DropWater CP = CurrentCamera.gameObject.AddComponent<CameraPlay_DropWater>() as CameraPlay_DropWater;
        CP.PosX = x;
        CP.PosY = y;
        CP.Duration = time;
        CP.Size = size;
    }

}