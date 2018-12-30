////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    /// <summary>
    /// Active Chromatical FX
    /// </summary>
    /// <param name="Time">Set the time effect in second</param>
    public static void Chromatical(float Time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Chromatical CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Chromatical>() as CameraPlay_Chromatical;
        CP.Duration = Time;
    }
    /// <summary>
    /// Active Chromatical FX. By default, the timer apparition duration is set to 3 seconds.
    /// </summary>
    public static void Chromatical()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        CameraPlay_Chromatical CP = CurrentCamera.gameObject.AddComponent<CameraPlay_Chromatical>() as CameraPlay_Chromatical;
        CP.Duration = 3;
    }

}