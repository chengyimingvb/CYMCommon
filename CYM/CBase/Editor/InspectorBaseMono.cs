using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CYM;
using Sirenix.OdinInspector.Editor;
namespace CYM
{
    [InitializeOnLoad]
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BaseMono), true)]
    public class InspectorBaseMono : OdinEditor
    {
        BaseMono preFabOverride;

        protected override void OnEnable()
        {
            base.OnEnable();
            preFabOverride = target as BaseMono;

        }

        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();
            if (preFabOverride == null)
                return;
            //this.DrawDefaultInspector ();

            PreFabOverride.MakeFieldsOverride(preFabOverride);

        }
    }
}
