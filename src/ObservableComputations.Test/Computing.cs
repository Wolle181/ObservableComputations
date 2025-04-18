﻿// Copyright (c) 2019-2021 Buchelnikov Igor Vladimirovich. All rights reserved
// Buchelnikov Igor Vladimirovich licenses this file to you under the MIT license.
// The LICENSE file is located at https://github.com/IgorBuchelnikov/ObservableComputations/blob/master/LICENSE

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObservableComputations.Test
{
	[TestFixture(false)]
	public partial class ComputingTests : TestBase
	{
		OcConsumer consumer = new OcConsumer();

		public class Order : INotifyPropertyChanged
		{
			private string _num;
			public string Num
			{
				get { return _num; }
				set { updatePropertyValue(ref _num, value); }
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

		[Test]
		public void TestRaiseValueChanged()
		{
			bool raised = false;
			Order order = new Order();
			Computing<string> computing = new Computing<string>(() => order.Num).For(consumer);
			computing.PropertyChanged += (sender, args) => { if (args.PropertyName == "Value") raised = true; };
			order.Num = "1";
			Assert.IsTrue(raised);
		}

		[Test]
		public void TestRaiseValueChanged2()
		{
			bool raised = false;
			Order order = new Order();
			Computing<string> computing = new Computing<string>(() =>  new Computing<string>(() => order.Num).Value).For(consumer);
			computing.PropertyChanged += (sender, args) => { if (args.PropertyName == "Value") raised = true; };
			order.Num = "1";
			Assert.IsTrue(raised);
		}

		public ComputingTests(bool debug) : base(debug)
		{
		}
	}
}