﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeneralAlgorithms.Algorithms.Common;
using GeneralAlgorithms.Algorithms.Polygons;
using GeneralAlgorithms.DataStructures.Common;
using GeneralAlgorithms.DataStructures.Polygons;
using MapGeneration.Core.Doors;
using MapGeneration.Core.MapDescriptions;
using MapGeneration.Interfaces.Core.Configuration;
using MapGeneration.Interfaces.Core.ConfigurationSpaces;
using MapGeneration.Interfaces.Core.Doors;
using MapGeneration.Interfaces.Core.MapDescriptions;
using MapGeneration.Utils;

namespace MapGeneration.Core.ConfigurationSpaces
{
    public class ConfigurationSpacesGenerator
    {
        private readonly IPolygonOverlap<GridPolygon> polygonOverlap;
        private readonly IDoorHandler doorHandler;
        private readonly ILineIntersection<OrthogonalLine> lineIntersection;
        private readonly IPolygonUtils<GridPolygon> polygonUtils;

        public ConfigurationSpacesGenerator(IPolygonOverlap<GridPolygon> polygonOverlap, IDoorHandler doorHandler, ILineIntersection<OrthogonalLine> lineIntersection, IPolygonUtils<GridPolygon> polygonUtils)
        {
            this.polygonOverlap = polygonOverlap;
            this.doorHandler = doorHandler;
            this.lineIntersection = lineIntersection;
            this.polygonUtils = polygonUtils;
        }

        public ConfigurationSpaces<TConfiguration> GetConfigurationSpaces<TConfiguration>(IMapDescription<int> mapDescription, List<int> offsets = null)
            where TConfiguration : IConfiguration<IntAlias<GridPolygon>, int>
        {
            var graph = mapDescription.GetGraph();
            var stageOneGraph = mapDescription.GetStageOneGraph();

            var roomDescriptions = graph
                .Vertices
                .ToDictionary(x => x, mapDescription.GetRoomDescription);

            var roomTemplates = roomDescriptions
                .Values
                .Where(x => x.GetType() == typeof(BasicRoomDescription))
                .Cast<BasicRoomDescription>()
                .SelectMany(x => x.RoomTemplates)
                .Distinct()
                .ToList();

            var roomTemplateInstances = roomTemplates
                .ToDictionary(x => x, GetRoomTemplateInstances);

            var roomTemplateInstancesMapping = roomTemplateInstances
                .SelectMany(x => x.Value)
                .CreateIntMapping();

            var roomTemplateInstancesCount = roomTemplateInstancesMapping.Count;

            // Generate configuration spaces
            var configurationSpaces = new ConfigurationSpace[roomTemplateInstancesCount][];
            for (var i = 0; i < roomTemplateInstancesMapping.Count; i++)
            {
                var shape1 = roomTemplateInstancesMapping.GetByValue(i);
                configurationSpaces[i] = new ConfigurationSpace[roomTemplateInstancesCount];

                for (var j = 0; j < roomTemplateInstancesCount; j++)
                {
                    var shape2 = roomTemplateInstancesMapping.GetByValue(j);

                    configurationSpaces[i][j] = GetConfigurationSpace(shape1, shape2, offsets);
                }
            }

            // Prepare shapes for individual nodes
            var intAliases = roomTemplateInstancesMapping
                .Keys
                .ToDictionary(x => x, x => new IntAlias<GridPolygon>(roomTemplateInstancesMapping[x], x.RoomShape))
                .ToTwoWayDictionary();
            var shapesForNodes = new List<WeightedShape>[graph.VerticesCount];

            foreach (var vertex in graph.Vertices)
            {
                shapesForNodes[vertex] = new List<WeightedShape>();
                var roomDescription = mapDescription.GetRoomDescription(vertex);

                if (roomDescription is BasicRoomDescription basicRoomDescription)
                {
                    foreach (var roomTemplate in basicRoomDescription.RoomTemplates)
                    {
                        var instances = roomTemplateInstances[roomTemplate];

                        foreach (var roomTemplateInstance in instances)
                        {
                            shapesForNodes[vertex].Add(new WeightedShape(intAliases[roomTemplateInstance], 1d / instances.Count));
                        }
                    }
                }
            }

            return new ConfigurationSpaces<TConfiguration>(shapesForNodes, configurationSpaces, lineIntersection, intAliases);
        }
        
