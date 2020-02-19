﻿using System;
using System.Collections.Generic;
using System.Linq;
using MapGeneration.Core.MapDescriptions;
using MapGeneration.Core.MapDescriptions.Interfaces;
using MapGeneration.Core.MapLayouts;

namespace MapGeneration.Utils.Statistics
{
    public class EntropyCalculator
    {
        public double ComputeAverageRoomTemplatesEntropy<TNode>(IMapDescription<TNode> mapDescription, List<MapLayout<TNode>> layouts, bool normalize = true)
        {
            return mapDescription
                .GetGraph()
                .Vertices
                .Where(x => mapDescription.GetRoomDescription(x).RoomTemplates.Count > 1)
                .Select(x => ComputeRoomTemplatesEntropy(mapDescription, layouts, x, normalize))
                .Average();
        }

        public double ComputeRoomTemplatesEntropy<TNode>(IMapDescription<TNode> mapDescription, List<MapLayout<TNode>> layouts, TNode node, bool normalize = true)
        {
            var distribution = GetRoomTemplatesDistribution(mapDescription, layouts, node);
            var entropy = ComputeEntropy(distribution, normalize);

            return entropy;
        }

        public Dictionary<RoomTemplate, double> GetRoomTemplatesDistribution<TNode>(IMapDescription<TNode> mapDescription, List<MapLayout<TNode>> layouts, TNode node)
        {
            var roomDescription = mapDescription.GetRoomDescription(node);
            var availableRoomTemplates = roomDescription.RoomTemplates;
            var data = layouts
                .Select(x => x.Rooms.Single(y => y.Node.Equals(node)))
                .Select(x => x.RoomTemplate)
                .ToList();

            return GetRoomTemplatesDistribution(data, availableRoomTemplates);
        }

        public Dictionary<RoomTemplate, double> GetRoomTemplatesDistribution(List<RoomTemplate> data, List<RoomTemplate> availableRoomTemplates)
        {
            var counts = new Dictionary<RoomTemplate, int>();

            foreach (var roomTemplate in availableRoomTemplates)
            {
                counts.Add(roomTemplate, 0);
            }

            foreach (var roomTemplate in data)
            {
                counts[roomTemplate] += 1;
            }

            return counts
                .Keys
                .ToDictionary(x => x, x => (double) counts[x] / data.Count);
        }

        public double ComputeEntropy<TElement>(Dictionary<TElement, double> distribution, bool normalize = false)
        {
            if (distribution == null) throw new ArgumentNullException(nameof(distribution));

            if (distribution.Count == 0)
            {
                throw new ArgumentException("Distribution must not be empty", nameof(distribution));
            }

            var entropy = -1 * 
                distribution
                .Values
                .Where(x => x != 0)
                .Sum(x => x * Math.Log(x, 2));

            if (normalize && distribution.Count != 1)
            {
                var maximumEntropy = Math.Log(distribution.Count, 2);
                var normalizedEntropy = entropy / maximumEntropy;

                return normalizedEntropy;
            }

            return entropy;
        }
    }
}