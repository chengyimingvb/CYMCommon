using System;
using CYM;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class BaseGOProvider : Presenter<PresenterData>
    {
        #region prop
        public List<GameObject> GOs { get; set; } = new List<GameObject>();
        public int GOCount
        {
            get { return GOs.Count; }
        }
        #endregion

        #region get GOs
        public TP[] GetGOs<TP, TD>(TD data=null) where TP : Presenter<TD> where TD : PresenterData,new()
        {
            if (data == null)
            {
                data = new TD();
            }
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            TP[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<TP>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0}", typeof(TP)));
                    break;
                }
                else
                {
                    ts[i].SetIndex(i);
                    AddChild(ts[i],true);
                    if (data != null)
                    {
                        ts[i].Init(data);
                    }
                }
            }
            return ts;
        }
        public TP[] GetGOs<TP, TD>(TD[] data) where TP : Presenter<TD> where TD : PresenterData, new()
        {
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            TP[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<TP>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0}", typeof(TP)));
                    break;
                }
                else
                {
                    ts[i].SetIndex ( i);
                    AddChild(ts[i],true);
                    if (data != null)
                    {
                        if (i < data.Length)
                            ts[i].Init(data[i]);
                    }
                    else
                    {
                        ts[i].Init(new TD());
                    }
                }
            }
            return ts;
        }
        #endregion

        #region callback
        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerEnter(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标退出
        /// </summary>
        /// <param name="eventData"></param>
		public override void OnPointerExit(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerClick(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerDown(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerUp(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 点击状态变化
        /// </summary>
        /// <param name="b"></param>
        public override void OnInteractable(bool b)
        {
        }
        #endregion
    }
}

