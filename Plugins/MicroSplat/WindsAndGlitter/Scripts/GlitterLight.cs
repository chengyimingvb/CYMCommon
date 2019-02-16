using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   [ExecuteInEditMode]
   public class GlitterLight : MonoBehaviour {

      #if UNITY_EDITOR
      void OnEnable()
      {
         UnityEditor.EditorApplication.update += Update;
      }

      void OnDisable()
      {
         UnityEditor.EditorApplication.update -= Update;
      }

      #endif

      void Update()
      {
         Shader.SetGlobalVector("_gGlitterLight", -this.transform.forward);
      }
   }

}