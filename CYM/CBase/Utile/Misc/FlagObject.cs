using CYM;
using UnityEngine;
namespace CYM
{
    public class FlagObject : MonoBehaviour
    {
        public GameObject flag;
        Material mat;
        void Awake()
        {
            mat = flag.GetComponent<Renderer>().material;
        }
        public void SetFlag(Texture texture)
        {
            mat.SetTexture("_MainTex", texture);
        }
    }

}