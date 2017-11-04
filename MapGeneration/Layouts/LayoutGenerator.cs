﻿namespace MapGeneration.Layouts
{
	using System;
	using System.Collections.Generic;
	using ConfigurationSpaces;
	using DataStructures.Graphs;

	public class LayoutGenerator<TPolygon, TNode> : ILayoutGenerator<TPolygon, TNode> where TNode : IComparable<TNode>
	{
		private IConfigurationSpaces<TPolygon, TNode> configurationSpaces;

		private Random random = new Random();
		
		public LayoutGenerator(IConfigurationSpaces<TPolygon, TNode> configurationSpaces)
		{
			this.configurationSpaces = configurationSpaces;
		}

		public IList<ILayout<TPolygon>> GetLayouts(IGraph<TNode> graph, int minimumLayouts = 10)
		{
			var stack = new Stack<LayoutNode>();
			var fullLayouts = new List<ILayout<TPolygon>>();
			var graphChains = GetChains(graph);
			var initialLayout = configurationSpaces.GetInitialLayout(graphChains[0]);

			stack.Push(new LayoutNode() { Layout = initialLayout, NumberOfChains = 1 });

			while (stack.Count > 0)
			{
				var layoutNode = stack.Pop();
				var extendedLayouts = GetExtendedLayouts(layoutNode.Layout, graphChains[layoutNode.NumberOfChains]);

				if (layoutNode.NumberOfChains + 1 == graphChains.Count)
				{
					fullLayouts.AddRange(extendedLayouts);
				}
				else
				{
					foreach (var extendedLayout in extendedLayouts)
					{
						stack.Push(new LayoutNode(){ Layout = extendedLayout, NumberOfChains = layoutNode.NumberOfChains + 1 });
					}
				}

				if (fullLayouts.Count >= minimumLayouts)
				{
					break;
				}
			}

			return fullLayouts;
		}

		private List<ILayout<TPolygon>> GetExtendedLayouts(ILayout<TPolygon> layout, List<TNode> chain)
		{
			var t = 0d;
			var ratio = 0d;
			var cycles = 100;
			var trialsPerCycle = 100;
			var k = 1d;
			var minimumDifference = 0f;
				
			var layouts = new List<ILayout<TPolygon>>();
			var currentLayout = configurationSpaces.AddChain(layout, chain);

			for (var i = 0; i < cycles; i++)
			{
				for (var j = 0; j < trialsPerCycle; j++)
				{
					var perturbedLayout = configurationSpaces.PerturbLayout(currentLayout); // TODO: locally perturb the layout

					if (perturbedLayout.IsValid())
					{
						// TODO: wouldn't it be too slow to compare againts all?
						if (layouts.TrueForAll(x => x.GetDifference(perturbedLayout) > minimumDifference))
						{
							layouts.Add(perturbedLayout);
						}
					}

					var energyOriginal = currentLayout.GetEnergy();
					var energyPerturbed = perturbedLayout.GetEnergy();
					var energyDelta =  energyPerturbed - energyOriginal;

					if (energyDelta < 0)
					{
						currentLayout = perturbedLayout;
					} else if (random.NextDouble() < Math.Pow(Math.E, -energyDelta / (k * t)))
					{
						currentLayout = perturbedLayout;
					}
				}

				t = t * ratio;
			}

			return layouts;
		}

		private List<List<TNode>> GetChains(IGraph<TNode> graph)
		{
			throw new NotImplementedException();
		}

		private struct LayoutNode
		{
			public ILayout<TPolygon> Layout;

			public int NumberOfChains;
		}
	}
}
