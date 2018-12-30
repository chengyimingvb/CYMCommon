////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    /// <summary>
    /// Active BulletHole FX. By default, the distortion is set at 1.
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    public static void BulletHole(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BulletHole CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BulletHole>() as CameraPlay_BulletHole;
        CP.Duration = Time;
        CP.PosX = Random.Range(0.1f, 0.9f);
        CP.PosY = Random.Range(0.1f, 0.9f);
        CP.Distortion = 1;
    }
    /// <summary>
    /// Active BulletHole FX. By default, the timer apparition duration is set to 4 seconds and the Pitch Distortion to 1.
    /// </summary>
    public static void BulletHole()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BulletHole CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BulletHole>() as CameraPlay_BulletHole;
        CP.Duration = 4;
        CP.PosX = Random.Range(0.1f, 0.9f);
        CP.PosY = Random.Range(0.1f, 0.9f);
        CP.Distortion = 1;
    }
    /// <summary>
    /// Active BulletHole FX. By default, the position is random
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void BulletHole(float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BulletHole CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BulletHole>() as CameraPlay_BulletHole;
        CP.Duration = Time;
        CP.PosX = Random.Range(0.1f, 0.9f);
        CP.PosY = Random.Range(0.1f, 0.9f);
        CP.Distortion = dist;
    }
    /// <summary>
    /// Active BulletHole FX.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the apparition time in sec</param>
    /// <param name="dist">Set the distorsion (1.0 = normal)</param>
    public static void BulletHole(float sx, float sy, float Time, float dist)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BulletHole CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BulletHole>() as CameraPlay_BulletHole;
        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Distortion = dist;
    }
    /// <summary>
    /// Active BulletHole FX.
    /// </summary>
    /// <param name="sx">The sx screen position of the effect. 0 = left side of the screen. 1 = Right side of the screen. 0.5f = center of the screen</param>
    /// <param name="sy">The sy position of the effect. 0 = up side of the screen. 1 = down side of the screen. 0.5f = center of the screen</param>
    /// <param name="Time">Set the apparition time in sec</param>
    public static void BulletHole(float sx, float sy, float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_BulletHole CP = CurrentCamera.gameObject.AddComponent<CameraPlay_BulletHole>() as CameraPlay_BulletHole;
        CP.Duration = Time;
        CP.PosX = sx;
        CP.PosY = sy;
        CP.Distortion = 1;
    }
}