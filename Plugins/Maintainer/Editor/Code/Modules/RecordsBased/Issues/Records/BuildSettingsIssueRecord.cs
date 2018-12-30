#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Text;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Tools;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public class BuildSettingsIssueRecord : IssueRecord, IShowableRecord
	{
		public void Show()
		{
			CSEditorTools.RevealInSettings(AssetSettingsKind.EditorBuildSettings);
		}

		internal static BuildSettingsIssueRecord Create(RecordType type, string body)
		{
            return new BuildSettingsIssueRecord(type, body);
		}

		protected BuildSettingsIssueRecord(RecordType type, string body):base(type, RecordLocation.BuildSettings)
		{
			bodyExtra = body;
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Build Settings</b> issue");
		}

		internal override bool CanBeFixed()
		{
			return false;
		}
	}
}