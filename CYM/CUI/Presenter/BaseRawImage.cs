using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class BaseRawImageData : BaseImageData
    {

    }
    public class BaseRawImage : Presenter<BaseRawImageData>
    {
        #region 组建
        [FoldoutGroup("Inspector")]
        public RawImage Image;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Data.Image != null && Image != null)
            {
                Sprite temp = Data.Image.Invoke();
                if (temp == null)
                    return;
                Image.texture = temp.texture;
            }
        }
        #endregion

    }
}