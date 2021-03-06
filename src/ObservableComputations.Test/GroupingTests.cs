﻿// Copyright (c) 2019-2021 Buchelnikov Igor Vladimirovich. All rights reserved
// Buchelnikov Igor Vladimirovich licenses this file to you under the MIT license.
// The LICENSE file is located at https://github.com/IgorBuchelnikov/ObservableComputations/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObservableComputations.Test
{
	[TestFixture(false)]
	public partial class GroupingTests : TestBase
	{
		OcConsumer consumer = new OcConsumer();

		public class Item : INotifyPropertyChanged
		{
			private int? _key;

			public int? Key
			{
				get { return _key; }
				set { updatePropertyValue(ref _key, value); }
			}

			public Item(int? key)
			{
				_key = key;
				Num = LastNum;
				LastNum++;
			}

			public static int LastNum;
			public int Num;

			#region INotifyPropertyChanged imlementation

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void onPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler onPropertyChanged = PropertyChanged;
				if (onPropertyChanged != null) onPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			protected bool updatePropertyValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
			{
				if (EqualityComparer<T>.Default.Equals(field, value)) return false;
				field = value;
				this.onPropertyChanged(propertyName);
				return true;
			}

			#endregion
		}

		TextFileOutput _textFileOutputLog = new TextFileOutput(@"D:\Grouping_Deep.log");
		TextFileOutput _textFileOutputTime = new TextFileOutput(@"D:\Grouping_Deep_Time.log");

#if !RunOnlyMinimalTestsToCover
		[Test]
		public void Grouping_Deep()
		{
			long counter = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();
					
			test(new int[0]);


			int from = -1;
			int to = 5;

			for (int v1 = from; v1 <= to; v1++)
			{
				test(new []{v1});
				for (int v2 = from; v2 <= to; v2++)
				{
					test(new []{v1, v2});
					for (int v3 = from; v3 <= to; v3++)
					{
						test(new []{v1, v2, v3});
						for (int v4 = from; v4 <= to; v4++)
						{
							test(new []{v1, v2, v3, v4});
							for (int v5 = from; v5 <= to; v5++)
							{
								test(new[] {v1, v2, v3, v4, v5});
								counter++;
								if (counter % 100 == 0)
								{
									_textFileOutputTime.AppentLine($"{stopwatch.Elapsed.TotalMinutes}: {counter}");
								}
							}
						}
					}
				}
			}
		}
#endif

		private void test(int[] values)
		{
			string testNum = string.Empty;
			int index = 0;
			int orderNum = 0;
			int indexOld = 0;
			int indexNew = 0;

			ObservableCollection<Item> items;
			Grouping<Item, int?> grouping;
			try
			{
				trace(testNum = "1", values, index, orderNum, indexOld, indexNew);
				items = getObservableCollection(values);
				grouping = items.Grouping(i => i.Key).For(consumer);
				grouping.ValidateConsistency();				
				consumer.Dispose();

				for (index = 0; index < values.Length; index++)
				{
					trace(testNum = "2", values, index, orderNum, indexOld, indexNew);
					items = getObservableCollection(values);
					Grouping<Item, int?> grouping1 = items.Grouping(i => i.Key).For(consumer);
					items.RemoveAt(index);
					grouping1.ValidateConsistency();					
					consumer.Dispose();
				}

				for (index = 0; index <= values.Length; index++)
				{
					for (orderNum = 0; orderNum <= values.Length; orderNum++)
					{
						trace(testNum = "8", values, index, orderNum, indexOld, indexNew);
						items = getObservableCollection(values);
						Grouping<Item, int?> grouping1 = items.Grouping(i => i.Key).For(consumer);
						items.Insert(index, new Item(orderNum));
						grouping1.ValidateConsistency();						
						consumer.Dispose();
					}
				}

				for (index = 0; index < values.Length; index++)
				{
					trace(testNum = "6", values, index, orderNum, indexOld, indexNew);
					items = getObservableCollection(values);
					Grouping<Item, int?> grouping3 = items.Grouping(i => i.Key).For(consumer);
					items[index] = new Item(null);
					grouping3.ValidateConsistency();					
					consumer.Dispose();

					for (orderNum = -1; orderNum <= values.Length; orderNum++)
					{
						trace(testNum = "3", values, index, orderNum, indexOld, indexNew);
						items = getObservableCollection(values);
						Grouping<Item, int?> grouping2 = items.Grouping(i => i.Key).For(consumer);
						items[index] = new Item(orderNum);
						grouping2.ValidateConsistency();						
						consumer.Dispose();

					}
				}

				for (index = 0; index < values.Length; index++)
				{
					trace(testNum = "4", values, index, orderNum, indexOld, indexNew);
					items = getObservableCollection(values);
					Grouping<Item, int?> grouping1 = items.Grouping(i => i.Key).For(consumer);
					items[index].Key = null;
					grouping1.ValidateConsistency();					
					consumer.Dispose();

					for (orderNum = -1; orderNum <= values.Length; orderNum++)
					{
						trace(testNum = "5", values, index, orderNum, indexOld, indexNew);
						items = getObservableCollection(values);
						Grouping<Item, int?> grouping2 = items.Grouping(i => i.Key).For(consumer);
						items[index].Key = orderNum;
						grouping2.ValidateConsistency();						
						consumer.Dispose();

					}
				}
				for (indexOld = 0; indexOld < values.Length; indexOld++)
				{
					for (indexNew = 0; indexNew < values.Length; indexNew++)
					{
						trace(testNum = "7", values, index, orderNum, indexOld, indexNew);
						items = getObservableCollection(values);
						Grouping<Item, int?> grouping2 = items.Grouping(i => i.Key).For(consumer);
						items.Move(indexOld, indexNew);
						grouping2.ValidateConsistency();						
						consumer.Dispose();
					}
				}
			}
			catch (Exception e)
			{
				string traceString = getTraceString(testNum, values, index, orderNum, indexOld, indexNew);
				_textFileOutputLog.AppentLine(traceString);
				_textFileOutputLog.AppentLine(e.Message);
				_textFileOutputLog.AppentLine(e.StackTrace);
				throw new Exception(traceString, e);
			}

			writeUsefulTest(getTestString(values));

		}

		private void trace(string testNum, int[] orderNums, int index, int orderNum, int indexOld, int indexNew)
		{
			string traceString = getTraceString(testNum, orderNums, index, orderNum, indexOld, indexNew);
			
			if (traceString == "#3. OrderNums=-1  index=0  orderNum=-1   indexOld=0   indexNew=0")
			{
				Debugger.Break();
			}
		}

		private string getTraceString(string testNum, int[] orderNums, int index, int orderNum, int indexOld, int indexNew)
		{
			return string.Format(
				"#{5}. OrderNums={0}  index={1}  orderNum={2}   indexOld={3}   indexNew={4}",
				string.Join(",", orderNums),
				index,
				orderNum,
				indexOld,
				indexNew,
				testNum);
		}


		private static ObservableCollection<Item> getObservableCollection(int[] orderNums)
		{
			return new ObservableCollection<Item>(orderNums.Select(orderNum => new Item(orderNum >= 0 ? orderNum : (int?)null)));
		}

		public GroupingTests(bool debug) : base(debug)
		{
		}
	}
}
