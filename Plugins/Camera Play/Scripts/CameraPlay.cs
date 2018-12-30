////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public partial class CameraPlay : MonoBehaviour
{
    public static Camera CurrentCamera;

    void Awake()
    {
        CurrentCamera = Camera.main;
    }

    void Update()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Pos"></param>
    /// <returns></returns>
    public static float PosScreenX(Vector3 Pos)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        Pos = CurrentCamera.WorldToScreenPoint(Pos);
        float x = Pos.x / Screen.width;
        return x;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Pos"></param>
    /// <returns></returns>
    public static float PosScreenY(Vector3 Pos)
    {
        if (CurrentCamera == null) CurrentCamera = Camera.main;
        Pos = CurrentCamera.WorldToScreenPoint(Pos);
        float y = Pos.y / Screen.height;
        return y;
    }
}
