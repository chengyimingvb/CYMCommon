#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public abstract class IssueRecord: RecordBase
	{
		private static readonly Dictionary<RecordType, RecordSeverity> recordTypeSeverity = new Dictionary<RecordType, RecordSeverity>
		{
			{RecordType.EmptyArrayItem, RecordSeverity.Info},
			{RecordType.MissingComponent, RecordSeverity.Error},
			{RecordType.MissingReference, RecordSeverity.Warning},
			{RecordType.DuplicateComponent, RecordSeverity.Warning},
			{RecordType.InconsistentTerrainData, RecordSeverity.Warning},
			{RecordType.MissingPrefab, RecordSeverity.Warning},
			{RecordType.DisconnectedPrefab, RecordSeverity.Info},
			{RecordType.EmptyMeshCollider, RecordSeverity.Info},
			{RecordType.EmptyMeshFilter, RecordSeverity.Info},
			{RecordType.EmptyAnimation, RecordSeverity.Info},
			{RecordType.EmptyRenderer, RecordSeverity.Info},
			{RecordType.EmptySpriteRenderer, RecordSeverity.Info},
			{RecordType.EmptyTerrainCollider, RecordSeverity.Info},
			{RecordType.EmptyAudioSource, RecordSeverity.Info},
			{RecordType.UndefinedTag, RecordSeverity.Warning},
			{RecordType.UnnamedLayer, RecordSeverity.Info},
			{RecordType.HugePosition, RecordSeverity.Warning},
			{RecordType.DuplicateScenesInBuild, RecordSeverity.Info},
			{RecordType.DuplicateTagsAndLayers, RecordSeverity.Info},
			{RecordType.Other, RecordSeverity.Info},
			{RecordType.Error, RecordSeverity.Error}
		};

		public RecordSeverity severity;
		public RecordType type;

		public bool @fixed;

		internal bool Fix(bool batchMode)
		{
			@fixed = PerformFix(batchMode);
			return @fixed;
		}

		internal abstract bool CanBeFixed();

		// ----------------------------------------------------------------------------
		// base constructors
		// ----------------------------------------------------------------------------

		protected IssueRecord(RecordType type)
		{
			this.type = type;
			severity = recordTypeSeverity[type];
		}

		protected IssueRecord(RecordType type, RecordLocation location):this(type)
		{
			this.location = location;
		}

		// ----------------------------------------------------------------------------
		// issue compact line generation
		// ----------------------------------------------------------------------------

		protected override void ConstructCompactLine(StringBuilder text)
		{
			ConstructHeader(text);
		}

		// ----------------------------------------------------------------------------
		// issue header generation
		// ----------------------------------------------------------------------------

		protected override void ConstructHeader(StringBuilder text)
		{
			switch (type)
			{
				case RecordType.MissingComponent:
					text.Append(headerFormatArgument > 1 ? string.Format("{0} missing components", headerFormatArgument) : "Missing component");
					break;
				case RecordType.MissingReference:
					text.Append("Missing reference");
					break;
				case RecordType.DuplicateComponent:
					text.Append("Duplicate component");
					break;
				case RecordType.EmptyArrayItem:
					text.Append(headerFormatArgument > 1 ? string.Format("Array with {0} empty items", headerFormatArgument) : "Array with empty item");
					break;
				case RecordType.MissingPrefab:
					text.Append("Instance of missing prefab");
					break;
				case RecordType.DisconnectedPrefab:
					text.Append("Disconnected prefab instance");
					break;
				case RecordType.EmptyMeshCollider:
					text.Append("MeshCollider without mesh");
					break;
				case RecordType.EmptyMeshFilter:
					text.Append("MeshFilter without mesh");
					break;
				case RecordType.EmptyAnimation:
					text.Append("Animation without any clips");
					break;
				case RecordType.EmptyRenderer:
					text.Append("Renderer without material");
					break;
				case RecordType.EmptySpriteRenderer:
					text.Append("SpriteRenderer without sprite");
					break;
				case RecordType.EmptyTerrainCollider:
					text.Append("TerrainCollider without Terrain Data");
					break;
				case RecordType.EmptyAudioSource:
					text.Append("AudioSource without AudioClip");
					break;
				case RecordType.UndefinedTag:
					text.Append("GameObject with undefined tag");
					break;
				case RecordType.UnnamedLayer:
					text.Append("GameObject with unnamed layer");
					break;
				case RecordType.HugePosition:
					text.Append("GameObject with huge position");
					break;
				case RecordType.InconsistentTerrainData:
					text.Append("Terrain and TerrainCollider with different Terrain Data");
					break;
				case RecordType.DuplicateScenesInBuild:
					text.Append("Same scene added to the Scenes In Build multiple times");
					break;
				case RecordType.DuplicateTagsAndLayers:
					text.Append("Duplicate item(s) found at the Tags and Layers settings");
					break;
				case RecordType.Error:
					text.Append("Error!");
					break;
				case RecordType.Other:
					text.Append("Other");
					break;
				default:
					text.Append("Unknown issue!");
					break;
			}
		}

		protected virtual bool PerformFix(bool batchMode)
		{
			return false;
		}

		
	}
}