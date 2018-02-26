﻿namespace MapGeneration.Interfaces.Core
{
	using System.Collections.Generic;

	public interface ILayoutGenerator<TNode>
	{
		IList<IMapLayout<TNode>> GetLayouts(IMapDescription<TNode> mapDescription, int numberOfLayouts = 10);
	}
}