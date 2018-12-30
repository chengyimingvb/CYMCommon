////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    public static bool SniperScope_Switch = false;
    private static CameraPlay_SniperScope CamSniperScope;
    /// <summary>
    /// Active Snipe Scope FX. Add a Snipe Scope effect to the current camera.
    /// </summary>
	/// <param name="time">Set the time effect in second</param> 
    public static void SniperScope_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamSniperScope != null) return;
        if (SniperScope_Switch) return;
        CamSniperScope = CurrentCamera.gameObject.AddComponent<CameraPlay_SniperScope>() as CameraPlay_SniperScope;
        if (CamSniperScope.CamTurnOff) return;
        SniperScope_Switch = true;
        CamSniperScope.Duration = time;
    }
	/// <summary>
    /// Active Snipe Scope FX. Add a Snipe Scope effect to the current camera.
    /// </summary>
    public static void SniperScope_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamSniperScope != null) return;
        if (SniperScope_Switch) return;
        CamSniperScope = CurrentCamera.gameObject.AddComponent<CameraPlay_SniperScope>() as CameraPlay_SniperScope;
        if (CamSniperScope.CamTurnOff) return;
        SniperScope_Switch = true;
        CamSniperScope.Duration = 0.5f;
    }
	 /// <summary>
    /// Turn off Snipe Scope FX
    /// </summary>
   /// <param name="time">Set the time effect in second</param> 
   public static void SniperScope_OFF(float time)
    {
        if (CamSniperScope == null) return;
        if (!SniperScope_Switch) return;
        CamSniperScope.Duration = time;
        CamSniperScope.CamTurnOff = true;
    }
    /// <summary>
    /// Turn off Snipe Scope FX
    /// </summary>
    public static void SniperScope_OFF()
    {
        if (CamSniperScope == null) return;
        if (!SniperScope_Switch) return;
        CamSniperScope.Duration = 1;
        CamSniperScope.CamTurnOff = true;
    }
}