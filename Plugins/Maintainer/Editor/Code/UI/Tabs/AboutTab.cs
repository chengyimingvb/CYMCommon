#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class AboutTab
	{
		private const string UasLinkShort = "content/32199?aid=1011lGBp&pubref=Maintainer";
		private const string UasLink = "https://www.assetstore.unity3d.com/#!/content/32199?aid=1011lGBp&pubref=Maintainer";
		private const string UasProfileLink = "https://www.assetstore.unity3d.com/#!/search/page=1/sortby=popularity/query=publisher:3918";
		private const string Homepage = "http://codestage.net/uas/maintainer";
		private const string SupportLink = "http://codestage.net/contacts/";
		private const string ChangelogLink = "http://codestage.net/uas_files/maintainer/changelog.txt";

		private GUIContent caption;
		private bool showDebug = false;

		private readonly MaintainerWindow window;

		public AboutTab(MaintainerWindow window)
		{
			this.window = window;
		}

		public GUIContent Caption 
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent("About", CSIcons.About);
				}
				return caption;
			}
		}

		public void Draw()
		{
			using (new GUILayout.HorizontalScope())
			{
				/* logo */

				using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
				{
					GUILayout.FlexibleSpace();

					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();

						var logo = CSImages.Logo;
						if (logo != null)
						{
							var logoRect = EditorGUILayout.GetControlRect(GUILayout.Width(logo.width), GUILayout.Height(logo.height));
							GUI.DrawTexture(logoRect, logo);
							GUILayout.Space(5);
						}

						GUILayout.FlexibleSpace();
					}

					GUILayout.FlexibleSpace();
				}

				/* buttons and stuff */

				using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
				{
					GUILayout.Space(10);
					GUILayout.Label("<size=18>Maintainer v.<b>" + Maintainer.Version + "</b></size>", UIHelpers.centeredLabel);
					GUILayout.Space(5);
					GUILayout.Label("Developed by Dmitriy Yukhanov\n" +
									"Logo by Daniele Giardini\n" +
									"Icons by Google, Austin Andrews, Cody", UIHelpers.centeredLabel);
					GUILayout.Space(10);
					UIHelpers.Separator();
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Homepage", CSIcons.Home))
					{
						Application.OpenURL(Homepage);
					}
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Support contacts", CSIcons.Support))
					{
						Application.OpenURL(SupportLink);
					}
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Full changelog (online)", CSIcons.Log))
					{
						Application.OpenURL(ChangelogLink);
					}
					GUILayout.Space(5);

					//GUILayout.Space(10);
					//GUILayout.Label("Asset Store links", UIHelpers.centeredLabel);
					UIHelpers.Separator();
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Plugin at Unity Asset Store", "Hold CTRL / CMD to open in built-in Asset Store browser.", CSIcons.AssetStore))
					{
						if (!Event.current.control)
						{
							Application.OpenURL(UasLink);
						}
						else
						{
							UnityEditorInternal.AssetStore.Open(UasLinkShort);
						}
					}
					GUILayout.Label("It's really important to know your opinion,\n rates & reviews are <b>greatly appreciated!</b>", UIHelpers.centeredLabel);
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("My profile at Unity Asset Store", CSIcons.Publisher))
					{
						Application.OpenURL(UasProfileLink);
					}
					GUILayout.Label("Check all my plugins!", UIHelpers.centeredLabel);

					if (Event.current.isKey && Event.current.control && Event.current.keyCode == KeyCode.D)
					{
						showDebug = !showDebug;
						Event.current.Use();
					}

					if (showDebug)
					{
						GUILayout.Space(5);
						UIHelpers.Separator();
						GUILayout.Space(5);
						GUILayout.Label("Welcome to secret debug mode =D");
						if (GUILayout.Button("Remove Assets Map"))
						{
							AssetsMap.Delete();
						}

						if (GUILayout.Button("Remove Settings and Close"))
						{
							window.Close();
							MaintainerSettings.Delete();
						}

						if (GUILayout.Button("Re-save all scenes in project"))
						{
							CSSceneTools.ReSaveAllScenes();
						}
					}
				}
			}
		}
	}
}