﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableComputations
{
	public class ScalarDisposing<TValue> : ScalarProcessingVoid<TValue> where TValue : IDisposable
	{
		public ScalarDisposing(IReadScalar<TValue> scalar) : base(scalar, getNewValueProcessor(), false)
		{
		}

		private static Action<TValue, ScalarProcessingVoid<TValue>> getNewValueProcessor()
		{
			return (newValue, scalarDisposing) => scalarDisposing.Value.Dispose();
		}
	}
}