using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class BaseProgressData : PresenterData
    {
        public Func<float> Value=()=> { return 0.0f; };
        public Func<float, string> ValueText = (x) => { return BaseUIUtils.OptionalTwoDigit(x); };
        public bool IsTween=false;
    }
    public class BaseProgress : Presenter<BaseProgressData>
    {
        #region 组建
        public Image Fill;
        public Text Text;
        #endregion

        Tweener tweener;

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Fill != null)
            {
                if (Data.IsTween)
                {
                    if (tweener != null)
                        tweener.Kill();
                    tweener =DOTween.To(()=>Fill.fillAmount,x=> Fill.fillAmount=x, Data.Value.Invoke(),1.0f)
                        .OnUpdate(()=> Text.text = Data.ValueText.Invoke(Fill.fillAmount));
                }
                else
                    Fill.fillAmount = Data.Value.Invoke();
            }
            if (Text != null)
            {
                if (Data.IsTween)
                {
                    //不做任何事情
                }
                else
                    Text.text = Data.ValueText.Invoke(Fill.fillAmount);
            }
        }
        public virtual void Refresh(float val,string text)
        {
            if (Fill != null)
            {
                Fill.fillAmount = val;
            }
            if (Text != null)
            {
                Text.text = text;
            }
        }
        #endregion
    }

}