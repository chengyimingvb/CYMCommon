using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CYM.UI
{
    [CustomPropertyDrawer(typeof(Presenter<>),true)]
    public class BasePresenterEditor : Editor
    {

        protected virtual void OnEnable()
        {
            Presenter<PresenterData> p = target as Presenter<PresenterData>;
            p.AutoSetup();
        }

    }
}
