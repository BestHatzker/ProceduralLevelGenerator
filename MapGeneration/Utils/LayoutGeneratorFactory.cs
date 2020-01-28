﻿using MapGeneration.Core.LayoutEvolvers.SimulatedAnnealing;
using MapGeneration.Interfaces.Core.LayoutGenerator;
using MapGeneration.Core.Configurations.EnergyData;
using MapGeneration.Interfaces.Utils;

namespace MapGeneration.Utils
{
	using System.Collections.Generic;
	using Core.ChainDecompositions;
	using Core.Configurations;
    using Core.ConfigurationSpaces;
	using Core.Constraints;
	using Core.Doors;
	using Core.GeneratorPlanners;
	using Core.LayoutConverters;
	using Core.LayoutEvolvers;
	using Core.LayoutGenerators;
	using Core.LayoutOperations;
	using Core.Layouts;
	using Core.MapDescriptions;
	using GeneralAlgorithms.Algorithms.Common;
	using GeneralAlgorithms.Algorithms.Polygons;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Polygons;
	using Interfaces.Core.MapLayouts;

	public static class LayoutGeneratorFactory
	{
        /// <summary>
        /// Gets a basic layout generator that should not be used to generated layouts with corridors.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <returns></returns>
        public static SimpleChainBasedGenerator<MapDescriptionOld<TNode>, Layout<Configuration<CorridorsData>>, IMapLayout<TNode>, int> GetSimpleChainBasedGenerator<TNode>(MapDescriptionOld<TNode> mapDescriptionOld, bool withCorridors, List<int> offsets = null, bool canTouch = false)
        {
            var chainDecomposition = new TwoStageChainDecomposition<int>(mapDescriptionOld, new BreadthFirstChainDecompositionOld<int>());
            var chains = chainDecomposition.GetChains(mapDescriptionOld.GetGraph());

            var generatorPlanner = new GeneratorPlanner<Layout<Configuration<CorridorsData>>, int>();

            var configurationSpacesGenerator = new ConfigurationSpacesGeneratorOld(
                new PolygonOverlap(),
                DoorHandler.DefaultHandler,
                new OrthogonalLineIntersection(),
                new GridPolygonUtils());
            var configurationSpaces = configurationSpacesGenerator.Generate<TNode, Configuration<CorridorsData>>(mapDescriptionOld);
            var corridorConfigurationSpaces = withCorridors ? configurationSpacesGenerator.Generate<TNode, Configuration<CorridorsData>>(mapDescriptionOld, offsets) : configurationSpaces;

            var layoutOperations = new LayoutOperationsWithConstraints<Layout<Configuration<CorridorsData>>, int, Configuration<CorridorsData>, IntAlias<GridPolygon>, CorridorsData>(corridorConfigurationSpaces, configurationSpaces.GetAverageSize(), mapDescriptionOld, configurationSpaces);

            var initialLayout = new Layout<Configuration<CorridorsData>>(mapDescriptionOld.GetGraph());
            var layoutConverter =
                new BasicLayoutConverterOld<Layout<Configuration<CorridorsData>>, TNode,
                    Configuration<CorridorsData>>(mapDescriptionOld, configurationSpaces,
                    configurationSpacesGenerator.LastIntAliasMapping);

            var averageSize = configurationSpaces.GetAverageSize();

            layoutOperations.AddNodeConstraint(new BasicContraint<Layout<Configuration<CorridorsData>>, int, Configuration<CorridorsData>, CorridorsData, IntAlias<GridPolygon>>(
                new FastPolygonOverlap(),
                averageSize,
                configurationSpaces
            ));

            if (withCorridors)
            {
                layoutOperations.AddNodeConstraint(new CorridorConstraintsOld<Layout<Configuration<CorridorsData>>, int, Configuration<CorridorsData>, CorridorsData, IntAlias<GridPolygon>>(
                    mapDescriptionOld,
                    averageSize,
                    corridorConfigurationSpaces
                ));

                if (!canTouch)
                {
                    var polygonOverlap = new FastPolygonOverlap();
                    layoutOperations.AddNodeConstraint(new TouchingConstraintsOld<Layout<Configuration<CorridorsData>>, int, Configuration<CorridorsData>, CorridorsData, IntAlias<GridPolygon>>(
                        mapDescriptionOld,
                        polygonOverlap
                    ));
                }
            }

            var layoutEvolver =
                    new SimulatedAnnealingEvolver<Layout<Configuration<CorridorsData>>, int,
                    Configuration<CorridorsData>>(layoutOperations, SimulatedAnnealingConfiguration.GetDefaultConfiguration(), true);

            var layoutGenerator = new SimpleChainBasedGenerator<MapDescriptionOld<TNode>, Layout<Configuration<CorridorsData>>, IMapLayout<TNode>, int>(initialLayout, generatorPlanner, chains, layoutEvolver, layoutConverter);

            layoutGenerator.OnRandomInjected += (random) =>
            {
                ((IRandomInjectable) configurationSpaces).InjectRandomGenerator(random);
                ((IRandomInjectable) layoutOperations).InjectRandomGenerator(random);
                ((IRandomInjectable) layoutEvolver).InjectRandomGenerator(random);
                ((IRandomInjectable) layoutConverter).InjectRandomGenerator(random);
            };

            layoutGenerator.OnCancellationTokenInjected += (token) =>
            {
                ((ICancellable) generatorPlanner).SetCancellationToken(token);
                ((ICancellable) layoutEvolver).SetCancellationToken(token);
            };

            return layoutGenerator;
        }
    }
}