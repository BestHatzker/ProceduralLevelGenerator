﻿namespace MapGeneration
{
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using GeneralAlgorithms.Algorithms.Polygons;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Polygons;
	using Grid;
	using Grid.Fast;
	using Utils.Benchmarks;

	internal class Program
	{
		private static void Main(string[] args)
		{
			var configuartionSpacesGenerator = new ConfigurationSpacesGenerator();
			var polygons = new List<GridPolygon>()
			{
				GridPolygon.GetSquare(3),
				GridPolygon.GetRectangle(3, 5),
				new GridPolygonBuilder()
					.AddPoint(0, 0)
					.AddPoint(0, 4)
					.AddPoint(2, 4)
					.AddPoint(2, 2)
					.AddPoint(6, 2)
					.AddPoint(6, 0)
					.Build(),
				new GridPolygonBuilder()
					.AddPoint(0, 0)
					.AddPoint(0, 4)
					.AddPoint(2, 4)
					.AddPoint(2, 2)
					.AddPoint(4, 2)
					.AddPoint(4, 0)
					.Build(),
				new GridPolygonBuilder()
					.AddPoint(0, 0)
					.AddPoint(0, 2)
					.AddPoint(2, 2)
					.AddPoint(2, 4)
					.AddPoint(4, 4)
					.AddPoint(4, 2)
					.AddPoint(6, 2)
					.AddPoint(6, 0)
					.Build()
			};

			polygons = polygons.Select(x => x.Scale(new IntVector2(4, 4))).ToList();
			var benchmark = new Benchmark();

			{
				var generator = new LayoutGenerator<int>(configuartionSpacesGenerator.Generate(polygons));
				generator.EnableTranslation();
				benchmark.Execute<GridPolygon, IntVector2, AbstractLayoutGenerator<int, GridPolygon, IntVector2>>(generator, "Generator with translation");
			}

			{
				var generator = new LayoutGenerator<int>(configuartionSpacesGenerator.Generate(polygons));
				benchmark.Execute<GridPolygon, IntVector2, AbstractLayoutGenerator<int, GridPolygon, IntVector2>>(generator, "Basic generator");
			}

		}
	}
}
