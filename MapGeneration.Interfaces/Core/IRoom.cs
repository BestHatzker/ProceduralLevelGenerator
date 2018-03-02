﻿namespace MapGeneration.Interfaces.Core
{
	using System;
	using System.Collections.Generic;
	using GeneralAlgorithms.DataStructures.Common;
	using GeneralAlgorithms.DataStructures.Polygons;

	public interface IRoom<TNode>
	{
		TNode Node { get; }

		GridPolygon Shape { get;  }

		IntVector2 Position { get; }

		bool IsCorridor { get; }

		IList<Tuple<TNode, OrthogonalLine>> Doors { get; }
	}
}