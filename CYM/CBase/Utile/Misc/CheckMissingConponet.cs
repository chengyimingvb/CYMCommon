using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{

    [ExecuteInEditMode]
    public class CheckMissingConponet : MonoBehaviour
    {
        public bool Check = false;
        private void OnEnable()
        {

        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Check)
            {
                DoCheck();
                Check = false;
            }
        }
        void DoCheck()
        {
            Transform[] trans = GetComponentsInChildren<Transform>(true);
            foreach (var item in trans)
            {

                foreach (var com in item.gameObject.GetComponents<Component>())
                {

                    if (com == null)
                        Debug.LogError("Missing:" + GetPath(item.gameObject), item.gameObject);
                }
            }
        }

        private static string GetPath(GameObject go)
        {
            return go.transform.parent == null ? "/" + go.name : GetPath(go.transform.parent.gameObject) + "/" + go.name;
        }
    }
}