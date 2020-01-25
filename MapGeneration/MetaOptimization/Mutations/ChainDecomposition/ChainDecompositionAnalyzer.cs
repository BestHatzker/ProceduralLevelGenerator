﻿using System;
using System.Collections.Generic;
using System.Linq;
using MapGeneration.Core.ChainDecompositions;
using MapGeneration.Interfaces.Core.MapDescriptions;
using MapGeneration.Interfaces.Utils;
using MapGeneration.MetaOptimization.Configurations;
using MapGeneration.MetaOptimization.Evolution;
using MapGeneration.MetaOptimization.Stats;

namespace MapGeneration.MetaOptimization.Mutations.ChainDecomposition
{
    public class ChainDecompositionAnalyzer<TConfiguration, TNode, TGeneratorStats> : IPerformanceAnalyzer<TConfiguration, Individual<TConfiguration, IGeneratorEvaluation<TGeneratorStats>>>
        where TConfiguration : IChainDecompositionConfiguration<TNode>, ISmartCloneable<TConfiguration>, ISimulatedAnnealingConfiguration
        where TGeneratorStats : IChainsStats
        where TNode : IEquatable<TNode>
    {
        private readonly IMapDescription<TNode> mapDescription;

        public ChainDecompositionAnalyzer(IMapDescription<TNode> mapDescription)
        {
            this.mapDescription = mapDescription;
        }

        public List<IMutation<TConfiguration>> ProposeMutations(Individual<TConfiguration, IGeneratorEvaluation<TGeneratorStats>> individual)
        {
            var mutations = new List<IMutation<TConfiguration>>();

            // Do not apply this mutation multiple times
            if (individual.Mutations.All(x => x.GetType() != typeof(ChainDecompositionMutation<TConfiguration, TNode>)))
            {
                mutations.Add(GetMutation(6, true, true));
                mutations.Add(GetMutation(8, true, true));
                mutations.Add(GetMutation(10, true, true));
                mutations.Add(GetMutation(8, false, true));
                mutations.Add(GetMutation(8, true, false));
            }

            return mutations;
        }

        private IMutation<TConfiguration> GetMutation(int maxTreeSize, bool mergeSmallChains, bool startTreeWithMultipleVertices)
        {
            var chains =
                new TwoStageChainDecomposition<TNode>(mapDescription, new BetterBreadthFirstChainDecomposition<TNode>(maxTreeSize, mergeSmallChains, startTreeWithMultipleVertices))
                    .GetChains(mapDescription.GetGraph()).ToList();

            return new ChainDecompositionMutation<TConfiguration, TNode>(5, chains, true, maxTreeSize, mergeSmallChains, startTreeWithMultipleVertices);
        }
    }
}