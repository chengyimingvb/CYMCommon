#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;
using RecordType = CodeStage.Maintainer.Cleaner.RecordType;

namespace CodeStage.Maintainer
{
	internal class RecordsSortings
	{
		internal static Func<CleanerRecord, string>				cleanerRecordByPath = record => record is AssetRecord ? ((AssetRecord)record).path : null;
		internal static Func<CleanerRecord, long>				cleanerRecordBySize = record => record is AssetRecord ? ((AssetRecord)record).size : 0;
		internal static Func<CleanerRecord, Cleaner.RecordType> cleanerRecordByType = record => record.type;
		internal static Func<CleanerRecord, string>				cleanerRecordByAssetType = record => record is AssetRecord ? ((AssetRecord)record).type == RecordType.UnreferencedAsset ? ((AssetRecord)record).assetType.FullName : null : null;

		internal static Func<IssueRecord, string>				issueRecordByPath = record => record is ObjectIssueRecord ? ((ObjectIssueRecord)record).path : null;
		internal static Func<IssueRecord, Issues.RecordType>	issueRecordByType = record => record.type;
		internal static Func<IssueRecord, RecordSeverity>		issueRecordBySeverity = record => record.severity;
	}
}