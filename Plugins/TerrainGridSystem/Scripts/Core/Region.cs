using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;

namespace TGS {
	public class Region {

		public Polygon polygon;

		public List<Vector2> points { get; set; }

		public List<Segment> segments;
		public List<Region> neighbours;
		public IAdmin entity;
		public Rect rect2D;
		public float rect2DArea;
		public GameObject surfaceGameObject;

		public Material customMaterial { get; set; }

		public Vector2 customTextureScale, customTextureOffset;
		public float customTextureRotation;
		public bool customRotateInLocalSpace;


		public Region (IAdmin entity) {
			this.entity = entity;
			neighbours = new List<Region> (12);
			segments = new List<Segment> (50);
		}


		public void Clear () {
			polygon = null;
			points.Clear ();
			segments.Clear ();
			neighbours.Clear ();
			rect2D.width = rect2D.height = 0;
			rect2DArea = 0;
			if (surfaceGameObject != null)
				GameObject.DestroyImmediate (surfaceGameObject);
			customMaterial = null;
		}

		public Region Clone () {
			Region c = new Region (entity);
			c.customMaterial = this.customMaterial;
			c.customTextureScale = this.customTextureScale;
			c.customTextureOffset = this.customTextureOffset;
			c.customTextureRotation = this.customTextureRotation;
			c.points = new List<Vector2> (points);
			c.polygon = polygon.Clone ();
			c.segments = new List<Segment> (segments);
			return c;
		}

		public bool Intersects (Region otherRegion) {
			return otherRegion.rect2D.Overlaps (otherRegion.rect2D);
		}


		public bool Contains (float x, float y) { 
			if (points == null)
				return false;

			if (x > rect2D.xMax || x < rect2D.xMin || y > rect2D.yMax || y < rect2D.yMin)
				return false;

			int numPoints = points.Count;
			int j = numPoints - 1; 
			bool inside = false; 
			for (int i = 0; i < numPoints; j = i++) { 
				if (((points [i].y <= y && y < points [j].y) || (points [j].y <= y && y < points [i].y)) &&
				    (x < (points [j].x - points [i].x) * (y - points [i].y) / (points [j].y - points [i].y) + points [i].x))
					inside = !inside; 
			} 
			return inside; 
		}

		public bool Contains (Region otherRegion) {
			if (!Intersects (otherRegion))
				return false;

			if (!Contains (otherRegion.rect2D.xMin, otherRegion.rect2D.yMin))
				return false;
			if (!Contains (otherRegion.rect2D.xMin, otherRegion.rect2D.yMax))
				return false;
			if (!Contains (otherRegion.rect2D.xMax, otherRegion.rect2D.yMin))
				return false;
			if (!Contains (otherRegion.rect2D.xMax, otherRegion.rect2D.yMax))
				return false;

			int opc = otherRegion.points.Count;
			for (int k = 0; k < opc; k++) {
				if (!Contains (otherRegion.points [k].x, otherRegion.points [k].y))
					return false;
			}
			return true;
		}
	}
}

