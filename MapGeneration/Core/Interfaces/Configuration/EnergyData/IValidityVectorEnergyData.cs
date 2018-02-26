﻿namespace MapGeneration.Core.Interfaces.Configuration.EnergyData
{
	using GeneralAlgorithms.DataStructures.Common;

	public interface IValidityVectorEnergyData : IEnergyData
	{
		SimpleBitVector32 ValidityVector { get; set; }
	}
}