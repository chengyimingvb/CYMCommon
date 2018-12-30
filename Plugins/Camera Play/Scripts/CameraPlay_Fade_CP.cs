////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool Fade_Switch = false;
    private static CameraPlay_Fade CamFade;

    /// <summary>
    /// Active Fade FX. By default, the timer apparition duration is set to 1 second. 
    /// </summary>
    public static void Fade_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFade != null) return;
        if (Fade_Switch) return;
        CamFade = CurrentCamera.gameObject.AddComponent<CameraPlay_Fade>() as CameraPlay_Fade;
        if (CamFade.CamTurnOff) return;
        Fade_Switch = true;
        CamFade.Duration = 1;
    }
    /// <summary>
    /// Active Fade FX. 
    /// </summary>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void Fade_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFade != null) return;
        if (Fade_Switch) return;
        CamFade = CurrentCamera.gameObject.AddComponent<CameraPlay_Fade>() as CameraPlay_Fade;
        if (CamFade.CamTurnOff) return;
        Fade_Switch = true;
        CamFade.Duration = time;
    }
    /// <summary>
    /// Active Fade FX
    /// </summary>
    /// <param name="col">Set the Color</param>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void Fade_ON(Color col, float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFade != null) return;
        if (Fade_Switch) return;
        CamFade = CurrentCamera.gameObject.AddComponent<CameraPlay_Fade>() as CameraPlay_Fade;
        if (CamFade.CamTurnOff) return;
        Fade_Switch = true;
        CamFade.ColorFade = col;
        CamFade.Duration = time;
    }
    /// <summary>
    ///  Active Fade FX
    /// </summary>
    /// <param name="col">Set the Color</param>
    public static void Fade_ON(Color col)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamFade != null) return;
        if (Fade_Switch) return;
        CamFade = CurrentCamera.gameObject.AddComponent<CameraPlay_Fade>() as CameraPlay_Fade;
        if (CamFade.CamTurnOff) return;
        Fade_Switch = true;
        CamFade.ColorFade = col;
        CamFade.Duration = 1;
    }
    /// <summary>
    ///  Turn Off The Fade FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void Fade_OFF()
    {
        if (CamFade == null) return;
        if (!Fade_Switch) return;
        CamFade.Duration = 1;
        CamFade.CamTurnOff = true;
    }
    /// <summary>
    /// Turn Off The Fade FX.
    /// </summary>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void Fade_OFF(float time)
    {
        if (CamFade == null) return;
        if (!Fade_Switch) return;
        CamFade.Duration = time;
        CamFade.CamTurnOff = true;
    }

}