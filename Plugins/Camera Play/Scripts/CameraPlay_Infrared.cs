////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraPlay_Infrared : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private string ShaderName = "CameraPlay/Infrared";


    private float TimeX = 1.0f;
    [HideInInspector] public float _Fade = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;

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
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, 0.5f);
            curve.AddKey(0.50f, 0.75f);
            curve.AddKey(0.75f, 1.01f);
            curve.AddKey(1, 1);
            float fresult = curve.Evaluate(flashy);

            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Fade", fresult);
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
                CameraPlay.Infrared_Switch = false;
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