		private ConfigurationSpace GetConfigurationSpace(GridPolygon polygon, List<IDoorLine> doorLines, GridPolygon fixedCenter, List<IDoorLine> doorLinesFixed, List<int> offsets = null)
		{
			if (offsets != null && offsets.Count == 0)
				throw new ArgumentException("There must be at least one offset if they are set", nameof(offsets));

			var configurationSpaceLines = new List<OrthogonalLine>();
			var reverseDoor = new List<Tuple<OrthogonalLine, DoorLine>>();

			doorLines = DoorUtils.MergeDoorLines(doorLines);
			doorLinesFixed = DoorUtils.MergeDoorLines(doorLinesFixed);

			// One list for every direction
			var lines = new List<IDoorLine>[4];

			// Init array
			for (var i = 0; i < lines.Length; i++)
			{
				lines[i] = new List<IDoorLine>();
			}

			// Populate lists with lines
			foreach (var line in doorLinesFixed)
			{
				lines[(int) line.Line.GetDirection()].Add(line);
			}

			foreach (var doorLine in doorLines)
			{
				var line = doorLine.Line;
				var oppositeDirection = OrthogonalLine.GetOppositeDirection(line.GetDirection());
				var rotation = line.ComputeRotation();
				var rotatedLine = line.Rotate(rotation);
				var correspondingLines = lines[(int)oppositeDirection].Where(x => x.Length == doorLine.Length).Select(x => new DoorLine(x.Line.Rotate(rotation), x.Length));

				foreach (var cDoorLine in correspondingLines)
				{
					var cline = cDoorLine.Line;
					var y = cline.From.Y - rotatedLine.From.Y;
					var from = new IntVector2(cline.From.X - rotatedLine.To.X + (rotatedLine.Length - doorLine.Length), y);
					var to = new IntVector2(cline.To.X - rotatedLine.From.X - (rotatedLine.Length + doorLine.Length), y);

					if (from.X < to.X) continue;

					if (offsets == null)
					{
						var resultLine = new OrthogonalLine(from, to, OrthogonalLine.Direction.Left).Rotate(-rotation);
						reverseDoor.Add(Tuple.Create(resultLine, new DoorLine(cDoorLine.Line.Rotate(-rotation), cDoorLine.Length)));
						configurationSpaceLines.Add(resultLine);
					}
					else
					{
						foreach (var offset in offsets)
						{
							var offsetVector = new IntVector2(0, offset);
							var resultLine = new OrthogonalLine(from - offsetVector, to - offsetVector, OrthogonalLine.Direction.Left).Rotate(-rotation);
							reverseDoor.Add(Tuple.Create(resultLine, new DoorLine(cDoorLine.Line.Rotate(-rotation), cDoorLine.Length)));
							configurationSpaceLines.Add(resultLine);
						}
					}
				}
			}

			// Remove all positions when the two polygons overlap
			configurationSpaceLines = RemoveOverlapping(polygon, fixedCenter, configurationSpaceLines);

			// Remove all non-unique positions
			configurationSpaceLines = lineIntersection.RemoveIntersections(configurationSpaceLines);

			return new ConfigurationSpace() { Lines = configurationSpaceLines, ReverseDoors = reverseDoor };
		}

