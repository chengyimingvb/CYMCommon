using System;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class BaseTabData: BaseTextData
    {
        /// <summary>
        /// 连接的Presenter
        /// </summary>
        public BasePresenter LinkPresenter;
        /// <summary>
        /// 连接的View
        /// </summary>
        public BaseUIView LinkView;
    }

    public class BaseTab : Presenter<BaseTabData>
    {
        #region inspector
        [SerializeField]
        GameObject ActiveObj;
        #endregion

        #region 组建
        public Text Text;
        public Image Image;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Text != null)
            {
                Text.text = Data.GetName();
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="b"></param>
        public void Select(bool b)
        {
            ActiveObj.SetActive(b);
        }
        #endregion

        #region editor
        public override void AutoSetup()
        {
            base.AutoSetup();


            if (Text == null)
            {
                Text = GetComponentInChildren<Text>();
                if (Text != null)
                {
                    CLog.Debug(CLog.Tag_UI, "Text为空，自动设置一个子text");
                }
            }

        }
        #endregion

    }

}