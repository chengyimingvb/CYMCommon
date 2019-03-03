using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;

namespace TGS {

	public enum CELL_SIDE {
		TopLeft = 0,
		Top = 1,
		TopRight = 2,
		BottomRight = 3,
		Bottom = 4,
		BottomLeft = 5,
		Left = 6,
		Right = 7
	}

	public enum CELL_DIRECTION {
		Exiting = 0,
		Entering = 1,
		Both = 2
	}

	public partial class Cell: IAdmin {
		/// <summary>
		/// Optional cell name.
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// The territory to which this cell belongs to. You can change it using CellSetTerritory method.
		/// WARNING: do not change this value directly, use CellSetTerritory instead.
		/// </summary>
		public int territoryIndex = -1;

		public Region region { get; set; }

		public Polygon polygon { get; set; }

		/// <summary>
		/// Unscaled center. Ranges from -0.5, -0.5 to 0.5, 0.5.
		/// </summary>
		public Vector2 center;

		/// <summary>
		/// Original cell center with applied scale
		/// </summary>
		public Vector2 scaledCenter;

		bool _visible;

		public bool visible { get { return _visible && visibleByRules; } set { _visible = value; } }

		public bool visibleByRules = true;

		public bool borderVisible { get; set; }

		/// <summary>
		/// Used internally to control incremental updates
		/// </summary>
		public bool isDirty { get; set; }

		/// <summary>
		/// Optional value that can be set with CellSetTag. You can later get the cell quickly using CellGetWithTag method.
		/// </summary>
		public int tag;


		public int row, column;

		/// <summary>
		/// If this cell blocks path finding.
		/// </summary>
		public bool canCross = true;

		float[] _crossCost;
		/// <summary>
		/// Used by pathfinding in Cell mode. Cost for crossing a cell for each side. Defaults to 1.
		/// </summary>
		/// <value>The cross cost.</value>
		public float[] crossCost {
			get { return _crossCost; }
			set { _crossCost = value; }
		}

		
		/// <summary>
		/// Group for this cell. A different group can be assigned to use along with FindPath cellGroupMask argument.
		/// </summary>
		public int group = 1;

		public Cell (string name, Vector2 center) {
			this.name = name;
			this.center = center;
			visible = true;
			borderVisible = true;
		}

		public Cell () : this ("", Vector2.zero) {
		}

		public Cell (string name) : this (name, Vector2.zero) {
		}

		public Cell (Vector2 center) : this ("", center) {
		}


		/// <summary>
		/// Gets the side cross cost.
		/// </summary>
		/// <returns>The side cross cost.</returns>
		/// <param name="side">Side.</param>
		public float GetSideCrossCost(CELL_SIDE side) {
			if (_crossCost==null) return 0;
			return _crossCost [(int)side];
		}

		/// <summary>
		/// Assigns a crossing cost for a given hexagonal side
		/// </summary>
		/// <param name="side">Side.</param>
		/// <param name="cost">Cost.</param>
		public void SetSideCrossCost(CELL_SIDE side, float cost) {
			if (_crossCost==null) _crossCost = new float[8];
			_crossCost [(int)side] = cost;
		}

		/// <summary>
		/// Sets the same crossing cost for all sides of the hexagon.
		/// </summary>
		public void SetAllSidesCost(float cost) {
			if (_crossCost==null) _crossCost = new float[8];
			for (int k=0;k<_crossCost.Length;k++) { _crossCost[k] = cost; }
		}
		

	}
}

