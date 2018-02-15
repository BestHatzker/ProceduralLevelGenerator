﻿namespace MapGeneration.Utils.ConfigParsing
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Core;
	using Core.Doors.DoorModes;
	using Deserializers;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Polygons;
	using Models;
	using YamlDotNet.Serialization;
	using YamlDotNet.Serialization.NamingConventions;

	public class ConfigLoader
	{
		private readonly Deserializer deserializer;

		private const string RoomsPath = "Resources/Rooms";

		private const string MapsPath = "Resources/Maps";

		public ConfigLoader()
		{
			deserializer = new DeserializerBuilder()
				.WithNamingConvention(new CamelCaseNamingConvention())
				.WithNodeDeserializer(new IntVector2Deserializer(), s => s.OnTop())
				.WithNodeDeserializer(new OrthogonalLineDeserializer(), s => s.OnTop())
				.WithNodeDeserializer(new StringTupleDeserializer(), s => s.OnTop())
				.WithTagMapping("!OverlapMode", typeof(OverlapMode))
				.WithTagMapping("!SpecificPositions", typeof(SpecificPositionsMode))
				.WithObjectFactory(new DefaultObjectFactory())
				.Build();
		}

		public RoomDescriptionsSetModel LoadRoomDescriptionsModel(TextReader reader)
		{
			return deserializer.Deserialize<RoomDescriptionsSetModel>(reader);
		}

		public MapDescriptionModel LoadMapDescriptionModel(TextReader reader)
		{
			return deserializer.Deserialize<MapDescriptionModel>(reader);
		}

		public MapDescription<string> LoadMapDescription(TextReader reader)
		{
			var mapDescriptionModel = LoadMapDescriptionModel(reader);
			var mapDescription = new MapDescription<string>();
			var roomDescriptionsSets = LoadRoomDescriptionsSetsFromResources();

			LoadRooms(mapDescription, mapDescriptionModel, roomDescriptionsSets);

			return mapDescription;
		}

		private Dictionary<string, RoomDescriptionsSetModel> LoadRoomDescriptionsSetsFromResources()
		{
			var filenames = Directory.GetFiles(RoomsPath, "*.yml");
			var models = new Dictionary<string, RoomDescriptionsSetModel>();

			foreach (var filename in filenames)
			{
				using (var sr = new StreamReader(filename))
				{
					var model = LoadRoomDescriptionsModel(sr);

					if (string.IsNullOrEmpty(model.Name))
					{
						throw new InvalidOperationException("Name must not be empty");
					}

					models.Add(model.Name, model);
				}
			}

			return models;
		}

		private void LoadRooms(MapDescription<string> mapDescription, MapDescriptionModel mapDescriptionModel, Dictionary<string, RoomDescriptionsSetModel> roomDescriptionsSets)
		{
			if (mapDescriptionModel.Rooms == null || mapDescriptionModel.Rooms.Count == 0)
				throw new InvalidOperationException("There must be at least one room");

			foreach (var room in mapDescriptionModel.Rooms)
			{
				if (string.IsNullOrEmpty(room.Name))
					throw new InvalidOperationException("Name of rooms must not be empty");

				mapDescription.AddRoom(room.Name);

				if (room.RoomShapes != null)
				{
					foreach (var rooms in room.RoomShapes)
					{
						var roomShapes = GetRoomDescriptions(rooms, roomDescriptionsSets, mapDescriptionModel.CustomRoomDescriptionsSet, rooms.Scale);
						mapDescription.AddRoomShapes(room.Name, roomShapes, rooms.Rotate ?? true, rooms.Probability ?? 1);
					}
				}
			}

			if (mapDescriptionModel.DefaultRoomShapes != null)
			{
				foreach (var rooms in mapDescriptionModel.DefaultRoomShapes)
				{
					var roomShapes = GetRoomDescriptions(rooms, roomDescriptionsSets, mapDescriptionModel.CustomRoomDescriptionsSet, rooms.Scale);
					mapDescription.AddRoomShapes(roomShapes, rooms.Rotate ?? true, rooms.Probability ?? 1);
				}
			}
		}

		private List<RoomDescription> GetRoomDescriptions(RoomShapesModel roomShapesModel, Dictionary<string, RoomDescriptionsSetModel> roomDescriptionsSets, RoomDescriptionsSetModel customRoomDescriptionsSet, IntVector2? scale)
		{
			var setModel = GetSetModel(roomShapesModel.SetName, roomDescriptionsSets, customRoomDescriptionsSet);
			var setName = string.IsNullOrEmpty(roomShapesModel.SetName) ? "custom" : roomShapesModel.SetName;

			if (!string.IsNullOrEmpty(roomShapesModel.RoomDescriptionName) && !setModel.RoomDescriptions.ContainsKey(roomShapesModel.RoomDescriptionName))
				throw new InvalidOperationException($"Room description with name \"{roomShapesModel.RoomDescriptionName}\" was not found in the set \"{setName}\"");

			var roomModels = GetFilledRoomModels(setModel, roomShapesModel.RoomDescriptionName, setName);
			var roomDescriptions = roomModels.Select(x => ConvertRoomModelToDescription(x, scale)).ToList();

			return roomDescriptions;
		}

		private RoomDescription ConvertRoomModelToDescription(RoomDescriptionModel model, IntVector2? scale)
		{
			return new RoomDescription(new GridPolygon(model.Shape).Scale(scale ?? new IntVector2(1, 1)), model.DoorMode);
		}

		private List<RoomDescriptionModel> GetFilledRoomModels(RoomDescriptionsSetModel setModel, string roomDescriptionName, string setName)
		{
			var roomModels = new List<RoomDescriptionModel>();
			var defaultModel = setModel.Default;

			foreach (var pair in setModel.RoomDescriptions)
			{
				var name = pair.Key;
				var roomModel = pair.Value;

				if (!string.IsNullOrEmpty(roomDescriptionName) && name != roomDescriptionName)
					continue;

				if (roomModel.Shape == null)
				{
					if (defaultModel.Shape == null)
						throw new InvalidOperationException($"Neither shape nor default shape are set. Room {name}, set {setName}");

					roomModel.Shape = defaultModel.Shape;
				}

				if (roomModel.DoorMode == null)
				{
					if (defaultModel.DoorMode == null)
						throw new InvalidOperationException($"Neither door mode nor default door mode are set. Room {name}, set {setName}");

					roomModel.DoorMode = defaultModel.DoorMode;
				}

				roomModels.Add(roomModel);
			}

			return roomModels;
		}

		private RoomDescriptionsSetModel GetSetModel(string name, Dictionary<string, RoomDescriptionsSetModel> roomDescriptionsSets, RoomDescriptionsSetModel customRoomDescriptionsSet)
		{
			if (string.IsNullOrEmpty(name))
			{
				return customRoomDescriptionsSet;
			}

			if (!roomDescriptionsSets.TryGetValue(name, out var set))
				throw new InvalidOperationException($"Room descriptions set with name \"{name}\" was not found");

			return set;
		}
	}
}