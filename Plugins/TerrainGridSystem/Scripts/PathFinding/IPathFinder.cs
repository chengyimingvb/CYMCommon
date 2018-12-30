using System;
using System.Collections.Generic;
using System.Text;

namespace TGS.PathFinding {

	public delegate float OnCellCross(int location);

	interface IPathFinder {

		HeuristicFormula Formula {
			get;
			set;
		}

		bool Diagonals {
			get;
			set;
		}

		float HeavyDiagonalsCost {
			get;
			set;
		}
		
		bool HexagonalGrid {
			get;
			set;
		}

		float HeuristicEstimate {
			get;
			set;
		}

		int MaxSteps {
			get;
			set;
		}

		float MaxSearchCost {
			get;
			set;
		}

		int CellGroupMask {
			get;
			set;
		}

		List<PathFinderNode> FindPath (PathFindingPoint start, PathFindingPoint end, out float cost, bool evenLayout);

		void SetCalcMatrix (Cell[] grid);

		OnCellCross OnCellCross { get; set; }

	}
}
