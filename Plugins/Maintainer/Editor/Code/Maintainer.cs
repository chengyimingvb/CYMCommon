#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using UnityEngine;

namespace CodeStage.Maintainer
{
	public class Maintainer
	{
		public const string LogPrefix = "<b>[Maintainer]</b> ";
		public const string Version = "1.4.0.1";
		public const string SupportEmail = "focus@codestage.net";

		internal const string DataLossWarning = "Make sure you've made a backup of your project before proceeding.\nAuthor is not responsible for any data loss due to use of the Maintainer!";

		private static string directory;

		public static string Directory
		{ 
			get
			{
				if (!string.IsNullOrEmpty(directory)) return directory;

				directory = MaintainerMarker.GetAssetPath();

				if (!string.IsNullOrEmpty(directory))
				{
					if (directory.IndexOf("Editor/Code/MaintainerMarker.cs", StringComparison.Ordinal) >= 0)
					{
						directory = directory.Replace("/Code/MaintainerMarker.cs", "");
					}
					else
					{
						directory = null;

						Debug.LogError(ConstructError("Looks like Maintainer is placed in project incorrectly!"));
					}
				}
				else
				{
					directory = null;
					Debug.LogError(ConstructError("Can't locate the Maintainer directory!"));
				}
				return directory;
			}
		}

		public static string ConstructError(string errorText, string moduleName = null)
		{
			return LogPrefix + (string.IsNullOrEmpty(moduleName) ? "" : moduleName + ": ") + errorText + "\nPlease report to " + SupportEmail;
		}

		public static string ConstructWarning(string warningText, string moduleName = null)
		{
			return LogPrefix + (string.IsNullOrEmpty(moduleName) ? "" : moduleName + ": ") + warningText;
		}
	}
}