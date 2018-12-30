////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraPlay_Zoom : MonoBehaviour
{
    #region Variables
    private string ShaderName = "CameraPlay/Zoom";


    public Shader SCShader;
    private float TimeX = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;

    [HideInInspector] public bool CamTurnOff = false;
    [HideInInspector] public float flashy = 0.0f;
    [HideInInspector] public float PosX = 0.0f;
    [HideInInspector] public float PosY = 0.0f;
    [HideInInspector] public float Duration = 1f;
    [HideInInspector] public float Zoom = 1f;
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
        SCShader = Shader.Find(ShaderName);
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
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, 0.6f);
            curve.AddKey(0.50f, 1f);
            curve.AddKey(0.75f, 0.6f);
            curve.AddKey(1, 0);
            float fresult = curve.Evaluate(Timer) * Zoom;
            if (fresult > 1) fresult = 1;
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Fade", fresult);
            material.SetFloat("_PosX", PosX);
            material.SetFloat("_PosY", PosY);
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
            SCShader = Shader.Find(ShaderName);
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