﻿namespace GeneralAlgorithms.Tests.Algorithms.Polygons
{
	using GeneralAlgorithms.Algorithms.Polygons;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Polygons;
	using NUnit.Framework;

	[TestFixture]
	internal class GridPolygonOverlapTests
	{
		private GridPolygonOverlap polygonOverlap;

		[SetUp]
		public void SetUp()
		{
			polygonOverlap = new GridPolygonOverlap();
		}

		[Test]
		public void DoOverlap_IdenticalRectangles_ReturnsTrue()
		{
			{
				// Identical squares
				var r1 = new GridRectangle(new IntVector2(0, 0), new IntVector2(2, 2));
				Assert.AreEqual(true, polygonOverlap.DoOverlap(r1, r1));
			}

			{
				// Identical rectangles
				var r1 = new GridRectangle(new IntVector2(0, 0), new IntVector2(4, 2));
				Assert.AreEqual(true, polygonOverlap.DoOverlap(r1, r1));
			}
		}

		[Test]
		public void DoOverlap_OverlappingRectangles_ReturnsTrue()
		{
			{
				// Overlapping squares
				var r1 = new GridRectangle(new IntVector2(0, 0), new IntVector2(2, 2));
				var r2 = r1 + new IntVector2(1, 0);
				Assert.AreEqual(true, polygonOverlap.DoOverlap(r1, r2));
			}

			{
				// Overlapping rectangles
				var r1 = new GridRectangle(new IntVector2(4, 5), new IntVector2(10, 7));
				var r2 = new GridRectangle(new IntVector2(5, 6), new IntVector2(12, 10));
				Assert.AreEqual(true, polygonOverlap.DoOverlap(r1, r2));
			}

			{
				// A rectangle inside another rectangle
				var r1 = new GridRectangle(new IntVector2(1, 1), new IntVector2(5, 6));
				var r2 = new GridRectangle(new IntVector2(2, 2), new IntVector2(4, 5));
				Assert.AreEqual(true, polygonOverlap.DoOverlap(r1, r2));
			}
		}

		[Test]
		public void DoOverlap_NonOverlappingRectangles_ReturnsFalse()
		{
			{
				// Non-overlapping squares
				var r1 = new GridRectangle(new IntVector2(0, 0), new IntVector2(2, 2));
				var r2 = r1 + new IntVector2(6, 0);
				Assert.AreEqual(false, polygonOverlap.DoOverlap(r1, r2));
			}

			{
				// Non-overlapping rectangles
				var r1 = new GridRectangle(new IntVector2(0, 0), new IntVector2(6, 3));
				var r2 = new GridRectangle(new IntVector2(7, 4), new IntVector2(10, 6));
				Assert.AreEqual(false, polygonOverlap.DoOverlap(r1, r2));
			}
		}

		[Test]
		public void DoOverlap_TouchingRectangles_ReturnsFalse()
		{
			{
				// Side-touching squares
				var r1 = new GridRectangle(new IntVector2(0, 0), new IntVector2(2, 2));
				var r2 = r1 + new IntVector2(0, 2);
				Assert.AreEqual(false, polygonOverlap.DoOverlap(r1, r2));
			}

			{
				// Vertex-touching rectangles
				var r1 = new GridRectangle(new IntVector2(0, 0), new IntVector2(6, 3));
				var r2 = new GridRectangle(new IntVector2(6, 3), new IntVector2(10, 6));
				Assert.AreEqual(false, polygonOverlap.DoOverlap(r1, r2));
			}
		}

		[Test]
		public void OverlapArea_NonTouching_ReturnsZero()
		{
			var r1 = GridPolygonUtils.GetSquare(6);
			var r2 = GridPolygonUtils.GetRectangle(2, 8);

			Assert.AreEqual(0, polygonOverlap.OverlapArea(r1, new IntVector2(0,0), r2, new IntVector2(7, 2)));
		}

		[Test]
		public void OverlapArea_TwoSquares()
		{
			var r1 = GridPolygonUtils.GetSquare(6);
			var r2 = GridPolygonUtils.GetSquare(3);

			Assert.AreEqual(6, polygonOverlap.OverlapArea(r1, new IntVector2(0, 0), r2, new IntVector2(2, -1)));
		}

		[Test]
		public void OverlapArea_TwoRectangles()
		{
			var r1 = GridPolygonUtils.GetRectangle(4, 6);
			var r2 = GridPolygonUtils.GetRectangle(5, 3);

			Assert.AreEqual(9, polygonOverlap.OverlapArea(r1, new IntVector2(0, 0), r2, new IntVector2(1, 2)));
		}

		[Test]
		public void DoTouch_TwoSquares()
		{
			var r1 = GridPolygonUtils.GetSquare(6);
			var r2 = GridPolygonUtils.GetSquare(3);

			Assert.AreEqual(true, polygonOverlap.DoTouch(r1, new IntVector2(0, 0), r2, new IntVector2(6, 0)));
			Assert.AreEqual(false, polygonOverlap.DoTouch(r1, new IntVector2(0, 0), r2, new IntVector2(6, -3)));
			Assert.AreEqual(true, polygonOverlap.DoTouch(r1, new IntVector2(0, 0), r2, new IntVector2(6, -2)));
		}
	}
}
