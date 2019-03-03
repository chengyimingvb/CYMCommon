using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;
using TGS.PathFinding;

namespace TGS
{
	
	/* Event definitions */

	public delegate int PathFindingEvent (int cellIndex);


	public partial class TerrainGridSystem : MonoBehaviour
	{

		/// <summary>
		/// Fired when path finding algorithmn evaluates a cell. Return the increased cost for cell.
		/// </summary>
		public event PathFindingEvent OnPathFindingCrossCell;

	
		[SerializeField]
		HeuristicFormula
			_pathFindingHeuristicFormula = HeuristicFormula.EuclideanNoSQR;

		/// <summary>
		/// The path finding heuristic formula to estimate distance from current position to destination
		/// </summary>
		public PathFinding.HeuristicFormula pathFindingHeuristicFormula {
			get { return _pathFindingHeuristicFormula; }
			set {
				if (value != _pathFindingHeuristicFormula) {
					_pathFindingHeuristicFormula = value;
					isDirty = true;
				}
			}
		}

        [SerializeField]
        int
            _pathFindingMaxSteps = 2000;

        /// <summary>
        /// The maximum number of steps that a path can return.
        /// </summary>
        public int pathFindingMaxSteps {
            get { return _pathFindingMaxSteps; }
            set {
                if (value != _pathFindingMaxSteps) {
                    _pathFindingMaxSteps = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
		float
            _pathFindingMaxCost = 200000;

        /// <summary>
        /// The maximum search cost of the path finding execution.
        /// </summary>
		public float pathFindingMaxCost {
            get { return _pathFindingMaxCost; }
            set {
                if (value != _pathFindingMaxCost) {
                    _pathFindingMaxCost = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
			_pathFindingUseDiagonals = true;

		/// <summary>
		/// If path can include diagonals between cells
		/// </summary>
		public bool pathFindingUseDiagonals {
			get { return _pathFindingUseDiagonals; }
			set {
				if (value != _pathFindingUseDiagonals) {
					_pathFindingUseDiagonals = value;
					isDirty = true;
				}
			}
		}

        [SerializeField]
        float
            _pathFindingHeavyDiagonalsCost = 1.4f;

        /// <summary>
        /// The cost for crossing diagonals.
        /// </summary>
        public float pathFindingHeavyDiagonalsCost {
            get { return _pathFindingHeavyDiagonalsCost; }
            set {
                if (value != _pathFindingHeavyDiagonalsCost) {
                    _pathFindingHeavyDiagonalsCost = value;
                    isDirty = true;
                }
            }
        }


		#region Public Path Finding functions

		/// <summary>
		/// Returns an optimal path from startPosition to endPosition with options.
		/// </summary>
		/// <returns>The route consisting of a list of cell indexes.</returns>
		/// <param name="startPosition">Start position in map coordinates (-0.5...0.5)</param>
		/// <param name="endPosition">End position in map coordinates (-0.5...0.5)</param>
		/// <param name="maxSearchCost">Maximum search cost for the path finding algorithm. A value of 0 will use the global default defined by pathFindingMaxCost</param>
		/// <param name="maxSteps">Maximum steps for the path. A value of 0 will use the global default defined by pathFindingMaxSteps</param>
        public List<int> FindPath (int cellIndexStart, int cellIndexEnd, float maxSearchCost = -1, int maxSteps = 0, int cellGroupMask = -1) {
            float dummy;
			return FindPath (cellIndexStart, cellIndexEnd, out dummy, maxSearchCost, maxSteps, cellGroupMask);
		}

		/// <summary>
		/// Returns an optimal path from startPosition to endPosition with options.
		/// </summary>
		/// <returns>The route consisting of a list of cell indexes.</returns>
		/// <param name="startPosition">Start position in map coordinates (-0.5...0.5)</param>
		/// <param name="endPosition">End position in map coordinates (-0.5...0.5)</param>
		/// <param name="totalCost">The total accumulated cost for the path</param>
		/// <param name="maxSearchCost">Maximum search cost for the path finding algorithm. A value of 0 will use the global default defined by pathFindingMaxCost</param>
		/// <param name="maxSteps">Maximum steps for the path. A value of 0 will use the global default defined by pathFindingMaxSteps</param>
		/// <param name="ignoreStartEndCellCanCrossCheck">Pass true to ignore verification if starting/end cell are marked as blocked or not.</param>
		public List<int> FindPath (int cellIndexStart, int cellIndexEnd, out float totalCost, float maxSearchCost = 0, int maxSteps = 0, int cellGroupMask = -1, bool ignoreStartEndCellCanCrossCheck = false)
		{
			totalCost = 0;
			Cell startCell = cells [cellIndexStart];
			Cell endCell = cells [cellIndexEnd];
			List<int> routePoints = null;
			if (cellIndexStart != cellIndexEnd) {
				bool startCellCanCross = startCell.canCross;
				bool endCellCanCross = endCell.canCross;
				if (ignoreStartEndCellCanCrossCheck) {
					startCell.canCross = endCell.canCross = true;
				} else if (!startCell.canCross || !endCell.canCross)
					return null;
				PathFindingPoint startingPoint = new PathFindingPoint (startCell.column, startCell.row);
				PathFindingPoint endingPoint = new PathFindingPoint (endCell.column, endCell.row);
				ComputeRouteMatrix ();
				finder.Formula = _pathFindingHeuristicFormula;
				finder.MaxSteps = maxSteps > 0 ? maxSteps : _pathFindingMaxSteps;
				finder.Diagonals = _pathFindingUseDiagonals;
                finder.HeavyDiagonalsCost = _pathFindingHeavyDiagonalsCost;
				finder.HexagonalGrid = _gridTopology == GRID_TOPOLOGY.Hexagonal;
                finder.MaxSearchCost = maxSearchCost > 0 ? maxSearchCost : _pathFindingMaxCost;
				finder.CellGroupMask = cellGroupMask;
				if (OnPathFindingCrossCell != null) {
					finder.OnCellCross = FindRoutePositionValidator;
				} else {
					finder.OnCellCross = null;
				}
				List<PathFinderNode> route = finder.FindPath (startingPoint, endingPoint, out totalCost, _evenLayout);
				startCell.canCross = startCellCanCross;
				endCell.canCross = endCellCanCross;
				if (route != null) {
					int routeCount = route.Count;
					routePoints = new List<int> (routeCount);
					for (int r = routeCount - 2; r >= 0; r--) {
						int cellIndex = route [r].PY * _cellColumnCount + route [r].PX;
						routePoints.Add (cellIndex);
					}
					routePoints.Add (cellIndexEnd);
				} else {
					return null;	// no route available
				}
			}
			return routePoints;
		}

		#endregion


	
	}
}

