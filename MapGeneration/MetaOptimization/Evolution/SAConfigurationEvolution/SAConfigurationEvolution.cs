﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MapGeneration.Benchmarks;
using MapGeneration.Benchmarks.GeneratorRunners;
using MapGeneration.Benchmarks.ResultSaving;
using MapGeneration.Core.LayoutEvolvers.SimulatedAnnealing;
using MapGeneration.Core.LayoutGenerators.DungeonGenerator;
using MapGeneration.Core.MapDescriptions;
using MapGeneration.Interfaces.Benchmarks;
using MapGeneration.MetaOptimization.Mutations;
using MapGeneration.MetaOptimization.Visualizations;
using MapGeneration.Utils.MapDrawing;

namespace MapGeneration.MetaOptimization.Evolution.SAConfigurationEvolution
{
    public class SAConfigurationEvolution : ConfigurationEvolution<DungeonGeneratorConfiguration, Individual>
    {
        private readonly GeneratorInput<MapDescriptionOld<int>> generatorInput;
        private readonly BenchmarkRunner<MapDescriptionOld<int>> benchmarkRunner = BenchmarkRunner.CreateForNodeType<int>();
        private readonly SVGLayoutDrawer<int> layoutDrawer = new SVGLayoutDrawer<int>();

        public SAConfigurationEvolution(
            GeneratorInput<MapDescriptionOld<int>> generatorInput,
            List<IPerformanceAnalyzer<DungeonGeneratorConfiguration, Individual>> analyzers, EvolutionOptions options)
            : base(analyzers, options, GetResultsDirectory(generatorInput))
        {
            this.generatorInput = generatorInput;
        }

        private static string GetResultsDirectory(GeneratorInput<MapDescriptionOld<int>> generatorInput)
        {
            return $"SAEvolutions/{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}_{generatorInput.Name}/";
        }

        protected override Individual EvaluateIndividual(Individual individual)
        {
            Logger.Write($"Evaluating individual {individual}");

            var scenario = BenchmarkScenario.CreateCustomForNodeType<int>(
                "SimulatedAnnealingParameters",
                input =>
                {
                    var layoutGenerator = new DungeonGenerator<int>(input.MapDescription, individual.Configuration);
                    layoutGenerator.InjectRandomGenerator(new Random(0));

                    var generatorRunner = new LambdaGeneratorRunner(() =>
                    {
                        var simulatedAnnealingArgsContainer = new List<SimulatedAnnealingEventArgs>();
                        var iterations = 0;
                        var cts = new CancellationTokenSource();
                        layoutGenerator.SetCancellationToken(cts.Token);

                        void SimulatedAnnealingEventHandler(object sender, SimulatedAnnealingEventArgs eventArgs)
                        {
                            iterations += eventArgs.IterationsSinceLastEvent;
                            simulatedAnnealingArgsContainer.Add(eventArgs);

                            if (individual.Parent != null && iterations > 10 * individual.Parent.Fitness)
                            {
                                cts.Cancel();
                            }
                        }

                        layoutGenerator.OnSimulatedAnnealingEvent += SimulatedAnnealingEventHandler;
                        var layout = layoutGenerator.GenerateLayout();
                        layoutGenerator.OnSimulatedAnnealingEvent -= SimulatedAnnealingEventHandler;

                        var additionalData = new AdditionalRunData()
                        {
                            SimulatedAnnealingEventArgs = simulatedAnnealingArgsContainer,
                            GeneratedLayout = layout,
                        };

                        var generatorRun = new GeneratorRun<AdditionalRunData>(layout != null, layoutGenerator.TimeTotal, layoutGenerator.IterationsCount, additionalData);

                        return generatorRun;
                    });

                    if (individual.Parent != null)
                    {
                        return new EarlyStoppingGeneratorRunner(generatorRunner, individual.Parent.Fitness, (successful, time, iterations) => new GeneratorRun<AdditionalRunData>(successful, time, iterations, null));
                    }
                    else
                    {
                        return generatorRunner;
                    }
                });

            var scenarioResult = benchmarkRunner.Run(scenario, new List<GeneratorInput<MapDescriptionOld<int>>>() { generatorInput }, 250, new BenchmarkOptions()
            {
                WithConsoleOutput = false,
            });
            var generatorRuns = scenarioResult
                .InputResults
                .First()
                .Runs
                .Cast<IGeneratorRun<AdditionalRunData>>()
                .ToList();

            var generatorEvaluation = new GeneratorEvaluation(generatorRuns); // TODO: ugly
            individual.ConfigurationEvaluation = generatorEvaluation;
            individual.Fitness = generatorRuns.Average(x => x.Iterations);
            individual.SuccessRate = generatorRuns.Count(x => x.IsSuccessful) / (double)generatorRuns.Count;

            Logger.WriteLine($" - fitness {individual.Fitness}, success rate {individual.SuccessRate * 100:F}%");

            Directory.CreateDirectory($"{ResultsDirectory}/{individual.Id}");

            for (int i = 0; i < generatorRuns.Count; i++)
            {
                var generatorRun = generatorRuns[i];

                if (generatorRun.IsSuccessful)
                {
                    var layout = generatorRun.AdditionalData.GeneratedLayout;
                    var svg = layoutDrawer.DrawLayout(layout, 800);
                    File.WriteAllText($"{ResultsDirectory}/{individual.Id}/{i}.svg", svg);
                    generatorRun.AdditionalData.GeneratedLayout = null;
                    generatorRun.AdditionalData.GeneratedLayoutSvg = svg;
                }
            }

            var resultSaver = new BenchmarkResultSaver();
            resultSaver.SaveResult(scenarioResult, $"{individual.Id}_benchmarkResults", ResultsDirectory, withDatetime: false);

            using (var file =
                new StreamWriter($@"{ResultsDirectory}{individual.Id}_visualization.txt"))
            {
                var dataVisualization = new ChainStatsVisualization<GeneratorData>();
                dataVisualization.Visualize(generatorEvaluation, file);
            }

            return individual;
        }

        protected override Individual CreateInitialIndividual(int id, DungeonGeneratorConfiguration configuration)
        {
            return new Individual(id, configuration);
        }

        protected override Individual CreateIndividual(int id, Individual parent, IMutation<DungeonGeneratorConfiguration> mutation)
        {
            return new Individual(id, parent, mutation);
        }
    }
}