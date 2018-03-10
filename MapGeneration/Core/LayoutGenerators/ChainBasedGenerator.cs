﻿namespace MapGeneration.Core.LayoutGenerators
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using ConfigurationSpaces;
	using Experimental;
	using GeneralAlgorithms.Algorithms.Graphs;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Polygons;
	using Interfaces.Benchmarks;
	using Interfaces.Core;
	using Interfaces.Core.ConfigurationSpaces;
	using Interfaces.Core.LayoutGenerator;
	using Interfaces.Core.MapDescription;

	/// <inheritdoc cref="ILayoutGenerator{TMapDescription,TNode}" />
	/// <summary>
	/// Chain based layout generator.
	/// </summary>
	/// <typeparam name="TMapDescription">Type of the map description.</typeparam>
	/// <typeparam name="TLayout">Type of the layout that will be used in the process.</typeparam>
	/// <typeparam name="TNode">Type of nodes in the map description.</typeparam>
	/// <typeparam name="TConfiguration">Type of configuration used.</typeparam>
	public class ChainBasedGenerator<TMapDescription, TLayout, TNode, TConfiguration> : IObservableGenerator<TMapDescription, TNode>, ICancellable, IBenchmarkable, IRandomInjectable
		where TMapDescription : IMapDescription<TNode>
		where TLayout : ILayout<TNode, TConfiguration>, ISmartCloneable<TLayout>
	{
		// Algorithms
		private IConfigurationSpaces<TNode, IntAlias<GridPolygon>, TConfiguration, ConfigurationSpace> configurationSpaces;
		private IChainDecomposition<TNode> chainDecomposition;
		private ILayoutEvolver<TLayout, TNode> layoutEvolver;
		private ILayoutOperations<TLayout, TNode> layoutOperations;
		private IGeneratorPlanner<TLayout> generatorPlanner;
		private ILayoutConverter<TLayout, IMapLayout<TNode>> layoutConverter;

		// Creators
		private Func<
			TMapDescription,
			IConfigurationSpaces<TNode, IntAlias<GridPolygon>, TConfiguration, ConfigurationSpace>
		> configurationSpacesCreator;

		private Func<
			TMapDescription,
			IChainDecomposition<TNode>
		> chainDecompositionCreator;

		private Func<
			TMapDescription,
			ILayoutOperations<TLayout, TNode>,
			ILayoutEvolver<TLayout, TNode>
		> layoutEvolverCreator;

		private Func<
			TMapDescription,
			IConfigurationSpaces<TNode, IntAlias<GridPolygon>, TConfiguration, ConfigurationSpace>,
			ILayoutOperations<TLayout, TNode>
		> layoutOperationsCreator;

		private Func<
			TMapDescription,
			IGeneratorPlanner<TLayout>
		> generatorPlannerCreator;

		private Func<
			TMapDescription,
			IConfigurationSpaces<TNode, IntAlias<GridPolygon>, TConfiguration, ConfigurationSpace>,
			ILayoutConverter<TLayout, IMapLayout<TNode>>
		> layoutConverterCreator;
		
		private Func<
			TMapDescription,
			TLayout
		> initialLayoutCreator;


		// Debug and benchmark variables
		private long timeFirst;
		private long timeTotal;
		private int layoutsCount;
		private readonly Stopwatch stopwatch = new Stopwatch();

		protected Random Random;
		protected CancellationToken? CancellationToken;

		private List<List<TNode>> chains;
		private TLayout initialLayout;
		private GeneratorContext context;

		// Settings
		protected bool BenchmarkEnabled;
		protected bool LayoutValidityCheckEnabled;

		// Events

		/// <inheritdoc />
		public event Action<IMapLayout<TNode>> OnPerturbed;

		/// <inheritdoc />
		public event Action<IMapLayout<TNode>> OnPartialValid;

		/// <inheritdoc />
		public event Action<IMapLayout<TNode>> OnValid;

		private readonly GraphUtils graphUtils = new GraphUtils();

		public IList<IMapLayout<TNode>> GetLayouts(TMapDescription mapDescription, int numberOfLayouts)
		{
			var graph = mapDescription.GetGraph();

			if (!graphUtils.IsConnected(graph))
				throw new ArgumentException("Given mapDescription must represent a connected graph.", nameof(mapDescription));

			if (!graphUtils.IsPlanar(graph))
				throw new ArgumentException("Given mapDescription must represent a planar graph.", nameof(mapDescription));

			// Create instances and inject the random generator and the cancellation token if possible
			configurationSpaces = configurationSpacesCreator(mapDescription);
			TryInjectRandomAndCancellationToken(configurationSpaces);

			chainDecomposition = chainDecompositionCreator(mapDescription);
			TryInjectRandomAndCancellationToken(chainDecomposition);

			initialLayout = initialLayoutCreator(mapDescription);
			TryInjectRandomAndCancellationToken(initialLayout);

			generatorPlanner = generatorPlannerCreator(mapDescription);
			TryInjectRandomAndCancellationToken(generatorPlanner);

			layoutOperations = layoutOperationsCreator(mapDescription, configurationSpaces);
			TryInjectRandomAndCancellationToken(layoutOperations);

			layoutConverter = layoutConverterCreator(mapDescription, configurationSpaces);
			TryInjectRandomAndCancellationToken(layoutConverter);

			layoutEvolver = layoutEvolverCreator(mapDescription, layoutOperations);
			TryInjectRandomAndCancellationToken(layoutEvolver);

			// Restart stopwatch
			stopwatch.Restart();

			chains = chainDecomposition.GetChains(graph);
			context = new GeneratorContext();

			RegisterEventHandlers();

			// TODO: handle number of layouts to be evolved - who should control that? generator or planner?
			// TODO: handle context.. this is ugly
			var layouts = generatorPlanner.Generate(initialLayout, numberOfLayouts, chains.Count,
				(layout, chainNumber, numOfLayouts) => layoutEvolver.Evolve(AddChain(layout, chainNumber), chains[chainNumber], numOfLayouts), context);

			// Stop stopwatch and prepare benchmark info
			stopwatch.Stop();
			timeTotal = stopwatch.ElapsedMilliseconds;
			layoutsCount = layouts.Count;

			// Reset cancellation token if it was already used
			if (CancellationToken.HasValue && CancellationToken.Value.IsCancellationRequested)
			{
				CancellationToken = null;
			}

			return layouts.Select(x => layoutConverter.Convert(x, true)).ToList();
		}

		/// <summary>
		/// Adds a next chain to a given layout.
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="chainNumber"></param>
		/// <returns></returns>
		protected virtual TLayout AddChain(TLayout layout, int chainNumber)
		{
			if (chainNumber >= chains.Count)
				throw new ArgumentException("Chain number is bigger than then number of chains.", nameof(chainNumber));

			layout = layout.SmartClone();

			layoutOperations.AddChain(layout, chains[chainNumber], true);

			return layout;
		}


		#region Event handlers

		private void RegisterEventHandlers()
		{
			// Register validity checks
			layoutEvolver.OnPerturbed -= CheckLayoutValidity;
			if (LayoutValidityCheckEnabled)
			{
				layoutEvolver.OnPerturbed += CheckLayoutValidity;
			}

			// Register iterations counting
			layoutEvolver.OnPerturbed -= IterationsCounterHandler;
			layoutEvolver.OnPerturbed += IterationsCounterHandler;

			layoutEvolver.OnPerturbed -= PerturbedLayoutsHandler;
			layoutEvolver.OnPerturbed += PerturbedLayoutsHandler;

			layoutEvolver.OnValid -= PartialValidLayoutsHandler;
			layoutEvolver.OnValid += PartialValidLayoutsHandler;

			// Setup first layout timer
			timeFirst = -1;
			generatorPlanner.OnLayoutGenerated -= FirstLayoutTimeHandler;
			generatorPlanner.OnLayoutGenerated += FirstLayoutTimeHandler;

			generatorPlanner.OnLayoutGenerated -= ValidLayoutsHandler;
			generatorPlanner.OnLayoutGenerated += ValidLayoutsHandler;
		}

		private void FirstLayoutTimeHandler(TLayout layout)
		{
			if (timeFirst == -1)
			{
				timeFirst = stopwatch.ElapsedMilliseconds;
			}
		}

		private void IterationsCounterHandler(TLayout layout)
		{
			context.IterationsCount++;
		}

		private void ValidLayoutsHandler(TLayout layout)
		{
			OnValid?.Invoke(layoutConverter.Convert(layout, true));
		}

		private void PartialValidLayoutsHandler(TLayout layout)
		{
			OnPartialValid?.Invoke(layoutConverter.Convert(layout, true));
		}

		private void PerturbedLayoutsHandler(TLayout layout)
		{
			OnPerturbed?.Invoke(layoutConverter.Convert(layout, false));
		}

		#endregion


		#region Creators

		/// <summary>
		/// Sets a function that can create a layout evolver.
		/// </summary>
		/// <remarks>
		/// Will be called on every call to GetLayouts().
		/// </remarks>
		/// <param name="creator"></param>
		public void SetLayoutEvolverCreator(Func<TMapDescription, ILayoutOperations<TLayout, TNode>, ILayoutEvolver<TLayout, TNode>> creator)
		{
			layoutEvolverCreator = creator;
		}

		/// <summary>
		/// Sets a function that can create a layout converter.
		/// </summary>
		/// <remarks>
		/// Will be called on every call to GetLayouts().
		/// </remarks>
		/// <param name="creator"></param>
		public void SetLayoutConverterCreator(Func<TMapDescription, IConfigurationSpaces<TNode, IntAlias<GridPolygon>, TConfiguration, ConfigurationSpace>, ILayoutConverter<TLayout, IMapLayout<TNode>>> creator)
		{
			layoutConverterCreator = creator;
		}

		/// <summary>
		/// Sets a function that can create a generator planner.
		/// </summary>
		/// <remarks>
		/// Will be called on every call to GetLayouts().
		/// </remarks>
		/// <param name="creator"></param>
		public void SetGeneratorPlannerCreator(Func<TMapDescription, IGeneratorPlanner<TLayout>> creator)
		{
			generatorPlannerCreator = creator;
		}

		/// <summary>
		/// Sets a function that can create an initial layout.
		/// </summary>
		/// <remarks>
		/// Will be called on every call to GetLayouts().
		/// </remarks>
		/// <param name="creator"></param>
		public void SetInitialLayoutCreator(Func<TMapDescription, TLayout> creator)
		{
			initialLayoutCreator = creator;
		}

		/// <summary>
		/// Sets a function that can create an instance of layout operations.
		/// </summary>
		/// <remarks>
		/// Will be called on every call to GetLayouts().
		/// </remarks>
		/// <param name="creator"></param>
		public void SetLayoutOperationsCreator(Func<TMapDescription, IConfigurationSpaces<TNode, IntAlias<GridPolygon>, TConfiguration, ConfigurationSpace>, ILayoutOperations<TLayout, TNode>> creator)
		{
			layoutOperationsCreator = creator;
		}

		/// <summary>
		/// Sets a function that can create configuration spaces.
		/// </summary>
		/// <remarks>
		/// Will be called on every call to GetLayouts().
		/// </remarks>
		/// <param name="creator"></param>
		public void SetConfigurationSpacesCreator(Func<TMapDescription, IConfigurationSpaces<TNode, IntAlias<GridPolygon>, TConfiguration, ConfigurationSpace>> creator)
		{
			configurationSpacesCreator = creator;
		}

		/// <summary>
		/// Sets a function that can create a chain decomposition.
		/// </summary>
		/// <remarks>
		/// Will be called on every call to GetLayouts().
		/// </remarks>
		/// <param name="creator"></param>
		public void SetChainDecompositionCreator(Func<TMapDescription, IChainDecomposition<TNode>> creator)
		{
			chainDecompositionCreator = creator;
		}

		#endregion


		/// <summary>
		/// Checks whether energies and validity vectors are the same as if they are all recomputed.
		/// </summary>
		/// <remarks>
		/// This check significantly slows down the generator.
		/// </remarks>
		/// <param name="enable"></param>
		public void SetLayoutValidityCheck(bool enable)
		{
			LayoutValidityCheckEnabled = enable;
		}

		private void CheckLayoutValidity(TLayout layout)
		{
			var copy = layout.SmartClone();

			layoutOperations.UpdateLayout(copy);

			foreach (var vertex in layout.Graph.Vertices)
			{
				var isInLayout = layout.GetConfiguration(vertex, out var configurationLayout);
				var isInCopy = copy.GetConfiguration(vertex, out var configurationCopy);

				if (isInLayout != isInCopy)
					throw new InvalidOperationException("Vertices must be either set in both or absent in both");

				// Skip the check if the configuration is not set
				if (!isInLayout)
					continue;

				if (!configurationCopy.Equals(configurationLayout))
					throw new InvalidOperationException("Configurations must be equal");
			}
		}

		/// <summary>
		/// Checks if a given object is IRandomInjectable and/or ICancellable
		/// and tries to inject a random generator or a cancellation token.
		/// </summary>
		/// <param name="o"></param>
		protected void TryInjectRandomAndCancellationToken(object o)
		{
			if (o is IRandomInjectable randomInjectable)
			{
				randomInjectable.InjectRandomGenerator(Random);
			}

			if (CancellationToken.HasValue && o is ICancellable cancellable)
			{
				cancellable.SetCancellationToken(CancellationToken.Value);
			}
		}

		/// <inheritdoc />
		long IBenchmarkable.TimeFirst => timeFirst;

		/// <inheritdoc />
		long IBenchmarkable.TimeTotal => timeTotal;

		/// <inheritdoc />
		int IBenchmarkable.IterationsCount => context.IterationsCount;

		/// <inheritdoc />
		int IBenchmarkable.LayoutsCount => layoutsCount;

		/// <inheritdoc />
		public void EnableBenchmark(bool enable)
		{
			BenchmarkEnabled = true;
		}

		/// <inheritdoc />
		public void InjectRandomGenerator(Random random)
		{
			Random = random;
		}

		/// <inheritdoc />
		public void SetCancellationToken(CancellationToken? cancellationToken)
		{
			CancellationToken = cancellationToken;
		}
	}
}