using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {
	public class Polygon {

		public List<Contour> contours;
		public Rectangle bounds;

		public Polygon () {
			contours = new List<Contour> ();
			bounds = null;
		}

		public void Clear () {
			contours.Clear ();
			bounds = null;
		}

		public Polygon Clone () {
			Polygon g = new Polygon ();
			for (int k = 0; k < contours.Count; k++) {
				g.AddContour (contours [k].Clone ());
			}
			return g;
		}


		public Rectangle boundingBox {
			get {
				if (bounds != null)
					return bounds;
			
				Rectangle bb = null;
				foreach (Contour c in contours) {
					Rectangle cBB = c.boundingBox;
					if (bb == null)
						bb = cBB;
					else
						bb = bb.Union (cBB);
				}
				bounds = bb;
				return bounds;
			}
		}

		public void AddContour (Contour c) {
			contours.Add (c);
		}

	}

}