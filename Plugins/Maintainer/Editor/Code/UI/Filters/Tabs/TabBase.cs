#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using UnityEngine;

namespace CodeStage.Maintainer.UI.Filters
{
	internal abstract class TabBase
	{
		internal GUIContent caption = new GUIContent("Untitled tab");

		internal Event currentEvent;
		internal EventType currentEventType;

		protected FilterType filterType;

		protected FiltersWindow window;

		protected Vector2 scrollPosition;

		protected TabBase(FilterType filterType)
		{
			this.filterType = filterType;
		}

		internal virtual void Show(FiltersWindow hostingWindow)
		{
			window = hostingWindow;
            scrollPosition = Vector2.zero;
		}

		internal void Draw()
		{
			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
			{
				GUILayout.Space(5);
				DrawTabContents();
			}
		}

		internal abstract void ProcessDrags();

		protected abstract void DrawTabContents();
	}
}