#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Text;
using UnityEditor;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public class TagsAndLayersIssueRecord : IssueRecord, IShowableRecord
	{
		public void Show()
		{
			EditorApplication.ExecuteMenuItem("Edit/Project Settings/Tags and Layers");
		}

		internal static TagsAndLayersIssueRecord Create(RecordType type, string body)
		{
            return new TagsAndLayersIssueRecord(type, body);
		}

		protected TagsAndLayersIssueRecord(RecordType type, string body):base(type, RecordLocation.TagsAndLayers)
		{
			bodyExtra = body;
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Tags and Layers</b> issue");
		}

		internal override bool CanBeFixed()
		{
			return false;
		}
	}
}