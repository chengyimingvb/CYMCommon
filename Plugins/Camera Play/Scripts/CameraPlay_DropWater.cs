////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class CameraPlay_DropWater : MonoBehaviour
{
    #region Variables
    [HideInInspector] public Shader SCShader;
    private float TimeX = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    [HideInInspector] public float PosX = 0.5f;
    [HideInInspector] public float PosY = 0.5f;
    [HideInInspector] public float Value = 0.5f;
    [HideInInspector] public float Size = 1f;
    [HideInInspector] public float Duration = 1f;
    [HideInInspector] private float Timer = 1f;

    #endregion
    #region Properties
    Material material
    {
        get
        {
            if (SCMaterial == null)
            {
                SCMaterial = new Material(SCShader);
                SCMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return SCMaterial;
        }
    }
    #endregion
    void Start()
    {
        Value = 0;
        Timer = 0;
        SCShader = Shader.Find("CameraPlay/ShockWave");
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        if (SCShader != null)
        {
            TimeX += Time.deltaTime;
            if (TimeX > 100) TimeX = 0;
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Value", PosX);
            material.SetFloat("_Value2", PosY);
            material.SetFloat("_Value3", Value);
            material.SetFloat("_Value4", Size);
            material.SetVector("_ScreenResolution", new Vector4(sourceTexture.width, sourceTexture.height, 0.0f, 0.0f));
            Graphics.Blit(sourceTexture, destTexture, material);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
        }
    }

    // Start Animation
    void OnEnable()
    {
        Value = 0;
        Timer = 0;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying != true)
        {
            SCShader = Shader.Find("CameraPlay/ShockWave");
        }
#endif

        Timer += Time.deltaTime * (1 / Duration);
        if (Timer > 1.5f) Object.Destroy(this);
        Value = Timer;

    }
    void OnDisable()
    {
        if (SCMaterial)
        {
            DestroyImmediate(SCMaterial);
        }
    }
}
