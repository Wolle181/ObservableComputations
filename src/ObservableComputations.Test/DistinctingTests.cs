﻿// Copyright (c) 2019-2021 Buchelnikov Igor Vladimirovich. All rights reserved
// Buchelnikov Igor Vladimirovich licenses this file to you under the MIT license.
// The LICENSE file is located at https://github.com/IgorBuchelnikov/ObservableComputations/blob/master/LICENSE

using System.Collections.ObjectModel;
using NUnit.Framework;

namespace ObservableComputations.Test
{
	[TestFixture(false)]
	public partial class DistinctingTests : TestBase
	{
		OcConsumer consumer = new OcConsumer();

		[Test]
		public void Distincting_Initialization_01()
		{
			ObservableCollection<int> items = new ObservableCollection<int>();

			Distincting<int> distincting = items.Distincting().For(consumer);
			distincting.ValidateInternalConsistency();			
			consumer.Dispose();
		}

		[Test, Combinatorial]
		public void Distincting_Remove(
			[Values(1, 2, 3)] int item0,
			[Values(1, 2, 3)] int item1,
			[Values(1, 2, 3)] int item2,
			[Values(1, 2, 3)] int item3,
			[Values(1, 2, 3)] int item4,
			[Range(0, 4, 1)] int index)
		{
			ObservableCollection<int> items = new ObservableCollection<int>(
				new[]
				{
					item0,
					item1,
					item2,
					item3,
					item4
				}

			);

			Distincting<int> distincting = items.Distincting().For(consumer);
			distincting.ValidateInternalConsistency();
			items.RemoveAt(index);
			distincting.ValidateInternalConsistency();			
			consumer.Dispose();
		}

		[Test, Combinatorial]
		public void Distincting_Remove1(
			[Values(1, 2, 3)] int item0)
		{
			ObservableCollection<int> items = new ObservableCollection<int>(
				new[]
				{
					item0
				}

			);

			Distincting<int> distincting = items.Distincting().For(consumer);
			distincting.ValidateInternalConsistency();
			items.RemoveAt(0);
			distincting.ValidateInternalConsistency();			
			consumer.Dispose();
		}

		[Test, Combinatorial]
		public void Distincting_Insert(
			[Values(1, 2, 3)] int item0,
			[Values(1, 2, 3)] int item1,
			[Values(1, 2, 3)] int item2,
			[Values(1, 2, 3)] int item3,
			[Values(1, 2, 3)] int item4,
			[Range(0, 4, 1)] int index,
			[Values(1, 2, 3)] int newValue)
		{
			ObservableCollection<int> items = new ObservableCollection<int>(
				new[]
				{
					item0,
					item1,
					item2,
					item3,
					item4
				}

			);

			Distincting<int> distincting = items.Distincting().For(consumer);
			distincting.ValidateInternalConsistency();
			items.Insert(index, newValue);
			distincting.ValidateInternalConsistency();			
			consumer.Dispose();
		}

		[Test, Combinatorial]
		public void Distincting_Insert1(
			[Values(1, 2, 3)] int newValue)
		{
			ObservableCollection<int> items = new ObservableCollection<int>();

			Distincting<int> distincting = items.Distincting().For(consumer);
			distincting.ValidateInternalConsistency();
			items.Insert(0, newValue);
			distincting.ValidateInternalConsistency();			
			consumer.Dispose();
		}

		[Test, Combinatorial]
		public void Distincting_Move(
			[Values(1, 2, 3)] int item0,
			[Values(1, 2, 3)] int item1,
			[Values(1, 2, 3)] int item2,
			[Values(1, 2, 3)] int item3,
			[Values(1, 2, 3)] int item4,
			[Range(0, 4, 1)] int oldIndex,
			[Range(0, 4, 1)] int newIndex)
		{
			ObservableCollection<int> items = new ObservableCollection<int>(
				new[]
				{
					item0,
					item1,
					item2,
					item3,
					item4
				}

			);

			Distincting<int> distincting = items.Distincting().For(consumer);
			distincting.ValidateInternalConsistency();
			items.Move(oldIndex, newIndex);
			distincting.ValidateInternalConsistency();			
			consumer.Dispose();
		}

		[Test, Combinatorial]
		public void Distincting_Set(
			[Values(1, 2, 3)] int item0,
			[Values(1, 2, 3)] int item1,
			[Values(1, 2, 3)] int item2,
			[Values(1, 2, 3)] int item3,
			[Values(1, 2, 3)] int item4,
			[Range(0, 4, 1)] int index,
			[Values(1, 2, 3)] int itemNew)
		{
			ObservableCollection<int> items = new ObservableCollection<int>(
				new[]
				{
					item0,
					item1,
					item2,
					item3,
					item4
				}

			);

			Distincting<int> distincting = items.Distincting().For(consumer);
			distincting.ValidateInternalConsistency();
			items[index] = itemNew;
			distincting.ValidateInternalConsistency();			
			consumer.Dispose();
		}

		public DistinctingTests(bool debug) : base(debug)
		{
		}
	}
}