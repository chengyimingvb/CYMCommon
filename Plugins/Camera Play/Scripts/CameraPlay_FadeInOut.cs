////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraPlay_FadeInOut : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private float TimeX = 1.0f;
   [Range(1, 16)] public float Size = 10;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    #endregion

    [HideInInspector] public float _Fade = 1.0f;
    [HideInInspector] public Color ColorFade = new Color(0, 0, 0, 1);
    [HideInInspector] public float flashy = 0.0f;
    [HideInInspector] public float Duration = 1f;
    [HideInInspector] private float Timer = 1f;


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

        SCShader = Shader.Find("CameraPlay/FadeInOut");

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
            Size = Timer;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, 0.5f);
            curve.AddKey(0.45f, 1f);
            curve.AddKey(0.50f, 1f);
            curve.AddKey(0.55f, 1f);
            curve.AddKey(0.75f, 0.5f);
            curve.AddKey(1, 0);
            float fresult = curve.Evaluate(Size);
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Fade", fresult);
            material.SetColor("_ColorFade", ColorFade);
            material.SetVector("_ScreenResolution", new Vector2(Screen.width, Screen.height));
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
            SCShader = Shader.Find("CameraPlay/Blur");
        }
    #endif

    }
    void OnEnable()
    {
        Size = 0;
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