﻿// Copyright (c) 2019-2021 Buchelnikov Igor Vladimirovich. All rights reserved
// Buchelnikov Igor Vladimirovich licenses this file to you under the MIT license.
// The LICENSE file is located at https://github.com/IgorBuchelnikov/ObservableComputations/blob/master/LICENSE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace ObservableComputations
{
	public class IndicesComputing<TSourceItem> : Selecting<ZipPair<int, TSourceItem>, int>, IHasSources
	{
		private readonly Expression<Func<TSourceItem, bool>> _predicateExpression;
		private readonly IReadScalar<INotifyCollectionChanged> _sourceScalar;
		private readonly INotifyCollectionChanged _sourceIndicesComputing;

		// ReSharper disable once MemberCanBePrivate.Global
		public Expression<Func<TSourceItem, bool>> PredicateExpression => _predicateExpression;

		// ReSharper disable once MemberCanBePrivate.Global
		public override IReadScalar<INotifyCollectionChanged> SourceScalar => _sourceScalar;

		// ReSharper disable once MemberCanBePrivate.Global
		public override INotifyCollectionChanged Source => _sourceIndicesComputing;

		public override int InitialCapacity => ((IHasInitialCapacity)_source).InitialCapacity;

		public override ReadOnlyCollection<object> Sources => new ReadOnlyCollection<object>(new object[]{Source, SourceScalar});

		[ObservableComputationsCall]
		public IndicesComputing(
			IReadScalar<INotifyCollectionChanged> sourceScalar, 
			Expression<Func<TSourceItem, bool>> predicateExpression,
			int initialCapacity = 0) : base(getSource(sourceScalar, predicateExpression, initialCapacity), pair => pair.LeftItem) 
		{
			_predicateExpression = predicateExpression;
			_sourceScalar = sourceScalar;
		}

		[ObservableComputationsCall]
		public IndicesComputing(
			INotifyCollectionChanged source, 
			Expression<Func<TSourceItem, bool>> predicateExpression,
			int initialCapacity = 0) : base(getSource(source, predicateExpression, initialCapacity), pair => pair.LeftItem) 
		{
			_predicateExpression = predicateExpression;
			_sourceIndicesComputing = source;
		}

		private static INotifyCollectionChanged getSource(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			Expression<Func<TSourceItem, bool>> predicateExpression,
			int initialCapacity)
		{
			Expression<Func<ZipPair<int, TSourceItem>, bool>> zipPairPredicateExpression = getZipPairPredicateExpression(predicateExpression);

			return new Computing<int>(() => sourceScalar.Value != null ? ((IList) sourceScalar.Value).Count : 0).SequenceComputing()
				.Zipping<int, TSourceItem>(sourceScalar)
				.Filtering(zipPairPredicateExpression, initialCapacity);
		}

		private static INotifyCollectionChanged getSource(
			INotifyCollectionChanged source,
			Expression<Func<TSourceItem, bool>> predicateExpression,
			int initialCapacity)
		{
			Expression<Func<ZipPair<int, TSourceItem>, bool>> zipPairPredicateExpression = getZipPairPredicateExpression(predicateExpression);

			return new Computing<int>(() => ((IList) source).Count).SequenceComputing()
				.Zipping<int, TSourceItem>(source)
				.Filtering(zipPairPredicateExpression, initialCapacity);
		}

		private static Expression<Func<ZipPair<int, TSourceItem>, bool>> getZipPairPredicateExpression(Expression<Func<TSourceItem, bool>> predicateExpression)
		{
			ParameterExpression zipPairParameterExpression
				= Expression.Parameter(typeof(ZipPair<int, TSourceItem>), "zipPair");
			Expression zipPairItem2Expression
				= Expression.PropertyOrField(
					zipPairParameterExpression,
					nameof(ZipPair<int, TSourceItem>.RightItem));
			ReplaceParameterVisitor replaceParameterVisitor
				= new ReplaceParameterVisitor(
					predicateExpression.Parameters[0],
					zipPairItem2Expression);
			Expression<Func<ZipPair<int, TSourceItem>, bool>> zipPairPredicateExpression
				= Expression.Lambda<Func<ZipPair<int, TSourceItem>, bool>>(
					replaceParameterVisitor.Visit(predicateExpression.Body),
					zipPairParameterExpression);
			return zipPairPredicateExpression;
		}

		[ExcludeFromCodeCoverage]
		internal new void ValidateInternalConsistency()
		{
			IList<TSourceItem> source = _sourceScalar.getValue(_sourceIndicesComputing, new ObservableCollection<TSourceItem>()) as IList<TSourceItem>;
			Func<TSourceItem, bool> predicate = _predicateExpression.Compile();

			List<int> result = new List<int>();

			// ReSharper disable once PossibleNullReferenceException
			for (int i = 0; i < source.Count; i++)
				if (predicate(source[i]))
					result.Add(i);

			if (!this.SequenceEqual(result)) throw new ValidateInternalConsistencyException("Consistency violation: IndicesComputing.1");
		}

		//private class FindExpressionVisitor : ExpressionVisitor
		//{
		//	private List<Expression> _result = new List<Expression>();
		//	public IEnumerable<Expression> Result => _result;

		//	Func<Expression, (bool, bool)> _predicate;
		//	public FindExpressionVisitor(Func<Expression, (bool, bool)> predicate)
		//	{
		//		_predicate = predicate;
		//	}

		//	public override Expression Visit(Expression node)
		//	{
		//		(bool includeInResult, bool stopFind) = _predicate(node);

		//		if (includeInResult) _result.Add(node);
		//		if (stopFind) return null;

		//		return base.Visit(node);
		//	}
		//}
	}
}
