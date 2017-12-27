﻿namespace MapGeneration.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Interfaces;

	public class Layout : ILayout<int>, ICloneable
	{
		private readonly Configuration?[] vertices;

		public Layout(int verticesCount)
		{
			vertices = new Configuration?[verticesCount];
		}

		private Layout(Configuration?[] vertices)
		{
			this.vertices = (Configuration?[])vertices.Clone();
		}

		public bool GetConfiguration(int node, out Configuration configuration)
		{
			if (vertices[node].HasValue)
			{
				configuration = vertices[node].Value;
				return true;
			}

			configuration = default(Configuration);
			return false;
		}

		public void SetConfiguration(int node, Configuration configuration)
		{
			vertices[node] = configuration;
		}

		public float GetEnergy()
		{
			return vertices.Where(x => x.HasValue).Sum(x => x.Value.Energy);
		}

		public IEnumerable<Configuration> GetConfigurations()
		{
			// TODO: what is this for?
			throw new System.NotImplementedException();
		}

		IEnumerable<IRoom<int>> ILayout<int>.GetRooms()
		{
			throw new System.NotImplementedException();
		}

		public Layout Clone()
		{
			return new Layout(vertices);
		}

		ILayout<int> ILayout<int>.Clone()
		{
			return Clone();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}