using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CYM
{
    [ExecuteInEditMode]
    public class PlayParticle : MonoBehaviour
    {
        ParticleSystem[] PS;

        public bool IsPlay;
        public bool IsStop;

        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }

        }
        private void OnEnable()
        {
            PS = gameObject.GetComponentsInChildren<ParticleSystem>();
        }
        private void Update()
        {
            if (IsPlay)
            {
                foreach (var item in PS)
                {
                    item.Play();
                }
                IsPlay = false;
            }

            if (IsStop)
            {
                foreach (var item in PS)
                {
                    item.Stop();
                }
                IsStop = false;
            }
        }
    }
}