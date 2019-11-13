﻿namespace MapGeneration.Core.ChainDecompositions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GeneralAlgorithms.DataStructures.Graphs;
	using Interfaces.Core.ChainDecompositions;
	using Interfaces.Core.MapDescriptions;

	/// <inheritdoc />
	/// <summary>
	/// Chain decomposition for layout generators with two-stage generation.
	/// </summary>
    /// <typeparam name="TNode"></typeparam>
	public class TwoStageChainDecomposition<TNode> : IChainDecomposition<TNode>
	{
		private readonly ICorridorMapDescription<TNode> mapDescription;
		private readonly IChainDecomposition<TNode> decomposition;

		public TwoStageChainDecomposition(ICorridorMapDescription<TNode> mapDescription, IChainDecomposition<TNode> decomposition)
		{
			this.mapDescription = mapDescription;
			this.decomposition = decomposition;
		}

		/// <inheritdoc />
		public List<List<TNode>> GetChains(IGraph<TNode> graph)
		{
            // Get all the faces from the stage one graph
			var stageOneGraph = mapDescription.GetGraphWithoutCorrridors();
			var faces = decomposition.GetChains(stageOneGraph);

			var usedVertices = new HashSet<TNode>();
			var notUsedStageTwoRooms = graph.Vertices.Where(x => mapDescription.GetRoomDescription(x).Stage == 2).ToList();

            // Iterate through all the faces, marking all the seen vertices
            // As soon as all the neighbors of a stage two room are used, add the stage two room to the current face
			foreach (var face in faces)
			{
				face.ForEach(x => usedVertices.Add(x));

				foreach (var stageTwoRoom in notUsedStageTwoRooms.ToList())
				{
					var neighbors = graph.GetNeighbours(stageTwoRoom).ToList();

                    if (neighbors.TrueForAll(x => usedVertices.Contains(x)))
                    {
                        notUsedStageTwoRooms.Remove(stageTwoRoom);
                        face.Add(stageTwoRoom);
                    }
                }
            }

            // It must not happen that a stage two room is not in the decomposition
			if (notUsedStageTwoRooms.Count != 0)
				throw new ArgumentException();

			return faces;
		}
	}
}