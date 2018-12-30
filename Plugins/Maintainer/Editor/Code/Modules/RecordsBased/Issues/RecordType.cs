#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	public enum RecordType
	{
		/* object issues */

		MissingComponent = 0,
		DuplicateComponent = 50,
		MissingReference = 100,
		EmptyArrayItem = 200,
		MissingPrefab = 300,
		DisconnectedPrefab = 400,
		EmptyMeshCollider = 500,
		EmptyMeshFilter = 510,
		EmptyAnimation = 520,
		EmptyRenderer = 600,
		EmptySpriteRenderer = 610,
		EmptyTerrainCollider = 620,
		EmptyAudioSource = 630,
		UndefinedTag = 700,
		UnnamedLayer = 800,
		HugePosition = 900,
		InconsistentTerrainData = 1100,

		/* project settings issues */
		 
		DuplicateScenesInBuild = 3000,
		DuplicateTagsAndLayers = 3010,
		Error = 5000,
		Other = 100000
	}
}