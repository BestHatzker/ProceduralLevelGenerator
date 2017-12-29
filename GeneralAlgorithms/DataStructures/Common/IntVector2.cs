﻿namespace GeneralAlgorithms.DataStructures.Common
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using Algorithms.Math;

	public struct IntVector2 : IComparable<IntVector2>, IEquatable<IntVector2>
	{
		public readonly int X;

		public readonly int Y;

		public IntVector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public int CompareTo(IntVector2 other)
		{
			if (other == this)
			{
				return 0;
			}

			return this < other ? -1 : 1;
		}

		public override string ToString()
		{
			return $"IntVector2 ({X}, {Y})";
		}

		[Pure]
		public string ToStringShort()
		{
			return $"[{X}, {Y}]";
		}

		public override bool Equals(object obj)
		{
			if (obj is null) return false;

			return obj is IntVector2 vector2 && Equals(vector2);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X * 397) ^ Y;
			}
		}

		public List<IntVector2> GetAdjacentTiles()
		{
			var positions = new List<IntVector2>
			{
				new IntVector2(X + 1, Y),
				new IntVector2(X - 1, Y),
				new IntVector2(X, Y + 1),
				new IntVector2(X, Y - 1)
			};

			return positions;
		}

		public List<IntVector2> GetAdjacentTilesAndDiagonal()
		{
			var positions = GetAdjacentTiles();

			positions.Add(new IntVector2(X + 1, Y + 1));
			positions.Add(new IntVector2(X - 1, Y - 1));
			positions.Add(new IntVector2(X - 1, Y + 1));
			positions.Add(new IntVector2(X + 1, Y - 1));

			return positions;
		}

		public static IntVector2 GetGridDirection(int x, int y)
		{
			if (x != 0)
				y = 0;

			if (y != 0)
				x = 0;

			return new IntVector2(x, y);
		}

		public static int ManhattanDistance(IntVector2 a, IntVector2 b)
		{
			return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
		}

		public static double EuclideanDistance(IntVector2 a, IntVector2 b)
		{
			return Math.Sqrt((int)(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)));
		}

		public static int MaxDistance(IntVector2 a, IntVector2 b)
		{
			return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
		}

		public List<IntVector2> GetRadius(int radius, Func<IntVector2, IntVector2, int> metric, bool includeInside)
		{
			var positions = new List<IntVector2>();

			for (var i = X - radius; i <= X + radius; i++)
			{
				for (var j = Y - radius; j <= Y + radius; j++)
				{
					var pos = new IntVector2(i, j);

					if (includeInside)
					{
						if (metric(this, pos) <= radius)
						{
							positions.Add(pos);
						}
					}
					else
					{
						if (metric(this, pos) == radius)
						{
							positions.Add(pos);
						}
					}
				}
			}

			return positions;
		}

		/// <summary>
		/// Rotate the point are the center.
		/// </summary>
		/// <remarks>
		/// Positive degrees mean clockwise rotation.
		/// </remarks>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public IntVector2 RotateAroundCenter(int degrees)
		{
			var x = X * IntegerGoniometric.Cos(degrees) + Y * IntegerGoniometric.Sin(degrees);
			var y = - X * IntegerGoniometric.Sin(degrees) + Y * IntegerGoniometric.Cos(degrees);

			return new IntVector2(x, y);
		}

		public int DotProduct(IntVector2 other)
		{
			return X * other.X + Y * other.Y;
		}

		public IntVector2 ElemWiseProduct(IntVector2 other)
		{
			return new IntVector2(X * other.X, Y * other.Y);
		}

		#region Operators

		public static IntVector2 operator +(IntVector2 a, IntVector2 b)
		{
			return new IntVector2(a.X + b.X, a.Y + b.Y);
		}

		public static IntVector2 operator -(IntVector2 a, IntVector2 b)
		{
			return new IntVector2(a.X - b.X, a.Y - b.Y);
		}

		public static IntVector2 operator *(int a, IntVector2 b)
		{
			return new IntVector2(a * b.X, a * b.Y);
		}

		public static bool operator ==(IntVector2 a, IntVector2 b)
		{
			return Equals(a, b);
		}

		public static bool operator !=(IntVector2 a, IntVector2 b)
		{

			return !Equals(a, b);
		}

		public static bool operator <=(IntVector2 a, IntVector2 b)
		{

			return a.X <= b.X || (a.X == b.X && a.Y <= b.Y);
		}

		public static bool operator <(IntVector2 a, IntVector2 b)
		{
			return a.X < b.X || (a.X == b.X && a.Y < b.Y);
		}

		public static bool operator >(IntVector2 a, IntVector2 b)
		{
			return !(a <= b);
		}

		public static bool operator >=(IntVector2 a, IntVector2 b)
		{
			return !(a < b);
		}

		#endregion

		public bool Equals(IntVector2 other)
		{
			return X == other.X && Y == other.Y;
		}
	}
}
