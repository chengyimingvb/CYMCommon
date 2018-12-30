////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool WidescreenH_Switch = false;
    private static CameraPlay_WidescreenH CamWidescreenH;
    /// <summary>
    /// Active Wide Screen FX. Add Wide Screen effect to the current camera.
    /// </summary>
    public static void WidescreenH_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamWidescreenH != null) return;
        if (WidescreenH_Switch) return;
        CamWidescreenH = CurrentCamera.gameObject.AddComponent<CameraPlay_WidescreenH>() as CameraPlay_WidescreenH;
        if (CamWidescreenH.CamTurnOff) return;
        WidescreenH_Switch = true;
        CamWidescreenH.Duration = 1;
    }
    /// <summary>
    /// Active Wide Screen FX. Add Wide Screen effect to the current camera.
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void WidescreenH_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamWidescreenH != null) return;
        if (WidescreenH_Switch) return;
        CamWidescreenH = CurrentCamera.gameObject.AddComponent<CameraPlay_WidescreenH>() as CameraPlay_WidescreenH;
        if (CamWidescreenH.CamTurnOff) return;
        WidescreenH_Switch = true;
        CamWidescreenH.Duration = time;
    }
    /// <summary>
    /// Active Wide Screen FX. Add Wide Screen effect to the current camera.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="time">Set the time effect in second.</param>
    public static void WidescreenH_ON(Color col, float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamWidescreenH != null) return;
        if (WidescreenH_Switch) return;
        CamWidescreenH = CurrentCamera.gameObject.AddComponent<CameraPlay_WidescreenH>() as CameraPlay_WidescreenH;
        if (CamWidescreenH.CamTurnOff) return;
        WidescreenH_Switch = true;
        CamWidescreenH.ColorFade = col;
        CamWidescreenH.Duration = time;
    }
    /// <summary>
    /// Active Wide Screen FX. Add Wide Screen effect to the current camera.
    /// </summary>
    /// <param name="col">Set the color of the wide screen</param>
    public static void WidescreenH_ON(Color col)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamWidescreenH != null) return;
        if (WidescreenH_Switch) return;
        CamWidescreenH = CurrentCamera.gameObject.AddComponent<CameraPlay_WidescreenH>() as CameraPlay_WidescreenH;
        if (CamWidescreenH.CamTurnOff) return;
        WidescreenH_Switch = true;
        CamWidescreenH.ColorFade = col;
        CamWidescreenH.Duration = 1;
    }
    /// <summary>
    /// Turn off Wide Screen Fx
    /// </summary>
    public static void WidescreenH_OFF()
    {
        if (CamWidescreenH == null) return;
        if (!WidescreenH_Switch) return;
        CamWidescreenH.Duration = 1;
        CamWidescreenH.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off Wide Screen Fx
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void WidescreenH_OFF(float time)
    {
        if (CamWidescreenH == null) return;
        if (!WidescreenH_Switch) return;
        CamWidescreenH.Duration = time;
        CamWidescreenH.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off Wide Screen Fx
    /// </summary>
    /// <param name="col">Set the color of the wide screen</param>
    public static void WidescreenH_OFF(Color col)
    {
        if (CamWidescreenH == null) return;
        if (!WidescreenH_Switch) return;
        CamWidescreenH.Duration = 1;
        CamWidescreenH.ColorFade = col;
        CamWidescreenH.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off Wide Screen Fx
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    /// <param name="col">Set the color of the wide screen</param>
    public static void WidescreenH_OFF(Color col, float time)
    {
        if (CamWidescreenH == null) return;
        if (!WidescreenH_Switch) return;
        CamWidescreenH.Duration = time;
        CamWidescreenH.ColorFade = col;
        CamWidescreenH.CamTurnOff = true;
    }
}