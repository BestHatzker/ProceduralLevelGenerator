﻿namespace MapGeneration.Core
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Benchmarks;
	using ConfigurationSpaces;
	using Doors;
	using GeneralAlgorithms.Algorithms.Common;
	using GeneralAlgorithms.Algorithms.Graphs.GraphDecomposition;
	using GeneralAlgorithms.Algorithms.Polygons;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Graphs;
	using GeneralAlgorithms.DataStructures.Polygons;
	using Interfaces;

	public class SALayoutGenerator<TNode> : ILayoutGenerator<TNode>, IRandomInjectable, IBenchmarkable
	{
		private readonly IGraphDecomposer<int> graphDecomposer = new GraphDecomposer<int>();
		private IConfigurationSpaces<int, IntAlias<GridPolygon>, Configuration> configurationSpaces;
		private readonly ConfigurationSpacesGenerator configurationSpacesGenerator = new ConfigurationSpacesGenerator(new PolygonOverlap(), DoorHandler.DefaultHandler, new OrthogonalLineIntersection(), new GridPolygonUtils());
		private LayoutOperations<int, Layout, Configuration, IntAlias<GridPolygon>> layoutOperations;
		private Random random = new Random(0);

		private IMapDescription<TNode> mapDescription;
		private FastGraph<TNode> graph;

		// Debug and benchmark variables
		private int iterationsCount;
		private bool withDebugOutput;
		private long timeFirst;
		private long timeTen;
		private int layoutsCount;
		protected bool BenchmarkEnabled;

		// Events
		public event Action<IMapLayout<TNode>> OnPerturbed;
		public event Action<IMapLayout<TNode>> OnValid;
		public event Action<IMapLayout<TNode>> OnValidAndDifferent;

		private double minimumDifference = 150; // TODO: change
		private double shapePerturbChance = 0.4f;

		public SALayoutGenerator()
		{
			
		}

		public IList<IMapLayout<TNode>> GetLayouts(IMapDescription<TNode> mapDescription, int numberOfLayouts = 10)
		{
			// TODO: should not be done like this
			configurationSpaces = configurationSpacesGenerator.Generate((MapDescription<TNode>) mapDescription); 
			layoutOperations = new LayoutOperations<int, Layout, Configuration, IntAlias<GridPolygon>>(configurationSpaces, new PolygonOverlap());
			configurationSpaces.InjectRandomGenerator(random);
			layoutOperations.InjectRandomGenerator(random);

			this.mapDescription = mapDescription;
			graph = mapDescription.GetGraph();

			iterationsCount = 0;
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var stack = new Stack<LayoutNode>();
			var fullLayouts = new List<Layout>();

			/*var c1 = new List<int>() { 11, 12, 18, 19 };
			var c2 = new List<int>() { 7, 20, 4, 8, 13 };
			var c3 = new List<int>() { 6, 5, 2, 0, 3 };
			var c4 = new List<int>() { 1, 9, 10 };
			var c5 = new List<int>() { 23, 24, 30 };
			var c5s = new List<int>() { 21 };
			var c5ss = new List<int>() { 22, 17, 29 };
			var c5sss = new List<int>() { 16, 27, 28 };
			var c6 = new List<int>() { 14, 15, 26, 25 };
			var c7 = new List<int>() { 31, 32, 33 };
			var c8 = new List<int>() { 35, 34, 36 };
			var c9 = new List<int>() { 38, 39, 37 };
			var c10 = new List<int>() { 40 };

			var graphChains = new List<List<int>>()
			{
				c1,
				c2,
				c3,
				c4,
				c5,
				c5s,
				c5ss,
				c5sss,
				c6,
				c7,
				c8,
				c9,
				c10,
			};*/

			var graphChains = graphDecomposer.GetChains(graph);
			var initialLayout = new Layout(graph);

			stack.Push(new LayoutNode { Layout = AddChainToLayout(initialLayout, graphChains[0]), NumberOfChains = 0 });

			if (withDebugOutput)
			{
				Console.WriteLine("--- Simulation has started ---");
			}

			while (stack.Count > 0)
			{
				var layoutNode = stack.Pop();
				var extendedLayouts = GetExtendedLayouts(layoutNode.Layout, graphChains[layoutNode.NumberOfChains], layoutNode.NumberOfChains);

				if (layoutNode.NumberOfChains + 1 == graphChains.Count)
				{
					foreach (var layout in extendedLayouts)
					{
						if (fullLayouts.TrueForAll(x =>
							GetDifference(x, layout) > 2 * minimumDifference)
						)
						{
							if (fullLayouts.Count == 0)
							{
								timeFirst = stopwatch.ElapsedMilliseconds;
							}

							fullLayouts.Add(layout);
						}
					}
				}
				else
				{
					var sorted = extendedLayouts
						.Select(x => AddChainToLayout(x, graphChains[layoutNode.NumberOfChains + 1]))
						.OrderByDescending(x => x.GetEnergy());


					foreach (var extendedLayout in sorted)
					{
						stack.Push(new LayoutNode() { Layout = extendedLayout, NumberOfChains = layoutNode.NumberOfChains + 1 });
					}
				}

				if (fullLayouts.Count >= numberOfLayouts)
				{
					break;
				}
			}

			stopwatch.Stop();
			timeTen = stopwatch.ElapsedMilliseconds;
			layoutsCount = fullLayouts.Count;

			if (withDebugOutput)
			{
				Console.WriteLine($"{fullLayouts.Count} layouts generated");
				Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds} ms");
				Console.WriteLine($"Total iterations: {iterationsCount}");
				Console.WriteLine($"Iterations per second: {(int)(iterationsCount / (stopwatch.ElapsedMilliseconds / 1000f))}");
			}

			// AddDoors(fullLayouts); TODO: how?

			return fullLayouts.Select(ConvertLayout).ToList();
		}

		private List<Layout> GetExtendedLayouts(Layout layout, List<int> chain, int chainNumber)
		{
			var cycles = 50;
			var trialsPerCycle = 500;

			var p0 = 0.2d;
			var p1 = 0.01d;
			var t0 = -1d / Math.Log(p0);
			var t1 = -1d / Math.Log(p1);
			var ratio = Math.Pow(t1 / t0, 1d / (cycles - 1));
			var deltaEAvg = 0d;
			var acceptedSolutions = 1;

			var t = t0;

			var layouts = new List<Layout>();
			var originalLayout = layout; //AddChainToLayout(layout, chain);
			var currentLayout = originalLayout;

			#region Debug output

			if (withDebugOutput)
			{
				Console.WriteLine($"Initial energy: {currentLayout.GetEnergy()}");
			}

			#endregion

			var numFailures = 0;

			for (var i = 0; i < cycles; i++)
			{
				var wasAccepted = false;
				
				#region Random restarts

				if (chainNumber != 0)
				{
					if (numFailures > 8 && random.Next(0, 2) == 0)
					{
						if (withDebugOutput)
						{
							Console.WriteLine($"Break, we got {numFailures} failures");
						}
						break;
					}

					if (numFailures > 6 && random.Next(0, 3) == 0)
					{
						if (withDebugOutput)
						{
							Console.WriteLine($"Break, we got {numFailures} failures");
						}
						break;
					}

					if (numFailures > 4 && random.Next(0, 5) == 0)
					{
						if (withDebugOutput)
						{
							Console.WriteLine($"Break, we got {numFailures} failures");
						}
						break;
					}

					if (numFailures > 2 && random.Next(0, 7) == 0)
					{
						if (withDebugOutput)
						{
							Console.WriteLine($"Break, we got {numFailures} failures");
						}
						break;
					}
				}

				#endregion
				
				for (var j = 0; j < trialsPerCycle; j++)
				{
					iterationsCount++;
					var perturbedLayout = PerturbLayout(currentLayout, chain, out var energyDelta);

					OnPerturbed?.Invoke(ConvertLayout(perturbedLayout));

					// TODO: can we check the energy instead?
					if (IsLayoutValid(perturbedLayout))
					{
						OnValid?.Invoke(ConvertLayout(perturbedLayout));

						// TODO: wouldn't it be too slow to compare againts all?
						if (layouts.TrueForAll(x => GetDifference(perturbedLayout, x, chain) > 2 * minimumDifference))
						{
							wasAccepted = true;
							// AddDoors(new List<Layout>() { perturbedLayout });
							layouts.Add(perturbedLayout);
							OnValidAndDifferent?.Invoke(ConvertLayout(perturbedLayout));

							#region Debug output

							if (withDebugOutput)
							{
								Console.WriteLine($"Found layout, cycle {i}, trial {j}, energy {perturbedLayout.GetEnergy()}");
							}

							#endregion

							if (layouts.Count >= 15)
							{
								#region Debug output

								if (withDebugOutput)
								{
									Console.WriteLine($"Returning {layouts.Count} partial layouts");
								}

								#endregion

								return layouts;
							}
						}
					}

					var deltaAbs = Math.Abs(energyDelta);
					var accept = false;

					if (energyDelta > 0)
					{
						if (i == 0 && j == 0)
						{
							deltaEAvg = deltaAbs * 15;
						}

						var p = Math.Pow(Math.E, -deltaAbs / (deltaEAvg * t));
						if (random.NextDouble() < p)
							accept = true;
					}
					else
					{
						accept = true;
					}

					if (accept)
					{
						acceptedSolutions++;
						currentLayout = perturbedLayout;
						deltaEAvg = (deltaEAvg * (acceptedSolutions - 1) + deltaAbs) / acceptedSolutions;
					}

				}

				if (!wasAccepted)
				{
					numFailures++;
				}

				t = t * ratio;
			}

			#region Debug output

			if (withDebugOutput)
			{
				Console.WriteLine($"Returning {layouts.Count} partial layouts");
			}

			#endregion

			return layouts;
		}

		private Layout AddChainToLayout(Layout layout, List<int> chain)
		{
			layout = layout.Clone();

			foreach (var node in chain)
			{
				layoutOperations.AddNodeGreedily(layout, node);
			}

			layoutOperations.RecomputeValidityVectors(layout);
			layoutOperations.RecomputeEnergy(layout);

			return layout;
		}

		private Layout PerturbLayout(Layout layout, List<int> chain, out double energyDelta)
		{
			// TODO: sometimes perturb a node that is not in the current chain?

			var energy = layout.GetEnergy();
			var newLayout = random.NextDouble() <= shapePerturbChance ? layoutOperations.PerturbShape(layout, chain, true) : layoutOperations.PerturbPosition(layout, chain, true);

			// TODO: remove
			/*var expected = newLayout.Clone();
			layoutOperations.RecomputeEnergy(expected);
			layoutOperations.RecomputeValidityVectors(expected);

			foreach (var node in layout.Graph.Vertices)
			{
				if (!expected.GetConfiguration(node, out var c1) || !newLayout.GetConfiguration(node, out var c2))
					continue;

				if (!c1.Equals(c2))
				{
					var x = 1;
				}
			}*/

			var newEnergy = newLayout.GetEnergy();
			energyDelta = newEnergy - energy;

			return newLayout;
		}

		private bool IsLayoutValid(Layout layout)
		{
			return layout.GetEnergy() == 0; // TODO: may it cause problems?
		}

		private double GetDifference(Layout first, Layout second, List<int> chain = null)
		{
			var diff = 0f;

			foreach (var node in chain ?? first.Graph.Vertices)
			{
				if (first.GetConfiguration(node, out var c1) && second.GetConfiguration(node, out var c2))
				{
					diff += (float)(Math.Pow(
						IntVector2.ManhattanDistance(c1.Shape.BoundingRectangle.Center + c1.Position,
							c2.Shape.BoundingRectangle.Center + c2.Position), 2) * (ReferenceEquals(c1.Shape, c2.Shape) ? 1 : 4));
				}
			}

			/*for (var i = 0; i < first.Graph.VerticesCount; i++)
			{
				if (first.GetConfiguration(i, out var c1) && second.GetConfiguration(i, out var c2))
				{
					diff += (float)Math.Pow(
						IntVector2.ManhattanDistance(c1.Shape.BoundingRectangle.Center + c1.Position,
							c2.Shape.BoundingRectangle.Center + c2.Position), 2);
				}
			}*/

			return diff;
		}

		private IMapLayout<TNode> ConvertLayout(Layout layout)
		{
			var rooms = new List<IRoom<TNode>>();

			foreach (var vertex in layout.Graph.Vertices)
			{
				if (layout.GetConfiguration(vertex, out var configuration))
				{
					rooms.Add(new Room<TNode>(graph.GetVertex(vertex), configuration.Shape, configuration.Position));
				}
			}

			return new MapLayout<TNode>(rooms);
		}

		public void EnableDebugOutput(bool enable)
		{
			withDebugOutput = enable;
		}

		private struct LayoutNode
		{
			public Layout Layout;

			public int NumberOfChains;
		}

		public void InjectRandomGenerator(Random random)
		{
			this.random = random;
			layoutOperations?.InjectRandomGenerator(random);
		}

		long IBenchmarkable.TimeFirst => timeFirst;

		long IBenchmarkable.TimeTen => timeTen;

		int IBenchmarkable.IterationsCount => iterationsCount;

		int IBenchmarkable.LayoutsCount => layoutsCount;

		void IBenchmarkable.EnableBenchmark(bool enable)
		{
			BenchmarkEnabled = enable;
		}
	}
}