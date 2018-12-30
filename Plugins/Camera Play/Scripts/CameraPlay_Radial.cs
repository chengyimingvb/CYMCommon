////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class CameraPlay_Radial : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private float TimeX = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    [HideInInspector]public float Intensity = 0.125f;
    [HideInInspector]public float MovX = 0.5f;
    [HideInInspector]public float MovY = 0.5f;
    [HideInInspector]public float Size = 1f;
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

        SCShader = Shader.Find("CameraPlay/Radial");
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
            Timer += Time.deltaTime * (1 / Duration);
            if (Timer > 1f) Object.Destroy(this);
            Intensity = Timer;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, 0.6f);
            curve.AddKey(0.50f, 1f);
            curve.AddKey(0.75f, 0.6f);
            curve.AddKey(1, 0);
            float fresult = curve.Evaluate(Intensity) * Size;
            if (fresult > 1) fresult = 1;
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Value", fresult);
            material.SetFloat("_Value2", MovX);
            material.SetFloat("_Value3", MovY);


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
            SCShader = Shader.Find("CameraPlay/Radial");
        }
#endif
    }
    void OnEnable()
    {
        Intensity = 0;
        Timer = 0;
    }

    void OnDisable()
    {
        if (SCMaterial)
        {
            DestroyImmediate(SCMaterial);
        }
    }
}
