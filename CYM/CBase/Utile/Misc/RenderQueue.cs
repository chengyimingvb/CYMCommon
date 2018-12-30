using UnityEngine;
using System.Collections;
namespace CYM
{
    public class RenderQueue : MonoBehaviour
    {
        public int renderQueue = 10000;
        Renderer[] rds;
        void Awake()
        {
            rds = GetComponentsInChildren<Renderer>();
        }
        void OnEnable()
        {
            SetRenderQueue(renderQueue);
        }
        public void SetRenderQueue(int renderQueue)
        {
            for (int i = 0; i < rds.Length; ++i)
            {
                rds[i].material.renderQueue = renderQueue;
            }
            
        }

    }
}
