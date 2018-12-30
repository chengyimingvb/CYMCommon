using System;
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