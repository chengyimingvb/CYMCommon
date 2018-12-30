////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{

    public static bool Drunk_Switch = false;
    private static CameraPlay_Drunk CamDrunk;

    public enum Drunk_Preset
    {
        Default = 0,
        Drunk_A_Lot = 1,
        Drunk_Vodka = 2,
        Drunk_Poison = 3,
        Drunk_Yellow = 4,
        Drunk_Dark = 5,
        Drunk_Cold = 6,
        Drunk_Burn = 7,
        Drunk_Blood = 8,
        Drunk_To_Much = 9
    };

    /// <summary>
    /// Active Drunk FX. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    public static void Drunk_ON()
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamDrunk != null) return;
        if (Drunk_Switch) return;
        CamDrunk = CurrentCamera.gameObject.AddComponent<CameraPlay_Drunk>() as CameraPlay_Drunk;
        if (CamDrunk.CamTurnOff) return;
        Drunk_Switch = true;
        CamDrunk.Duration = 1;
    }
    /// <summary>
    /// Turn Off The Drunk FX.
    /// </summary>
    public static void Drunk_OFF()
    {
        if (CamDrunk == null) return;
        if (!Drunk_Switch) return;
        CamDrunk.Duration = 1;
        CamDrunk.CamTurnOff = true;
    }
    /// <summary>
    /// Turn Off The Drunk FX with a specific timer.
    /// </summary>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void Drunk_OFF(float time)
    {
        if (CamDrunk == null) return;
        if (!Drunk_Switch) return;
        CamDrunk.Duration = time;
        CamDrunk.CamTurnOff = true;
    }
    /// <summary>
    /// Active Drunk FX with specific Drunk FX using Drunk_Preset. By default, the timer apparition duration is set to 1 second.
    /// </summary>
    /// <param name="Preset">Use a specific Drunk using Drunk_Preset.</param>
    public static void Drunk_ON(Drunk_Preset Preset)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamDrunk != null) return;
        if (Drunk_Switch) return;
        CamDrunk = CurrentCamera.gameObject.AddComponent<CameraPlay_Drunk>() as CameraPlay_Drunk;
        if (CamDrunk.CamTurnOff) return;
        Drunk_Switch = true;
        CamDrunk.Duration = 1;
        CamDrunk.Preset = (CameraPlay_Drunk.Drunk_Preset)Preset;
    }
    /// <summary>
    /// Active Drunk FX with specific Drunk FX using Drunk_Preset and with a specific apparition timing. 
    /// </summary>
    /// <param name="Preset">Use a specific Drunk FX using Drunk_Preset.</param>
    /// <param name="time">Set the apparition time in sec.</param>
    public static void Drunk_ON(Drunk_Preset Preset, float time)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        if (CamDrunk != null) return;
        if (Drunk_Switch) return;
        CamDrunk = CurrentCamera.gameObject.AddComponent<CameraPlay_Drunk>() as CameraPlay_Drunk;
        if (CamDrunk.CamTurnOff) return;
        Drunk_Switch = true;
        CamDrunk.Duration = time;
        CamDrunk.Preset = (CameraPlay_Drunk.Drunk_Preset)Preset;
    }
}