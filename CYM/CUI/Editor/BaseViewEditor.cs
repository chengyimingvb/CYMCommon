using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CYM.UI
{
    [CustomPropertyDrawer(typeof(BaseView))]
    public class BaseViewEditor : Editor
    {
        protected virtual void OnEnable()
        {
            BaseView p = target as BaseView;
            //Undo.RecordObject(p, "AutoSetup");
            p.AutoSetup();
        }
    }

}