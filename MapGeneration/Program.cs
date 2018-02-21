﻿namespace MapGeneration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Benchmarks;
	using Core;
	using Core.ConfigurationSpaces;
	using Core.Doors;
	using Core.Doors.DoorModes;
	using Core.GraphDecomposition;
	using Core.Interfaces;
	using GeneralAlgorithms.Algorithms.Common;
	using GeneralAlgorithms.Algorithms.Graphs.GraphDecomposition;
	using GeneralAlgorithms.Algorithms.Polygons;
	using Utils;
	using Utils.ConfigParsing;
	using Utils.ConfigParsing.Deserializers;
	using Utils.ConfigParsing.Models;
	using YamlDotNet.RepresentationModel;
	using YamlDotNet.Serialization;
	using YamlDotNet.Serialization.NamingConventions;
	using YamlDotNet.Serialization.NodeDeserializers;

	internal class Program
	{
		private static void Main(string[] args)
		{
			var benchmark = new Benchmark();

			using (var sw = new StreamWriter(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + ".txt"))
			{
				var layoutGenerator = new SALayoutGenerator<int>();
				layoutGenerator.InjectRandomGenerator(new Random(0));
				layoutGenerator.SetLayoutValidityCheck(false);

				var scenario = new BenchmarkScenario<SALayoutGenerator<int>, int>();
				scenario.SetRunsCount(1);
				
				var setups2 = scenario.MakeSetupsGroup();
				setups2.AddSetup("Lazy", generator => { generator.EnableLazyProcessing(true);});
				//setups2.AddSetup("Not lazy", generator => { generator.EnableLazyProcessing(false); });

				//var setups3 = scenario.MakeSetupsGroup();
				//setups3.AddSetup("Perturb pos", generator => { generator.EnablePerturbPositionAfterShape(true);});
				//setups3.AddSetup("No perturb", generator => { generator.EnablePerturbPositionAfterShape(false);});

				var setups4 = scenario.MakeSetupsGroup();
				//for (var i = 0; i < 10; i++)
				//{
				//	var sigma = (int)Math.Pow(10, i);
				//	setups4.AddSetup($"Sigma avg {sigma}", generator => { generator.EnableSigmaFromAvg(true, sigma); });
				//}
				//setups4.AddSetup("Sigma constant", generator => { generator.EnableSigmaFromAvg(false); });
				setups4.AddSetup($"Sigma avg {100}", generator => { generator.EnableSigmaFromAvg(true, 10); });

				var setups6 = scenario.MakeSetupsGroup();
				//for (var i = 1; i < 10; i++)
				//{
				//	var chance = i * 0.03f;
				//	setups6.AddSetup($"Perturb outside with chance {chance}", generator => { generator.EnablePerturbOutsideChain(true, chance); });
				//}
				setups6.AddSetup("Perturb inside", generator => { generator.EnablePerturbOutsideChain(false); });

				{
					// Chain decomposition
					//var setups = scenario.MakeSetupsGroup();
					//setups.AddSetup("Breadth first", generator => { generator.SetChainDecomposition(new BreadthFirstLongerChainsDecomposition<int>(new GraphDecomposer<int>())); });
					//// setups.AddSetup("Handmade decomposition", generator => { generator.SetChainDecomposition(new DummyChainsDecomposition()); });
					//setups.AddSetup("Basic decomposition", generator => { generator.SetChainDecomposition(new BasicChainsDecomposition<int>(new GraphDecomposer<int>())); });
					//setups.AddSetup("Longer chains", generator => { generator.SetChainDecomposition(new LongerChainsDecomposition<int>(new GraphDecomposer<int>())); });
				}

				{
					// Random restarts
					//var setups = scenario.MakeSetupsGroup();
					//setups.AddSetup("Without random restart", generator => { generator.SetRandomRestarts(false); });

					//foreach (var place in Enum.GetValues(typeof(SALayoutGenerator<int>.RestartSuccessPlace)).Cast<SALayoutGenerator<int>.RestartSuccessPlace>())
					//{
					//	for (var j = 0; j < 2; j++)
					//	{
					//		var reset = j == 0;

					//		for (var i = 0; i < 5; i++)
					//		{
					//			var scale = (i + 1) * 0.5f + 0.5f;

					//			setups.AddSetup($"With random restarts - {place}, reset: {reset}, scale: {scale}", generator => { generator.SetRandomRestarts(true, place, reset, scale); });
					//		}
					//	}
					//}

					// setups.AddSetup("Test", generator => { generator.SetRandomRestarts(true, SALayoutGenerator<int>.RestartSuccessPlace.OnValidAndDifferent, true, 1.5f);});
					// setups.AddSetup("As original", generator => { generator.SetRandomRestarts(true, SALayoutGenerator<int>.RestartSuccessPlace.OnValidAndDifferent, false, 1f); });
				}

				{
					// Difference from average size
					//var setups = scenario.MakeSetupsGroup();
					//setups.AddSetup("Difference old", generator => { generator.SetDifferenceFromAverageSize(false); });

					//for (var i = 1; i < 10; i++)
					//{
					//	var scale = (float) (i * 0.2);
					//	setups.AddSetup($"Difference scale {scale}", generator => { generator.SetDifferenceFromAverageSize(true, scale); });
					//}

					//for (var i = 1; i < 6; i++)
					//{
					//	var scale = (float) Math.Pow(0.1, i);
					//	setups.AddSetup($"Difference scale {scale}", generator => { generator.SetDifferenceFromAverageSize(true, scale); });
					//}

					//for (var i = 1; i < 6; i++)
					//{
					//	var scale = (float)Math.Pow(10, i);
					//	setups.AddSetup($"Difference scale {scale}", generator => { generator.SetDifferenceFromAverageSize(true, scale); });
					//}
				}

				benchmark.Execute(layoutGenerator, scenario, MapDescriptionsDatabase.ReferenceSet, 80, sw);
			}
		}
	}
}
