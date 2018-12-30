using System;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class BaseImageData : PresenterData
    {
        public Func<Sprite> Image;
    }

    [RequireComponent(typeof(Image))]
    public class BaseImage : Presenter<BaseImageData>
    {
        #region 组建
        public Image Image;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Data.Image != null)
                Image.overrideSprite = Data.Image.Invoke();
        }
        #endregion

        #region wrap
        public bool Grey
        {
            set
            {
                if (value)
                    Image.material = SelfBaseGlobal.GRMgr.ImageGrey;
                else
                    Image.material = null;
            }
        }
        public Sprite sprite
        {
            get { return Image.sprite; }
            set { Image.sprite = value; }
        }
        #endregion
    }
}
