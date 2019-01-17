using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CYM.UI
{
    public class BaseScrollView :Presenter<PresenterData>
    {
        ScrollRect ScrollRect;
        [SerializeField]
        Scrollbar Scrollbar;
        [SerializeField]
        ScrollRect.ScrollbarVisibility ScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        protected override void Start()
        {
            base.Start();
            ScrollRect = GetComponent<ScrollRect>();
            if (ScrollRect != null && Scrollbar != null)
            {
                if (ScrollRect.vertical)
                {
                    ScrollRect.verticalScrollbar = Scrollbar;
                }
                else if (ScrollRect.horizontal)
                {
                    ScrollRect.horizontalScrollbar = Scrollbar;
                }
                Scrollbar.size = 0.0f;
                ScrollRect.horizontalScrollbarVisibility = ScrollbarVisibility;
                ScrollRect.verticalScrollbarVisibility = ScrollbarVisibility;
                ScrollRect.movementType = ScrollRect.MovementType.Clamped;
                ScrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
                Scrollbar.onValueChanged.AddListener(_ScrollBar_OnValueChanged);

                if (ScrollRect != null)
                {
                    ScrollRect.inertia = true;
                    ScrollRect.decelerationRate = 0.2f;
                }
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            ResetScrollBar(1.0f);
        }

        private void Update()
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                Scrollbar.size = 0.0f;
            }
        }

        public void ResetScrollBar(float val)
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                Scrollbar.value = val;
            }
        }

        private void _ScrollRect_OnValueChanged(Vector2 arg0)
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                Scrollbar.size = 0.0f;
            }
        }

        private void _ScrollBar_OnValueChanged(float arg0)
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                Scrollbar.size = 0.0f;
            }
        }
    }

}