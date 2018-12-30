////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class CameraPlay_SniperScope : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private float TimeX = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;

    [HideInInspector] public float Fade = 1f;
    [HideInInspector] public float Size = 0.45f;
    [HideInInspector] public float Smooth = 0.1f;
    [HideInInspector] public float _Cible = 0.5f;
    [HideInInspector] public float _Distortion = 0.5f;
    [HideInInspector] public float _ExtraColor = 0.5f;
    [HideInInspector] public float _ExtraLight = 0.35f;
    [HideInInspector] public Color _Tint = new Color(0, 0.6f, 0, 0.25f);
    [HideInInspector] private float StretchX = 1f;
    [HideInInspector] private float StretchY = 1f;
    [HideInInspector] public float _PosX = 0f;
    [HideInInspector] public float _PosY = 0f;
    [HideInInspector] public float _BlackFade = 1f;

    [HideInInspector] public bool CamTurnOff = false;
    [HideInInspector] public float flashy = 0.0f;
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
    void OnEnable()
    {
        Timer = 0;
    }

    void Start()
    {

        SCShader = Shader.Find("CameraPlay/SniperScope");
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
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, 0.5f);
            curve.AddKey(0.50f, 0.75f);
            curve.AddKey(0.75f, 1.01f);
            curve.AddKey(1, 1);
            float fresult = curve.Evaluate(flashy);
            material.SetFloat("_Fade", fresult);
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Value", Size);
            material.SetFloat("_Value2", Smooth);
            material.SetFloat("_Value3", StretchX);
            material.SetFloat("_Value4", StretchY);
            material.SetFloat("_Cible", _Cible);
            material.SetFloat("_ExtraColor", _ExtraColor);
            material.SetFloat("_Distortion", _Distortion);
            material.SetFloat("_PosX", _PosX);
            material.SetFloat("_PosY", _PosY);
            material.SetColor("_Tint", _Tint);
            material.SetFloat("_BlackFade", _BlackFade);
            material.SetFloat("_ExtraLight", _ExtraLight);
            Vector2 Scr = new Vector2(Screen.width, Screen.height);
            material.SetVector("_ScreenResolution", new Vector4(Scr.x, Scr.y, Scr.y / Scr.x, 0));

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
            SCShader = Shader.Find("CameraPlay/SniperScope");
        }
#endif

        if (CamTurnOff == false)
        {
            Timer += Time.deltaTime * (1 / Duration);
            if (Timer > 1f) Timer = 1;
            flashy = Timer;
        }

        if (CamTurnOff)
        {
            Timer -= Time.deltaTime * (1 / Duration);
            flashy = Timer;
            if (Timer < 0)
            {
                CameraPlay.SniperScope_Switch = false;
                Object.Destroy(this);
            }
        }
    }
    void OnDisable()
    {
        if (SCMaterial)
        {
            DestroyImmediate(SCMaterial);
        }
    }
}
