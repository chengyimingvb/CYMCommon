////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool Colored_Switch = false;
    private static CameraPlay_Colored CamColored;

    /// <summary>
    /// Active Colored FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void Colored_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamColored != null) return;
        if (Colored_Switch) return;
        CamColored = CurrentCamera.gameObject.AddComponent<CameraPlay_Colored>() as CameraPlay_Colored;
        if (CamColored.CamTurnOff) return;
        Colored_Switch = true;
		 CamColored.Colored = Color.red;
        CamColored.Duration = 1;
    }
    /// <summary>
    /// Active Colored FX.
    /// </summary>
    /// <param name="time">Set the apparition time in sec</param>
    public static void Colored_ON(float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamColored != null) return;
        if (Colored_Switch) return;
        CamColored = CurrentCamera.gameObject.AddComponent<CameraPlay_Colored>() as CameraPlay_Colored;
        if (CamColored.CamTurnOff) return;
        Colored_Switch = true;
    	 CamColored.Colored = Color.red;
         CamColored.Duration = time;
    }
    /// <summary>
    /// Active Colored FX.
    /// </summary>
    /// <param name="col">Set the color</param>
    /// <param name="time">Set the apparition time in sec</param>
    public static void Colored_ON(Color col, float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamColored != null) return;
        if (Colored_Switch) return;
        CamColored = CurrentCamera.gameObject.AddComponent<CameraPlay_Colored>() as CameraPlay_Colored;
        if (CamColored.CamTurnOff) return;
        Colored_Switch = true;
        CamColored.Colored = col;
        CamColored.Duration = time;
    }
    /// <summary>
    /// Active Colored FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    /// <param name="col">Set the color</param>
    public static void Colored_ON(Color col)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamColored != null) return;
        if (Colored_Switch) return;
        CamColored = CurrentCamera.gameObject.AddComponent<CameraPlay_Colored>() as CameraPlay_Colored;
        if (CamColored.CamTurnOff) return;
        Colored_Switch = true;
        CamColored.Colored = col;
        CamColored.Duration = 1;
    }
    /// <summary>
    ///  Turn Off Colored FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void Colored_OFF()
    {
        if (CamColored == null) return;
        if (!Colored_Switch) return;
        CamColored.Duration = 1;
    	 CamColored.Colored = Color.red;
         CamColored.CamTurnOff = true;
    }
    /// <summary>
    /// Turn Off Colored FX.
    /// </summary>
    /// <param name="time">Set the apparition time in sec</param>
    public static void Colored_OFF(float time)
    {
        if (CamColored == null) return;
        if (!Colored_Switch) return;
        CamColored.Duration = time;
 	 CamColored.Colored = Color.red;
            CamColored.CamTurnOff = true;
    }
/// <summary>
    /// Active Colored FX.
    /// </summary>
    /// <param name="col">Set the color</param>
    /// <param name="time">Set the apparition time in sec</param>
    public static void Colored_OFF(Color col, float time)
    {
         if (CamColored == null) return;
        if (!Colored_Switch) return;
        CamColored.Duration = time;
        CamColored.CamTurnOff = true;
        CamColored.Colored = col;
        
    }
}