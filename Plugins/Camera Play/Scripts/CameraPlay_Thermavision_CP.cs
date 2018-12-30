////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool Thermavision_Switch = false;
    private static CameraPlay_Thermavision CamThermavision;
    /// <summary>
    /// Active Thermal Vision FX. Add Thermal Vision effect to the current camera.
    /// </summary>
    public static void Thermavision_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamThermavision != null) return;
        if (Thermavision_Switch) return;
        CamThermavision = CurrentCamera.gameObject.AddComponent<CameraPlay_Thermavision>() as CameraPlay_Thermavision;
        if (CamThermavision.CamTurnOff) return;
        Thermavision_Switch = true;
        CamThermavision.Duration = 1;
    }
    /// <summary>
    /// Active Thermal Vision FX. Add Thermal Vision effect to the current camera.
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void Thermavision_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamThermavision != null) return;
        if (Thermavision_Switch) return;
        CamThermavision = CurrentCamera.gameObject.AddComponent<CameraPlay_Thermavision>() as CameraPlay_Thermavision;
        if (CamThermavision.CamTurnOff) return;
        Thermavision_Switch = true;
        CamThermavision.Duration = time;
    }
    /// <summary>
    /// Turn off Thermal Vision Fx
    /// </summary>
    public static void Thermavision_OFF()
    {
        if (CamThermavision == null) return;
        if (!Thermavision_Switch) return;
        CamThermavision.Duration = 1;
        CamThermavision.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off Thermal Vision Fx
    /// </summary>
    /// <param name="time">Set the time effect in second.</param>
    public static void Thermavision_OFF(float time)
    {
        if (CamThermavision == null) return;
        if (!Thermavision_Switch) return;
        CamThermavision.Duration = time;
        CamThermavision.CamTurnOff = true;
    }

}