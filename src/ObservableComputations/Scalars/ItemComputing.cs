﻿// Copyright (c) 2019-2021 Buchelnikov Igor Vladimirovich. All rights reserved
// Buchelnikov Igor Vladimirovich licenses this file to you under the MIT license.
// The LICENSE file is located at https://github.com/IgorBuchelnikov/ObservableComputations/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ObservableComputations
{
	public class ItemComputing<TSourceItem> : ScalarComputing<TSourceItem>, IHasSourceCollections, ISourceIndexerPropertyTracker, ISourceCollectionChangeProcessor
	{
		public virtual IReadScalar<INotifyCollectionChanged> SourceScalar => _sourceScalar;

		// ReSharper disable once MemberCanBePrivate.Global
		public IReadScalar<int> IndexScalar => _indexScalar;

		public virtual INotifyCollectionChanged Source => _source;

		// ReSharper disable once MemberCanBePrivate.Global
		public int Index => _index;

		// ReSharper disable once MemberCanBeProtected.Global
		public TSourceItem DefaultValue => _defaultValue;

		public virtual ReadOnlyCollection<INotifyCollectionChanged> Sources => new ReadOnlyCollection<INotifyCollectionChanged>(new []{Source});
		public virtual ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>> SourceScalars => new ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>>(new []{SourceScalar});

		protected readonly IReadScalar<INotifyCollectionChanged> _sourceScalar;

		protected INotifyCollectionChanged _source;
		private IList<TSourceItem> _sourceAsList;

		private readonly IReadScalar<int> _indexScalar;
		private int _index;
		internal readonly TSourceItem _defaultValue;

		private bool _indexerPropertyChangedEventRaised;
		private INotifyPropertyChanged _sourceAsINotifyPropertyChanged;

		private IHasChangeMarker _sourceAsIHasChangeMarker;
		private bool _lastProcessedSourceChangeMarker;

		List<TSourceItem> _sourceCopy;

		private readonly ISourceCollectionChangeProcessor _thisAsSourceCollectionChangeProcessor;
		readonly Action _changeIndexAction;

		private void initializeIndexScalar()
		{
			if (_indexScalar != null)
			{
				_indexScalar.PropertyChanged += HandleIndexScalarChanged;
				_index = _indexScalar.Value;
			}

		}

		[ObservableComputationsCall]
		public ItemComputing(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			int index, 
			TSourceItem defaultValue = default(TSourceItem)) : this()
		{
			_sourceScalar = sourceScalar;
			//initializeSourceScalar();

			_index = index;
			_defaultValue = defaultValue;
			//initializeFromSource();
		}

		[ObservableComputationsCall]
		public ItemComputing(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			IReadScalar<int> indexScalar, 
			TSourceItem defaultValue = default) : this()
		{
			_sourceScalar = sourceScalar;
			//initializeSourceScalar();

			_defaultValue = defaultValue;

			_indexScalar = indexScalar;
			//initializeIndexScalar();

			//initializeFromSource();
		}

		[ObservableComputationsCall]
		public ItemComputing(
			INotifyCollectionChanged source,
			int index, 
			TSourceItem defaultValue = default) : this()
		{
			_source = source;
			_index = index;
			_defaultValue = defaultValue;
			//initializeFromSource();
		}

		[ObservableComputationsCall]
		public ItemComputing(
			INotifyCollectionChanged source,
			IReadScalar<int> indexScalar, 
			TSourceItem defaultValue = default) : this()
		{
			_source = source;
			_defaultValue = defaultValue;
			_indexScalar = indexScalar;
			
			//initializeIndexScalar();
			//initializeFromSource();
		}

		private ItemComputing()
		{
			_thisAsSourceCollectionChangeProcessor = this;
			_changeIndexAction = () =>
			{
				_index = _indexScalar.Value;
				recalculateValue();
			};
			_deferredQueuesCount = 2;
		}


		private void HandleIndexScalarChanged(object sender, PropertyChangedEventArgs e)
		{
			Utils.processChange(
				sender, 
				e, 
				_changeIndexAction,
				ref _isConsistent, 
				ref _handledEventSender, 
				ref _handledEventArgs, 
				0, _deferredQueuesCount,
				ref _deferredProcessings, this);
		}

		protected override void processSource()
		{
			if (_sourceReadAndSubscribed)
			{
				_source.CollectionChanged -= handleSourceCollectionChanged;

				if (_sourceAsINotifyPropertyChanged != null)
				{
					_sourceAsINotifyPropertyChanged.PropertyChanged -=
						((ISourceIndexerPropertyTracker) this).HandleSourcePropertyChanged;
					_sourceAsINotifyPropertyChanged = null;
				}

				_sourceCopy = null;

				_sourceReadAndSubscribed = false;
			}

			Utils.changeSource(ref _source, _sourceScalar, _downstreamConsumedComputings, _consumers, this,
				out _sourceAsList, true);


			if (_source != null && _isActive)
			{
				Utils.initializeFromHasChangeMarker(
					out _sourceAsIHasChangeMarker, 
					_sourceAsList, 
					ref _lastProcessedSourceChangeMarker, 
					ref _sourceAsINotifyPropertyChanged,
					(ISourceIndexerPropertyTracker)this);

				_source.CollectionChanged += handleSourceCollectionChanged;
				_sourceCopy = new List<TSourceItem>(_sourceAsList);
				recalculateValue();

				_sourceReadAndSubscribed = true;
			}
			else
				setDefaultValue(_defaultValue);
		}

		private void recalculateValue()
		{
			if (_sourceCopy != null && _sourceCopy.Count > _index)
			{
				if (_isDefaulted)
				{
					_isDefaulted = false;
					raisePropertyChanged(Utils.IsDefaultedPropertyChangedEventArgs);
				}

				setValue(_sourceCopy[_index]);
			}
			else
			{
				if (!_isDefaulted)
				{
					_isDefaulted = true;
					raisePropertyChanged(Utils.IsDefaultedPropertyChangedEventArgs);
				}

				setValue(_defaultValue);
			}
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
					//if (e.NewItems.Count > 1) throw new Exception("Adding of multiple items is not supported");
					_sourceCopy.Insert(e.NewStartingIndex, (TSourceItem) e.NewItems[0]);
					if (e.NewStartingIndex <= _index) recalculateValue();
					break;
				case NotifyCollectionChangedAction.Remove:
					//if (e.OldItems.Count > 1) throw new Exception("Removing of multiple items is not supported");
					_sourceCopy.RemoveAt(e.OldStartingIndex);
					if (e.OldStartingIndex <= _index) recalculateValue();
					break;
				case NotifyCollectionChangedAction.Replace:
					//if (e.NewItems.Count > 1) throw new Exception("Replacing of multiple items is not supported");
					_sourceCopy[e.NewStartingIndex] = (TSourceItem) e.NewItems[0];
					if (e.OldStartingIndex == _index) recalculateValue();
					break;
				case NotifyCollectionChangedAction.Move:
					int oldStartingIndex = e.OldStartingIndex;
					int newStartingIndex = e.NewStartingIndex;
					if (newStartingIndex == oldStartingIndex) return;

					TSourceItem sourceItem = (TSourceItem) e.NewItems[0];
					_sourceCopy.RemoveAt(oldStartingIndex);
					_sourceCopy.Insert(newStartingIndex, sourceItem);

					if (newStartingIndex < oldStartingIndex)
					{
						if (_index >= newStartingIndex && _index <= oldStartingIndex)
							setValue(_sourceCopy[_index]);
					}
					else
					{
						if (_index >= oldStartingIndex && _index <= newStartingIndex)
							setValue(_sourceCopy[_index]);
					}

					break;
				case NotifyCollectionChangedAction.Reset:
					processSource();
					break;
			}
		}

		internal override void addToUpstreamComputings(IComputingInternal computing)
		{
			Utils.AddDownstreamConsumedComputing(computing, _sourceScalar, _source);
			(_indexScalar as IComputingInternal)?.AddDownstreamConsumedComputing(computing);
		}

		internal override void removeFromUpstreamComputings(IComputingInternal computing)		
		{
			Utils.RemoveDownstreamConsumedComputing(computing, _sourceScalar, _source);
			(_indexScalar as IComputingInternal)?.RemoveDownstreamConsumedComputing(computing);
		}

		protected override void initialize()
		{
			Utils.initializeSourceScalar(_sourceScalar, ref _source, scalarValueChangedHandler);
			initializeIndexScalar();
		}

		protected override void uninitialize()
		{
			Utils.uninitializeSourceScalar(_sourceScalar, scalarValueChangedHandler, ref _source);
			if (_indexScalar != null) 
				_indexScalar.PropertyChanged -= HandleIndexScalarChanged;
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
			IList<TSourceItem> source = _sourceScalar.getValue(_source, new ObservableCollection<TSourceItem>()) as IList<TSourceItem>;
			int index = _indexScalar.getValue(_index);
			TSourceItem defaultValue = _defaultValue;

			// ReSharper disable once PossibleNullReferenceException
			if (source.Count > index)
			{
				if (!source[index].IsSameAs(_value))
					throw new ObservableComputationsException(this, "Consistency violation: ItemComputing.1");
			}
			else
			{
				if (!defaultValue.IsSameAs(_value))
					throw new ObservableComputationsException(this, "Consistency violation: ItemComputing.2");			
			}
		}

	}
}
