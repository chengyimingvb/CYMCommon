using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class Instan : MonoBehaviour
    {
        public GameObject Prefab;
        public bool IsAttachParent = true;
        public bool IsDestroy = false;
        void Awake()
        {
            GameObject go = Instantiate(Prefab);
            if (IsAttachParent)
                go.transform.SetParent(transform, false);
            if (IsDestroy)
                Destroy(gameObject);
        }
    }
}