		/// <summary>
		/// Computes configuration space of given two polygons.
		/// </summary>
		/// <param name="polygon"></param>
		/// <param name="doorsMode"></param>
		/// <param name="fixedCenter"></param>
		/// <param name="fixedDoorsMode"></param>
		/// <param name="offsets"></param>
		/// <returns></returns>
		public ConfigurationSpace GetConfigurationSpace(GridPolygon polygon, IDoorMode doorsMode, GridPolygon fixedCenter,
			IDoorMode fixedDoorsMode, List<int> offsets = null)
		{
			var doorLinesFixed = doorHandler.GetDoorPositions(fixedCenter, fixedDoorsMode);
			var doorLines = doorHandler.GetDoorPositions(polygon, doorsMode);

			return GetConfigurationSpace(polygon, doorLines, fixedCenter, doorLinesFixed, offsets);
		}

        public ConfigurationSpace GetConfigurationSpace(RoomTemplateInstance roomTemplateInstance, RoomTemplateInstance fixedRoomTemplateInstance, List<int> offsets = null)
        {
            return GetConfigurationSpace(roomTemplateInstance.RoomShape, roomTemplateInstance.DoorLines,
                fixedRoomTemplateInstance.RoomShape, fixedRoomTemplateInstance.DoorLines, offsets);
        }

        /// <summary>
        /// Returns a list of positions such that a given polygon does not overlap a given fixed one.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="fixedCenter"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        private List<OrthogonalLine> RemoveOverlapping(GridPolygon polygon, GridPolygon fixedCenter, List<OrthogonalLine> lines)
        {
            var nonOverlapping = new List<OrthogonalLine>();

            foreach (var line in lines)
            {
                var overlapAlongLine = polygonOverlap.OverlapAlongLine(polygon, fixedCenter, line);

                var lastOverlap = false;
                var lastPoint = line.From;

                foreach (var @event in overlapAlongLine)
                {
                    var point = @event.Item1;
                    var overlap = @event.Item2;

                    if (overlap && !lastOverlap)
                    {
                        var endPoint = point + -1 * line.GetDirectionVector();

                        if (line.Contains(endPoint) != -1)
                        {
                            nonOverlapping.Add(new OrthogonalLine(lastPoint, endPoint));
                        }
                    }

                    lastOverlap = overlap;
                    lastPoint = point;
                }

                if (overlapAlongLine.Count == 0)
                {
                    nonOverlapping.Add(line);
                }
                else if (!lastOverlap && lastPoint != line.To)
                {
                    nonOverlapping.Add(new OrthogonalLine(lastPoint, line.To));
                }
            }

            return nonOverlapping;
        }

        public List<RoomTemplateInstance> GetRoomTemplateInstances(IRoomTemplate roomTemplate)
        {
            var result = new List<RoomTemplateInstance>();
            var doorLines = doorHandler.GetDoorPositions(roomTemplate.Shape, roomTemplate.DoorsMode);
            var shape = roomTemplate.Shape;

            foreach (var transformation in roomTemplate.AllowedTransformations)
            {
                var transformedShape = shape.Transform(transformation);
                var smallestPoint = transformedShape.BoundingRectangle.A;

                // Both the shape and doors are moved so the polygon is in the first quadrant and touches axes
                transformedShape = transformedShape + (-1 * smallestPoint);
                transformedShape = polygonUtils.NormalizePolygon(transformedShape);
                var transformedDoorLines = doorLines
                    .Select(x => DoorUtils.TransformDoorLine(x, transformation))
                    .Select(x => new DoorLine(x.Line + (-1 * smallestPoint), x.Length))
                    .Cast<IDoorLine>()
                    .ToList();

                // Check if we already have the same room shape (together with door lines)
                var sameRoomShapeFound = false;
                foreach (var roomInfo in result)
                {
                    if (roomInfo.RoomShape.Equals(transformedShape) &&
                        roomInfo.DoorLines.SequenceEqualWithoutOrder(transformedDoorLines))
                    {
                        roomInfo.Transformations.Add(transformation);

                        sameRoomShapeFound = true;
                        break;
                    }
                }

                if (sameRoomShapeFound)
                    continue;

                result.Add(new RoomTemplateInstance(roomTemplate, transformedShape, transformation, transformedDoorLines));
            }

            return result;
		}
    }
}