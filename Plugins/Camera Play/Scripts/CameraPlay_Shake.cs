////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class CameraPlay_Shake : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private float TimeX = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    [HideInInspector] public float Value = 0.5f;
    [HideInInspector] public float Size = 1f;
    [HideInInspector] public float Duration = 1f;
    [HideInInspector] private float Timer = 1f;
    [HideInInspector] public float Speed = 15f;

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
        SCShader = Shader.Find("CameraPlay/Shake");
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
    }

    // Start Animation
    void OnEnable()
    {
        Value = 0;
        Timer = 0;
    }


    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        if (SCShader != null)
        {
            TimeX += Time.deltaTime;
            if (TimeX > 100) TimeX = 0;
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Value", Speed);
            material.SetFloat("_Value2", Size * 0.008f);
            material.SetFloat("_Value3", Size * 0.008f);
            material.SetVector("_ScreenResolution", new Vector4(sourceTexture.width, sourceTexture.height, 0.0f, 0.0f));
            Graphics.Blit(sourceTexture, destTexture, material);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
        }
    }
    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying != true)
        {
            SCShader = Shader.Find("CameraPlay/Shake");
        }
#endif

        Timer += Time.deltaTime * (1 / Duration);
        if (Timer > 1f) Object.Destroy(this);
    }
    void OnDisable()
    {
        if (SCMaterial)
        {
            DestroyImmediate(SCMaterial);
        }
    }
}