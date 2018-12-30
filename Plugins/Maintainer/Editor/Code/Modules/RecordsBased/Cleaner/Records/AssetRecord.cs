#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Text;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.Cleaner
{
	[Serializable]
	public class AssetRecord : CleanerRecord, IShowableRecord
	{
		public string path;
		public long size; // in bytes
		public string beautyPath;
		public string assetDatabasePath;
		public Type assetType;
		public bool isTexture;

		public void Show()
		{
			if (!CSEditorTools.RevealAndSelect(assetDatabasePath, null, -1, -1, null))
			{
				MaintainerWindow.ShowNotification("Can't show it properly");
			}
		}

		internal static AssetRecord CreateEmptyFolderRecord(string folderPath)
		{
			var newRecord = new AssetRecord(folderPath);

			if (!string.IsNullOrEmpty(newRecord.path))
			{
				return newRecord;
			}

			return null;
		}

		internal static AssetRecord Create(RecordType type, AssetInfo assetInfo)
		{
			var newRecord = new AssetRecord(type, assetInfo);
			return newRecord;
		}

		protected AssetRecord(string folderPath) : base(RecordType.EmptyFolder, RecordLocation.Asset)
		{
			path = folderPath;

			var index = Application.dataPath.IndexOf("/Assets", StringComparison.Ordinal);

			assetDatabasePath = !Path.IsPathRooted(folderPath) ? folderPath : folderPath.Replace('\\', '/').Substring(index + 1);
			beautyPath = CSEditorTools.NicifyAssetPath(assetDatabasePath);
		}

		protected AssetRecord(RecordType type, AssetInfo assetInfo) : base(type, RecordLocation.Asset)
		{
			path = assetInfo.AssetPath;

			assetDatabasePath = CSEditorTools.GetAssetDatabasePath(path);
			beautyPath = CSEditorTools.NicifyAssetPath(assetDatabasePath);
			assetType = assetInfo.Type;

			if (assetType.BaseType == typeof(Texture))
			{
				isTexture = true;
			}

			if (type == RecordType.UnreferencedAsset)
			{
				size = assetInfo.Size;
			}
		}

		protected override void ConstructCompactLine(StringBuilder text)
		{
			text.Append(beautyPath);
		}

		protected override void ConstructHeader(StringBuilder header)
		{
			base.ConstructHeader(header);

			if (type == RecordType.UnreferencedAsset)
			{
				header.Append(assetType.Name);
			}
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Path:</b> ").Append(beautyPath);
			if (size > 0)
			{
				text.AppendLine().Append("<b>Size:</b> ").Append(CSEditorTools.FormatBytes(size));
			}
			if (type == RecordType.UnreferencedAsset)
			{
				text.AppendLine().Append("<b>Full Type:</b> ").Append(assetType.FullName);
			}
		}

		protected override bool PerformClean()
		{
			bool result;

			if (MaintainerSettings.Cleaner.useTrashBin)
			{
				result = AssetDatabase.MoveAssetToTrash(assetDatabasePath);
			}
			else
			{
				switch (type)
				{
					case RecordType.EmptyFolder:
						{
							if (Directory.Exists(path))
							{
								Directory.Delete(path, true);
							}
							break;
						}
					case RecordType.UnreferencedAsset:
						{
							if (File.Exists(path))
							{
								CSEditorTools.RemoveReadOnlyAttribute(path);
								File.Delete(path);
							}
							break;
						}
					case RecordType.Error:
						break;
					case RecordType.Other:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				var metaPath = path + ".meta";

				if (File.Exists(metaPath))
				{
					CSEditorTools.RemoveReadOnlyAttribute(metaPath);
					File.Delete(metaPath);
				}

				result = !(Directory.Exists(path) || File.Exists(path));
			}
				
			if (!result)
			{
				Debug.LogWarning(Maintainer.LogPrefix + ProjectCleaner.ModuleName + " can't clean asset: " + beautyPath);
			}
			else
			{
				var directory = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
				{
					var filesInDir = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);

					if (filesInDir.Length == 0)
					{
						CreateEmptyFolderRecord(directory).Clean();
					}
				}
			}

			return result;
		}
	}
}