﻿namespace MapGeneration.Core.GeneratorPlanners
{
	using System.Collections.Generic;
	using System.Linq;

	/// <inheritdoc />
	public class LazyGeneratorPlanner<TLayout> : GeneratorPlannerBase<TLayout>
	{
		private readonly bool resetAfterValid = true;

		public LazyGeneratorPlanner()
		{

		}

		public LazyGeneratorPlanner(bool resetAfterValid)
		{
			this.resetAfterValid = resetAfterValid;
		}

		/// <summary>
		/// Alaways chooses a not finished layout on the highest level.
		/// </summary>
		/// <param name="rows"></param>
		/// <returns></returns>
		protected override Instance GetNextInstance(List<InstanceRow> rows)
		{
			var depth = rows.Count - 1;

			while (depth >= 0)
			{
				var row = rows[depth];
				var instance = row.Instances.FirstOrDefault(x => !x.IsFinished);

				if (instance == null)
				{
					depth--;
					continue;
				}

				return instance;
			}

			return AddZeroLevelInstance();
		}

		protected override void AfterValid()
		{
			base.AfterValid();

			if (resetAfterValid)
			{
				ResetRows();
			}
		}
	}
}