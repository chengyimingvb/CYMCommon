using UnityEngine;

namespace FoW
{
    public enum FogOfWarBlurType
    {
        Gaussian3,
        Gaussian5,
        Antialias
    }

    public class FogOfWarBlur
    {
        RenderTexture _target;
        RenderTexture _source;
        static Material _blurMaterial = null;

        void SetupRenderTarget(Vector2i resolution, ref RenderTexture tex)
        {
            if (tex == null)
                tex = new RenderTexture(resolution.x, resolution.y, 0);
            else if (tex.width != resolution.x || tex.height != resolution.y)
            {
                tex.width = resolution.x;
                tex.height = resolution.y;
            }
        }

        public Texture Apply(Texture2D fogtexture, Vector2i resolution, int amount, int iterations, FogOfWarBlurType type)
        {
            if (amount <= 0 || iterations <= 0)
                return fogtexture;

            if (_blurMaterial == null)
                _blurMaterial = new Material(Shader.Find("Hidden/FogOfWarBlurShader"));

            _blurMaterial.SetFloat("_BlurAmount", amount);
            _blurMaterial.SetKeywordEnabled("GAUSSIAN3", type == FogOfWarBlurType.Gaussian3);
            _blurMaterial.SetKeywordEnabled("GAUSSIAN5", type == FogOfWarBlurType.Gaussian5);
            _blurMaterial.SetKeywordEnabled("ANTIALIAS", type == FogOfWarBlurType.Antialias);

            SetupRenderTarget(resolution, ref _target);
            if (iterations > 1)
                SetupRenderTarget(resolution, ref _source);

            RenderTexture lastrt = RenderTexture.active;

            RenderTexture.active = _target;
            Graphics.Blit(fogtexture, _blurMaterial);

            for (int i = 1; i < iterations; ++i)
            {
                FogOfWarUtils.Swap(ref _target, ref _source);
                RenderTexture.active = _target;
                Graphics.Blit(_source, _blurMaterial);
            }

            RenderTexture.active = lastrt;

            return _target;
        }
    }
}
