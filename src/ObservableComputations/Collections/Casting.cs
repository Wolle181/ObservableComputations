﻿// Copyright (c) 2019-2021 Buchelnikov Igor Vladimirovich. All rights reserved
// Buchelnikov Igor Vladimirovich licenses this file to you under the MIT license.
// The LICENSE file is located at https://github.com/IgorBuchelnikov/ObservableComputations/blob/master/LICENSE

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ObservableComputations
{
	public class Casting<TResultItem> : CollectionComputing<TResultItem>, IHasSourceCollections, ISourceIndexerPropertyTracker, ISourceCollectionChangeProcessor
	{
		// ReSharper disable once MemberCanBePrivate.Global
		public virtual IReadScalar<INotifyCollectionChanged> SourceScalar => _sourceScalar;

		// ReSharper disable once MemberCanBePrivate.Global
		public virtual INotifyCollectionChanged Source => _source;

		public virtual ReadOnlyCollection<INotifyCollectionChanged> Sources => new ReadOnlyCollection<INotifyCollectionChanged>(new []{Source});
		public virtual ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>> SourceScalars => new ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>>(new []{SourceScalar});

		private IList _sourceAsList;

		private INotifyCollectionChanged _source;
		private readonly IReadScalar<INotifyCollectionChanged> _sourceScalar;

		private bool _indexerPropertyChangedEventRaised;
		private INotifyPropertyChanged _sourceAsINotifyPropertyChanged;

		private IHasChangeMarker _sourceAsIHasChangeMarker;
		private bool _lastProcessedSourceChangeMarker;

		private readonly ISourceCollectionChangeProcessor _thisAsSourceCollectionChangeProcessor;

		[ObservableComputationsCall]
		public Casting(
			IReadScalar<INotifyCollectionChanged> sourceScalar) : base(Utils.getCapacity(sourceScalar))
		{
			_sourceScalar = sourceScalar;
			_thisAsSourceCollectionChangeProcessor = this;
		}

		[ObservableComputationsCall]
		public Casting(
			INotifyCollectionChanged source) : base(Utils.getCapacity(source))
		{
			_source = source;
			_thisAsSourceCollectionChangeProcessor = this;
		}

		protected override void processSource()
		{
			int originalCount = _items.Count;

			if (_sourceReadAndSubscribed)
			{	
				_source.CollectionChanged -= handleSourceCollectionChanged;

				if (_sourceAsINotifyPropertyChanged != null)
				{
					_sourceAsINotifyPropertyChanged.PropertyChanged -=
						((ISourceIndexerPropertyTracker) this).HandleSourcePropertyChanged;
					_sourceAsINotifyPropertyChanged = null;
				}

				_sourceReadAndSubscribed = false;
			}

			Utils.changeSource(ref _source, _sourceScalar, _downstreamConsumedComputings, _consumers, this,
				out _sourceAsList, true);

			if (_sourceAsList != null && _isActive)
			{
				Utils.initializeFromHasChangeMarker(
					out _sourceAsIHasChangeMarker, 
					_sourceAsList, 
					ref _lastProcessedSourceChangeMarker, 
					ref _sourceAsINotifyPropertyChanged,
					(ISourceIndexerPropertyTracker)this);


				int count = _sourceAsList.Count;
				object[] sourceCopy = new object[count];
				_sourceAsList.CopyTo(sourceCopy, 0);

				_source.CollectionChanged += handleSourceCollectionChanged;

				int sourceIndex;
				for (sourceIndex = 0; sourceIndex < count; sourceIndex++)
				{
					if (originalCount > sourceIndex)
						_items[sourceIndex] = (TResultItem)sourceCopy[sourceIndex];
					else
						_items.Insert(sourceIndex, (TResultItem)sourceCopy[sourceIndex]);
				}

				for (int index = originalCount - 1; index >= sourceIndex; index--)
				{
					_items.RemoveAt(index);
				}

				_sourceReadAndSubscribed = true;
			}			
			else
			{
				_items.Clear();
			}

			reset();
		}

		private void handleSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!Utils.preHandleSourceCollectionChanged(
				sender, 
				e, 
				ref _isConsistent, 
				ref _indexerPropertyChangedEventRaised, 
				ref _lastProcessedSourceChangeMarker, 
				_sourceAsIHasChangeMarker, 
				ref _handledEventSender, 
				ref _handledEventArgs,
				ref _deferredProcessings,
				1, _deferredQueuesCount, this)) return;

			_thisAsSourceCollectionChangeProcessor.processSourceCollectionChanged(sender, e);

			Utils.postHandleChange(
				ref _handledEventSender,
				ref _handledEventArgs,
				_deferredProcessings,
				ref _isConsistent,
				this);
		}

		void ISourceCollectionChangeProcessor.processSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					IList newItems = e.NewItems;
					//if (newItems.Count > 1) throw new ObservableComputationsException(this, "Adding of multiple items is not supported");
					baseInsertItem(e.NewStartingIndex, (TResultItem) newItems[0]);
					break;
				case NotifyCollectionChangedAction.Remove:
					//if (e.OldItems.Count > 1) throw new ObservableComputationsException(this, "Removing of multiple items is not supported");
					baseRemoveItem(e.OldStartingIndex);
					break;
				case NotifyCollectionChangedAction.Move:
					int oldStartingIndex = e.OldStartingIndex;
					int newStartingIndex = e.NewStartingIndex;
					if (oldStartingIndex != newStartingIndex)
					{
						baseMoveItem(oldStartingIndex, newStartingIndex);
					}

					break;
				case NotifyCollectionChangedAction.Replace:
					IList newItems1 = e.NewItems;
					//if (newItems1.Count > 1) throw new ObservableComputationsException(this, "Replacing of multiple items is not supported");
					baseSetItem(e.NewStartingIndex, (TResultItem) newItems1[0]);
					break;
				case NotifyCollectionChangedAction.Reset:
					processSource();
					break;
			}
		}

		internal override void addToUpstreamComputings(IComputingInternal computing)
		{
			Utils.AddDownstreamConsumedComputing(computing, _sourceScalar, _source);
		}

		internal override void removeFromUpstreamComputings(IComputingInternal computing)		
		{
			Utils.RemoveDownstreamConsumedComputing(computing, _sourceScalar, _source);
		}

		protected override void initialize()
		{
			Utils.initializeSourceScalar(_sourceScalar, ref _source, scalarValueChangedHandler);
		}

		protected override void uninitialize()
		{
			Utils.uninitializeSourceScalar(_sourceScalar, scalarValueChangedHandler, ref _source);
		}

		#region Implementation of ISourceIndexerPropertyTracker

		void ISourceIndexerPropertyTracker.HandleSourcePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			Utils.handleSourcePropertyChanged(propertyChangedEventArgs, ref _indexerPropertyChangedEventRaised);
		}

		#endregion

		[ExcludeFromCodeCoverage]
		internal void ValidateConsistency()
		{
			IList source = _sourceScalar.getValue(_source, new ObservableCollection<object>()) as IList;
			// ReSharper disable once PossibleNullReferenceException
			if (Count != source.Count) throw new ObservableComputationsException(this, "Consistency violation: Casting.1");

			for (int i = 0; i < source.Count; i++)
			{
				object sourceItem = source[i];
				TResultItem resultItem = this[i];

				if (!resultItem.IsSameAs(sourceItem)) throw new ObservableComputationsException(this, "Consistency violation: Casting.2");
			}
		}
	}
}
