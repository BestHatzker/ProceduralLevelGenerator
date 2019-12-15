﻿using MapGeneration.Core.MapDescriptions;
using MapGeneration.Interfaces.Core.MapDescriptions;

namespace MapGeneration.Core.LayoutOperations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigurationSpaces;
	using GeneralAlgorithms.DataStructures.Common;
	using Interfaces.Core.Configuration;
	using Interfaces.Core.Configuration.EnergyData;
	using Interfaces.Core.ConfigurationSpaces;
	using Interfaces.Core.Constraints;
	using Interfaces.Core.Layouts;
	using Interfaces.Utils;
	using Utils;

	/// <summary>
	/// Layout operations that compute energy based on given constraints.
	/// </summary>
	public class LayoutOperationsWithConstraints<TLayout, TNode, TConfiguration, TShapeContainer, TEnergyData, TLayoutEnergyData> : AbstractLayoutOperations<TLayout, TNode, TConfiguration, TShapeContainer>
		where TLayout : IEnergyLayout<TNode, TConfiguration, TLayoutEnergyData>, ISmartCloneable<TLayout>
		where TConfiguration : IEnergyConfiguration<TShapeContainer, TEnergyData>, ISmartCloneable<TConfiguration>, new()
		where TEnergyData : IEnergyData, new()
		where TLayoutEnergyData : IEnergyData, new()
	{
		private readonly List<INodeConstraint<TLayout, TNode, TConfiguration, TEnergyData>> nodeConstraints = new List<INodeConstraint<TLayout, TNode, TConfiguration, TEnergyData>>();
		private readonly List<ILayoutConstraint<TLayout, TNode, TLayoutEnergyData>> layoutConstraints = new List<ILayoutConstraint<TLayout, TNode, TLayoutEnergyData>>();

        public LayoutOperationsWithConstraints(IConfigurationSpaces<TNode, TShapeContainer, TConfiguration, ConfigurationSpace> stageOneConfigurationSpaces, int averageSize, IMapDescription<TNode> mapDescription, IConfigurationSpaces<TNode, TShapeContainer, TConfiguration, ConfigurationSpace> stageTwoConfigurationSpaces) : base(stageOneConfigurationSpaces, averageSize, mapDescription, stageTwoConfigurationSpaces)
        {
        }

		/// <summary>
		/// Adds a constraint for nodes.
		/// </summary>
		/// <param name="constraint"></param>
		public void AddNodeConstraint(INodeConstraint<TLayout, TNode, TConfiguration, TEnergyData> constraint)
		{
			nodeConstraints.Add(constraint);
		}

		/// <summary>
		/// Adds a constraint for layouts.
		/// </summary>
		/// <param name="constraint"></param>
		public void AddLayoutConstraint(ILayoutConstraint<TLayout, TNode, TLayoutEnergyData> constraint)
		{
			layoutConstraints.Add(constraint);
		}

		/// <summary>
		/// Checks if a given layout is valid by first checking whether the layout itself is valid
		/// and then checking whether all configurations of nodes are valid.
		/// </summary>
		/// <param name="layout"></param>
		/// <returns></returns>
		public override bool IsLayoutValid(TLayout layout)
		{
			if (!layout.EnergyData.IsValid)
				return false;

			if (layout.GetAllConfigurations().Any(x => !x.EnergyData.IsValid))
				return false;

			return true;
		}

		/// <summary>
		/// TODO: should it check if all nodes are laid out?
		/// Checks if a given layout is valid by first checking whether the layout itself is valid
		/// and then checking whether all configurations of nodes are valid.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="chain"></param>
		/// <returns></returns>
		public override bool IsLayoutValid(TLayout layout, IList<TNode> chain)
		{
			return IsLayoutValid(layout);
		}

		/// <summary>
		/// Gets an energy of a given layout by summing energies of nodes and the energy of the layout itself.
		/// </summary>
		/// <param name="layout"></param>
		/// <returns></returns>
		public override float GetEnergy(TLayout layout)
		{
			return layout.GetAllConfigurations().Sum(x => x.EnergyData.Energy) + layout.EnergyData.Energy;
		}

		/// <summary>
		/// Updates a given layout by computing energies of all nodes and the energy of the layout iself.
		/// </summary>
		/// <remarks>
		/// Energies are computed from constraints.
		/// </remarks>
		/// <param name="layout"></param>
		public override void UpdateLayout(TLayout layout)
		{
			foreach (var node in layout.Graph.Vertices)
			{
				if (!layout.GetConfiguration(node, out var configuration))
					continue;

				var newEnergyData = NodeRunAllCompute(layout, node, configuration);
				configuration.EnergyData = newEnergyData;
				layout.SetConfiguration(node, configuration);
			}

			var layoutEnergyData = LayoutRunAllCompute(layout);
			layout.EnergyData = layoutEnergyData;
		}

		/// <summary>
		/// Tries all shapes and positions from the maximum intersection to find a configuration
		/// with the lowest energy.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="node"></param>
		public override void AddNodeGreedily(TLayout layout, TNode node)
		{
			var configurations = new List<TConfiguration>();
			var neighbours = layout.Graph.GetNeighbours(node);

			foreach (var neighbour in neighbours)
			{
				if (layout.GetConfiguration(neighbour, out var configuration))
				{
					configurations.Add(configuration);
				}
			}

			// The first node is set to have a random shape and [0,0] position
			if (configurations.Count == 0)
			{
				layout.SetConfiguration(node, CreateConfiguration(StageOneConfigurationSpaces.GetRandomShape(node), new IntVector2()));
				return;
			}

			var bestEnergy = float.MaxValue;
			var bestShape = default(TShapeContainer);
			var bestPosition = new IntVector2();

			var shapes = StageOneConfigurationSpaces.GetShapesForNode(node).ToList();
			shapes.Shuffle(Random);

			// Try all shapes
			foreach (var shape in shapes)
			{
				var intersection = StageOneConfigurationSpaces.GetMaximumIntersection(CreateConfiguration(shape, new IntVector2()), configurations);

				if (intersection == null)
					continue;

				intersection.Shuffle(Random);

				// Try all lines from the maximum intersection
				foreach (var intersectionLine in intersection)
				{
					// Limit the number of points to 20.
					// It is very slow to try all the positions if rooms are big.
					const int maxPoints = 20;

					if (intersectionLine.Length > maxPoints)
					{
						var mod = intersectionLine.Length / maxPoints - 1;

						for (var i = 0; i < maxPoints; i++)
						{
							var position = intersectionLine.GetNthPoint(i != maxPoints - 1 ? i * mod : intersectionLine.Length);

							var energy = NodeComputeEnergyData(layout, node, CreateConfiguration(shape, position)).Energy;

							if (energy < bestEnergy)
							{
								bestEnergy = energy;
								bestShape = shape;
								bestPosition = position;
							}

							if (bestEnergy <= 0)
							{
								break;
							}
						}
					}
					else
					{
						var points = intersectionLine.GetPoints();
						points.Shuffle(Random);

						foreach (var position in points)
						{
							var energy = NodeComputeEnergyData(layout, node, CreateConfiguration(shape, position)).Energy;

							if (energy < bestEnergy)
							{
								bestEnergy = energy;
								bestShape = shape;
								bestPosition = position;
							}

							if (bestEnergy <= 0)
							{
								break;
							}
						}
					}

					// There is no point of looking for more solutions when you already reached a valid state
					// and so no position would be accepted anyway
					if (bestEnergy <= 0)
					{
						break;
					}
				}
			}

			if (bestEnergy == float.MaxValue)
			{
				throw new ArgumentException("No shape for the current room could be connected to its neighbours");
			}

			var newConfiguration = CreateConfiguration(bestShape, bestPosition);
			layout.SetConfiguration(node, newConfiguration);
		}

		/// <summary>
		/// Creates a configuration with a given shape container and position.
		/// </summary>
		/// <param name="shapeContainer"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		protected TConfiguration CreateConfiguration(TShapeContainer shapeContainer, IntVector2 position)
		{
			var configuration = new TConfiguration
			{
				ShapeContainer = shapeContainer,
				Position = position
			};

			return configuration;
		}


		protected override void UpdateLayout(TLayout layout, TNode perturbedNode, TConfiguration configuration)
		{
			// Prepare new layout with temporary configuration to compute energies
			var graph = layout.Graph;
			var oldLayout = layout.SmartClone(); // TODO: is the clone needed?
			oldLayout.GetConfiguration(perturbedNode, out var oldConfiguration);

			// Update validity vectors and energies of vertices
			foreach (var vertex in graph.Vertices)
			{
				if (vertex.Equals(perturbedNode))
					continue;

				if (!layout.GetConfiguration(vertex, out var nodeConfiguration))
					continue;

				var vertexEnergyData = NodeRunAllUpdate(layout, perturbedNode, oldConfiguration, configuration, vertex, nodeConfiguration);

				nodeConfiguration.EnergyData = vertexEnergyData;
				layout.SetConfiguration(vertex, nodeConfiguration);
			}

			// Update energy and validity vector of the perturbed node
			var newEnergyData = NodeRunAllUpdate(perturbedNode, oldLayout, layout);
			configuration.EnergyData = newEnergyData;
			layout.SetConfiguration(perturbedNode, configuration);

			var layoutEnergyData = LayoutRunAllUpdate(perturbedNode, oldLayout, layout);
			layout.EnergyData = layoutEnergyData;
		}

		/// <summary>
		/// Computes energy data of a give node with a given configuration.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="node"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		protected TEnergyData NodeComputeEnergyData(TLayout layout, TNode node, TConfiguration configuration)
		{
			return NodeRunAllCompute(layout, node, configuration);
		}

		/// <summary>
		/// Run all constraints to compute energy data for a given node and configuration.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="node"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		private TEnergyData NodeRunAllCompute(TLayout layout, TNode node, TConfiguration configuration)
		{
			var energyData = new TEnergyData();
			var valid = true;

			foreach (var constraint in nodeConstraints)
			{
				if (!constraint.ComputeEnergyData(layout, node, configuration, ref energyData))
				{
					valid = false;
				}
			}

			energyData.IsValid = valid;
			return energyData;
		}

		/// <summary>
		/// Computes updated energy data of a given node by running all constraints.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="perturbedNode"></param>
		/// <param name="oldConfiguration"></param>
		/// <param name="newConfiguration"></param>
		/// <param name="node"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		private TEnergyData NodeRunAllUpdate(TLayout layout, TNode perturbedNode, TConfiguration oldConfiguration, TConfiguration newConfiguration, TNode node, TConfiguration configuration)
		{
			var energyData = new TEnergyData();
			var valid = true;

			foreach (var constraint in nodeConstraints)
			{
				if (!constraint.UpdateEnergyData(layout, perturbedNode, oldConfiguration, newConfiguration, node, configuration, ref energyData))
				{
					valid = false;
				}
			}

			energyData.IsValid = valid;
			return energyData;
		}

		/// <summary>
		/// Computes updated energy data of the node that was perturbed.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="oldLayout"></param>
		/// <param name="newLayout"></param>
		/// <returns></returns>
		private TEnergyData NodeRunAllUpdate(TNode node, TLayout oldLayout, TLayout newLayout)
		{
			var energyData = new TEnergyData();
			var valid = true;

			foreach (var constraint in nodeConstraints)
			{
				if (!constraint.UpdateEnergyData(oldLayout, newLayout, node, ref energyData))
				{
					valid = false;
				}
			}

			energyData.IsValid = valid;
			return energyData;
		}

		/// <summary>
		/// Computes energy data of a given layout.
		/// </summary>
		/// <param name="layout"></param>
		/// <returns></returns>
		private TLayoutEnergyData LayoutRunAllCompute(TLayout layout)
		{
			var energyData = new TLayoutEnergyData();
			var valid = true;

			foreach (var constraint in layoutConstraints)
			{
				if (!constraint.ComputeLayoutEnergyData(layout, ref energyData))
				{
					valid = false;
				}
			}

			energyData.IsValid = valid;
			return energyData;
		}

		/// <summary>
		/// Updates energy data of a given layout.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="oldLayout"></param>
		/// <param name="newLayout"></param>
		/// <returns></returns>
		private TLayoutEnergyData LayoutRunAllUpdate(TNode node, TLayout oldLayout, TLayout newLayout)
		{
			var energyData = new TLayoutEnergyData();
			var valid = true;

			foreach (var constraint in layoutConstraints)
			{
				if (!constraint.UpdateLayoutEnergyData(oldLayout, newLayout, node, ref energyData))
				{
					valid = false;
				}
			}

			energyData.IsValid = valid;
			return energyData;
		}


		/// <summary>
		/// Tries to add corridors.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="chain"></param>
		/// <returns></returns>
		public override bool TryCompleteChain(TLayout layout, IList<TNode> chain)
		{
			if (AddCorridors(layout, chain))
			{
				UpdateLayout(layout);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Greedily adds corridors from a given chain to the layout.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="chain"></param>
		/// <returns></returns>
		private bool AddCorridors(TLayout layout, IEnumerable<TNode> chain)
		{
			var clone = layout.SmartClone();
			var corridors = chain.Where(x => MapDescription.GetRoomDescription(x).Stage == 2).ToList();

			foreach (var corridor in corridors)
			{
				if (!AddCorridorGreedily(clone, corridor))
					return false;
			}

			foreach (var corridor in corridors)
			{
				clone.GetConfiguration(corridor, out var configuration);
				layout.SetConfiguration(corridor, configuration);
			}

			return true;
		}

        /// <summary>
        /// Greedily adds only non corridor nodes to the layout.
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="chain"></param>
        /// <param name="updateLayout"></param>
        public override void AddChain(TLayout layout, IList<TNode> chain, bool updateLayout)
        {
            var rooms = chain.Where(x => MapDescription.GetRoomDescription(x).Stage == 1);

            foreach (var room in rooms)
            {
                AddNodeGreedily(layout, room);
            }

            if (updateLayout)
            {
                UpdateLayout(layout);
            }
        }

		/// <summary>
		/// Adds corridor node greedily.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool AddCorridorGreedily(TLayout layout, TNode node)
		{
			var configurations = new List<TConfiguration>();
			var neighbors = layout.Graph.GetNeighbours(node);

			foreach (var neighbor in neighbors)
			{
				if (layout.GetConfiguration(neighbor, out var configuration))
				{
					configurations.Add(configuration);
				}
			}

			if (configurations.Count == 0)
			{
				throw new InvalidOperationException();
			}

			var foundValid = false;
			var bestShape = default(TShapeContainer);
			var bestPosition = new IntVector2();

			var shapes = StageTwoConfigurationSpaces.GetShapesForNode(node).ToList();
			shapes.Shuffle(Random);

			foreach (var shape in shapes)
			{
				var intersection = StageTwoConfigurationSpaces.GetMaximumIntersection(CreateConfiguration(shape, new IntVector2()), configurations, out var configurationsSatisfied);

				if (configurationsSatisfied != 2)
					continue;

				intersection.Shuffle(Random);

				foreach (var intersectionLine in intersection)
				{
					const int maxPoints = 20;

					if (intersectionLine.Length > maxPoints)
					{
						var mod = intersectionLine.Length / maxPoints - 1;

						for (var i = 0; i < maxPoints; i++)
						{
							var position = intersectionLine.GetNthPoint(i != maxPoints - 1 ? i * mod : intersectionLine.Length + 1);

							var energyData = NodeComputeEnergyData(layout, node, CreateConfiguration(shape, position));

							if (energyData.IsValid)
							{
								bestShape = shape;
								bestPosition = position;
								foundValid = true;
								break;
							}

							if (foundValid)
							{
								break;
							}
						}
					}
					else
					{
						var points = intersectionLine.GetPoints();
						points.Shuffle(Random);

						foreach (var position in points)
						{
							var energyData = NodeComputeEnergyData(layout, node, CreateConfiguration(shape, position));

							if (energyData.IsValid)
							{
								bestShape = shape;
								bestPosition = position;
								foundValid = true;
								break;
							}

							if (foundValid)
							{
								break;
							}
						}
					}

					if (foundValid)
					{
						break;
					}
				}
			}

			var newConfiguration = CreateConfiguration(bestShape, bestPosition);
			layout.SetConfiguration(node, newConfiguration);

			return foundValid;
		}

        /// <summary>
        /// Perturbs non corridor rooms until a valid layout is found.
        /// Then tries to use a greedy algorithm to lay out corridor rooms.
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="chain"></param>
        /// <param name="updateLayout"></param>
        public override void PerturbLayout(TLayout layout, IList<TNode> chain, bool updateLayout)
        {
			// TODO: change
            var nonCorridors = chain.Where(x => MapDescription.GetRoomDescription(x).Stage == 1).ToList();

            if (Random.NextDouble() < 0.4f)
            {
                PerturbShape(layout, nonCorridors, updateLayout);
            }
            else
            {
                var random = nonCorridors.GetRandom(Random);
                PerturbPosition(layout, random, updateLayout);
            }
        }
	}
}