using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;

namespace TGS {

	public partial class Territory: IAdmin {

		public string name { get; set; }

		public Region region { get; set; }

		public Polygon polygon { get; set; }

		public Vector2 center;
		public Vector2 scaledCenter;
		public List<Cell> cells;
		public Color fillColor = Color.gray;
		public Color frontierColor = new Color(0, 0, 0, 0);

		public bool visible { get; set; }
		public bool borderVisible { get; set; }

		public bool neutral { get; set; }


		/// <summary>
		/// Used internally to control incremental updates
		/// </summary>
		public bool isDirty { get; set; }


		public Territory() : this("") {
		}

		public Territory(string name) {
			this.name = name;
			visible = true;
			borderVisible = true;
			cells = new List<Cell>();
		}

	}

}