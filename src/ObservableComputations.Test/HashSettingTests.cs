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
	public partial class HashSettingTests : TestBase
	{
		OcConsumer consumer = new OcConsumer();

		public class Item : INotifyPropertyChanged
		{
			private int _key;
			public int Key
			{
				get => _key;
				set => updatePropertyValue(ref _key, value);
			}

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

		TextFileOutput _textFileOutputLog = new TextFileOutput(@"D:\Projects\NevaPolimer\Hashing_Deep.log");
		TextFileOutput _textFileOutputTime = new TextFileOutput(@"D:\Projects\NevaPolimer\Hashing_Deep_Time.log");

#if !RunOnlyMinimalTestsToCover
		[Test]
		public void Hashing_Deep()
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
			int index = 0;
			int newKey = 0;
			int newValue = 0;
			int indexOld = 0;
			int indexNew = 0;
			string testNum = string.Empty;

			ObservableCollection<Item> items;
			HashSetting<Item, int> hashing;
			try
			{
				trace(testNum = "1", values, index, newKey, newValue, indexOld, indexNew);
				items = getObservableCollection(values);
				hashing = items.HashSetting(i => i.Key).For(consumer);
				hashing.ValidateInternalConsistency();				
				consumer.Dispose();

				for (index = 0; index < values.Length; index++)
				{
					trace(testNum = "2", values, index, newKey, newValue, indexOld, indexNew);
					items = getObservableCollection(values);
					HashSetting<Item, int> hashing1 = items.HashSetting(i => i.Key).For(consumer);
					items.RemoveAt(index);
					hashing1.ValidateInternalConsistency();					
					consumer.Dispose();
				}

				for (index = 0; index <= values.Length; index++)
				{
					for (newValue = -1; newValue <= values.Length; newValue++)
					{
						trace(testNum = "3", values, index, newKey, newValue, indexOld, indexNew);
						items = getObservableCollection(values);
						HashSetting<Item, int> hashing1 = items.HashSetting(i => i.Key).For(consumer);
						items.Insert(index, new Item(){Key = values.Length});
						hashing1.ValidateInternalConsistency();						
						consumer.Dispose();
					}
				}

				for (index = 0; index < values.Length; index++)
				{
					for (newValue = -1; newValue <= values.Length; newValue++)
					{
						trace(testNum = "4", values, index, newKey, newValue, indexOld, indexNew);
						items = getObservableCollection(values);
						HashSetting<Item, int> hashing2 = items.HashSetting(i => i.Key).For(consumer);
						items[index] = new Item(){Key = values.Length};
						hashing2.ValidateInternalConsistency();						
						consumer.Dispose();

						trace(testNum = "5", values, index, newKey, newValue, indexOld, indexNew);
						items = getObservableCollection(values);
						hashing2 = items.HashSetting(i => i.Key).For(consumer);
						items[index] = new Item(){Key = items[index].Key};
						hashing2.ValidateInternalConsistency();						
						consumer.Dispose();

					}
				}

				for (index = 0; index < values.Length; index++)
				{
					for (newValue = -1; newValue <= values.Length; newValue++)
					{
						trace(testNum = "6", values, index, newKey, newValue, indexOld, indexNew);
						items = getObservableCollection(values);
						HashSetting<Item, int> hashing2 = items.HashSetting(i => i.Key).For(consumer);
						items[index].Key = values.Length;
						hashing2.ValidateInternalConsistency();						
						consumer.Dispose();

						trace(testNum = "7", values, index, newKey, newValue, indexOld, indexNew);
						items = getObservableCollection(values);
						hashing2 = items.HashSetting(i => i.Key).For(consumer);
						hashing2.ValidateInternalConsistency();						
						consumer.Dispose();

					}
				}

				for (indexOld = 0; indexOld < values.Length; indexOld++)
				{
					for (indexNew = 0; indexNew < values.Length; indexNew++)
					{
						trace(testNum = "3", values, index, newKey, newValue, indexOld, indexNew);
						items = getObservableCollection(values);
						HashSetting<Item, int> hashing2 = items.HashSetting(i => i.Key).For(consumer);
						items.Move(indexOld, indexNew);
						hashing2.ValidateInternalConsistency();						
						consumer.Dispose();
					}
				}
			}
			catch (Exception e)
			{
				string traceString = getTraceString(testNum, values, index, newKey, newValue, indexOld, indexNew);
				_textFileOutputLog.AppentLine(traceString);
				_textFileOutputLog.AppentLine(e.Message);
				_textFileOutputLog.AppentLine(e.StackTrace);
				throw new Exception(traceString, e);
			}

			writeUsefulTest(getTestString(values));
		}

		private void trace(string testNum, int[] values, int index, int newKey, int newValue, int indexOld,int indexNew)
		{
			string traceString = getTraceString(testNum, values, index, newKey, newValue, indexOld, indexNew);
			if (traceString == "#3. values=-1,-1  index=2  newKey=0   newValue=3   indexNew=0  indexOld=1")
			{
				
			}
		}

		private static string getTraceString(string testNum, int[] values, int index, int newKey, int newValue, int indexOld,int indexNew)
		{
			return string.Format(
				"#{0}. values={1}  index={2}  newKey={3}   newValue={4}   indexNew={5}  indexOld={6}",
				testNum,
				string.Join(",", values),
				index,
				newKey,
				newValue,
				indexOld,
				indexNew);
		}


		private static ObservableCollection<Item> getObservableCollection(int[] values)
		{
			return new ObservableCollection<Item>(Enumerable.Range(0, values.Length).Select(n => new Item(){Key = n}));
		}

		public HashSettingTests(bool debug) : base(debug)
		{
		}
	}
}
