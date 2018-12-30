using UnityEngine;
using UnityEngine.UI;
using CYM;
using System.Collections.Generic;

namespace CYM.UI
{
    public class BaseListPresenter<TP,TD>:Presenter<TD> where TP:Presenter<TD> where TD: PresenterData,new()
    {

        //public void Init()
        //{
        //    Items = GP.GetGOs<T>(null, onHintEnter, onHintExit);
        //}

        public virtual void Init(TD[] datas)
        {
            Items = GP.GetGOs<TP, TD>();
            if(Items!=null&&datas!=null)
            {
                foreach(var item in Items)
                {
                    if(item.Index<datas.Length)
                    item.Init(datas[item.Index]);
                }

            }
        }

        //protected virtual void OnScrollSet()
        //{
        //}

        BaseGOProvider _gp;
        public BaseGOProvider GP
        {
            get
            {
                if (_gp == null)
                {
                    _gp = GetComponent<BaseGOProvider>();
                }
                if (_gp == null)
                {
                    throw new System.Exception("cannot find gos provider");
                }
                return _gp;
            }
        }
        public TP[] Items { get; private set; }

        public override void Refresh()
        {
            base.Refresh();
        }

        //public void Refresh<TD>(IList<TD> datas, Callback<T, TD> c)
        //{
        //    //if (Scroll != null)
        //    //{
        //    //    Scroll.RefreshScroll(Items, datas, c);
        //    //}
        //    //else
        //    //{
        //    //    BaseUIUtils.Refresh(Items, datas, c);
        //    //}
        //}

    }

}