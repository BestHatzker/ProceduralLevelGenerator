﻿namespace MapGeneration.Grid
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Polygons;
	using Interfaces;
	using Utils;

	public class ConfigurationSpaces : IConfigurationSpaces<GridPolygon, Configuration, IntVector2>
	{
		private readonly List<GridPolygon> polygons;
		private readonly Dictionary<GridPolygon, Dictionary<GridPolygon, ConfigurationSpace>> configugurationSpaces;
		private Random random = new Random();

		public ConfigurationSpaces(Dictionary<GridPolygon, Dictionary<GridPolygon, ConfigurationSpace>> configugurationSpaces)
		{
			this.configugurationSpaces = configugurationSpaces;
			polygons = configugurationSpaces.Keys.ToList();

			foreach (var list in configugurationSpaces.Values.SelectMany(x => x.Values).Select(x => x.Points))
			{
				list.Sort();
			}
		}

		public IntVector2 GetRandomIntersection(List<Configuration> configurations, Configuration mainConfiguration)
		{
			var maximumIntersection = GetMaximumIntersection(configurations, mainConfiguration);

			return maximumIntersection.GetRandom(random);
		}

		public List<IntVector2> GetMaximumIntersection(List<Configuration> configurations, Configuration mainConfiguration)
		{
			var spaces = configugurationSpaces[mainConfiguration.Polygon];

			for (var i = configurations.Count; i > 0; i--)
			{
				foreach (var indices in configurations.GetCombinations(i))
				{
					IEnumerable<IntVector2> points = null;

					foreach (var index in indices)
					{
						var newPoints = spaces[configurations[index].Polygon].Points.Select(x => x + configurations[index].Position);
						points = points != null ? points.IntersectSorted(newPoints) : newPoints;

						if (!points.Any())
						{
							break;
						}
					}

					if (points != null && points.Any())
					{
						return points.ToList();
					}
				}
			}

			throw new InvalidOperationException("There should always be at least one point in the intersection");
		}

		public GridPolygon GetRandomShape()
		{
			return polygons.GetRandom(random);
		}

		public ICollection<GridPolygon> GetAllShapes()
		{
			return polygons;
		}

		void IConfigurationSpaces<GridPolygon, Configuration, IntVector2>.InjectRandomGenerator(Random random)
		{
			this.random = random;
		}

		public List<GridPolygon> GetPolygons()
		{
			// TODO: Maybe return only a readonly collection
			return polygons;
		}
	}
}
