﻿// Copyright (c) 2019-2021 Buchelnikov Igor Vladimirovich. All rights reserved
// Buchelnikov Igor Vladimirovich licenses this file to you under the MIT license.
// The LICENSE file is located at https://github.com/IgorBuchelnikov/ObservableComputations/blob/master/LICENSE

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ObservableComputations
{
	public class FirstComputing<TSourceItem> : ItemComputing<TSourceItem>
	{
		[ObservableComputationsCall]
		public FirstComputing(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			TSourceItem defaultValue = default(TSourceItem)) : base(sourceScalar, 0, defaultValue)
		{
		}

		[ObservableComputationsCall]
		public FirstComputing(
			INotifyCollectionChanged source,
			TSourceItem defaultValue = default(TSourceItem)) : base(source, 0, defaultValue)
		{
		}

		[ExcludeFromCodeCoverage]
		internal void ValidateConsistency()
		{
			IList<TSourceItem> source = (IList<TSourceItem>) _sourceScalar.getValue(_source, new ObservableCollection<TSourceItem>());
			TSourceItem defaultValue = _defaultValue;

			if (!EqualityComparer<TSourceItem>.Default.Equals(_value, source.Count > 0 ? source.First() : defaultValue))
				throw new ObservableComputationsException(this, "Consistency violation: FirstComputing.1");
		}

	}
}
