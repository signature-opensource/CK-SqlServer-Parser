using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    class LocationInserterTrivia
    {
        readonly FIFOBuffer<MatchedNode> _lastBuffer;
        readonly Func<SqlTrivia, bool> _matcher;
        readonly int _targetMatchCount;
        readonly int _expectedMatchCount;
        readonly bool _fromFirst;
        readonly bool _all;
        bool _hasError;
        int _matchCount;

        public class MatchedNode
        {
            public readonly int Position;
            public readonly ISqlNode Node;
            public readonly IReadOnlyList<int> IdxTrivias;

            public MatchedNode( int p, ISqlNode n, IReadOnlyList<int> t )
            {
                Position = p;
                Node = n;
                IdxTrivias = t;
            }

            public ISqlNode Apply( string before, string after )
            {
                var e = Node;
                int deltaInsert = 0;
                bool inTrailing = false;
                if( IdxTrivias == null )
                {
                    if( before != null ) e = e.AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, before ) );
                    if( after != null ) e = e.AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, after ) );
                    return e;
                }
                foreach( int idx in IdxTrivias )
                {
                    ImmutableList<SqlTrivia> trivias;
                    int actualIdx;
                    if( idx >= 0 )
                    {
                        trivias = e.LeadingTrivias;
                        actualIdx = idx + deltaInsert;
                     }
                    else
                    {
                        if( !inTrailing )
                        {
                            inTrailing = true;
                            deltaInsert = 0;
                        }
                        trivias = e.TrailingTrivias;
                        actualIdx = ~idx + deltaInsert;
                    }
                    if( before != null )
                    {
                        trivias = trivias.Insert( actualIdx++, new SqlTrivia( SqlTokenType.None, before ) );
                        ++deltaInsert;
                    }
                    if( after != null )
                    {
                        trivias = trivias.Insert( actualIdx + 1, new SqlTrivia( SqlTokenType.None, after ) );
                        ++deltaInsert;
                    }
                    e = idx >= 0 ? e.SetTrivias( trivias, e.TrailingTrivias ) : e.SetTrivias( e.LeadingTrivias, trivias );
                }
                return e;
            }
        }

        public LocationInserterTrivia( SqlTInsert ins )
        {
            InsertClause = ins;

            var t = ins.Location.Pattern as ISqlHasStringValue;
            if( t != null )
            {
                if( t.Value.StartsWith( "--" ) )
                {
                    string lineComment = t.Value.Substring( 2 ).Trim();
                    _matcher = trivia => trivia.TokenType == SqlTokenType.LineComment && trivia.Text.TrimStart().StartsWith( lineComment );
                }
                else
                {
                    Debug.Assert( t.Value.StartsWith( "/*" ) && t.Value.EndsWith( "*/" ) );
                    string starComment = t.Value.Substring( 2, t.Value.Length-4 ).Trim();
                    _matcher = trivia => trivia.TokenType == SqlTokenType.StarComment && trivia.Text.Contains( starComment );
                }
            }
            if( ins.Location.FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.Single )
            {
                _expectedMatchCount = 1;
                _fromFirst = true;
            }
            else
            {
                _expectedMatchCount = ins.Location.ExpectedMatchCount?.Value ?? 0;
                if( ins.Location.FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.All )
                {
                    _fromFirst = _all = true;
                }
                else if( ins.Location.FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.First )
                {
                    _fromFirst = true;
                }
            }
            int index = ins.Location.Offset ?.Value ?? 0;
            if( _fromFirst ) _targetMatchCount = index + 1;
            else _lastBuffer = new FIFOBuffer<MatchedNode>( index + 1 );
        }

        public readonly SqlTInsert InsertClause;

        public int MatchCount => _matchCount;

        /// <summary>
        /// Gets the expected match count. Zero when not applicable.
        /// </summary>
        public int ExpectedMatchCount => _expectedMatchCount;

        public bool CanStop => _hasError || (_fromFirst && _expectedMatchCount == 0 && !_all && _matchCount == _targetMatchCount);

        public bool RequiresConclude => !_fromFirst;

        public MatchedNode AddCandidate( IActivityMonitor monitor, int position, ISqlNode n )
        {
            List<int> matchPos = null;
            if( _matcher == null )
            {
                if( !HandleMatchCount( monitor, ref matchPos, int.MaxValue ) ) return null;
            }
            else
            {
                int idx = 0;
                foreach( var t in n.LeadingTrivias ) 
                {
                    if( _matcher( t ) && !HandleMatchCount( monitor, ref matchPos, idx ) && _hasError ) return null;
                    ++idx;
                }
                idx = 0;
                foreach( var t in n.TrailingTrivias )
                {
                    if( _matcher( t ) && !HandleMatchCount( monitor, ref matchPos, ~idx ) && _hasError ) return null;
                    ++idx;
                }
                if( matchPos == null ) return null;
            }
            MatchedNode m = new MatchedNode( position, n, matchPos );
            if( _lastBuffer != null )
            {
                _lastBuffer.Push( m );
                return null;
            }
            return m;
        }

        bool HandleMatchCount( IActivityMonitor monitor, ref List<int> matchPos, int idx = int.MaxValue )
        {
            if( ++_matchCount > 1 && (_expectedMatchCount > 0 && _matchCount > _expectedMatchCount) )
            {
                monitor.Error().Send( $"Too many matches found for: '{InsertClause}'. Max is {_expectedMatchCount}." );
                _hasError = true;
            }
            else if( !_fromFirst || (_all || _matchCount == _targetMatchCount) )
            {
                if( idx != int.MaxValue )
                {
                    if( matchPos == null ) matchPos = new List<int>();
                    matchPos.Add( idx );
                }
                return true;
            }
            return false;
        }

        public MatchedNode Conclude()
        {
            Debug.Assert( !_fromFirst );
            if( _matchCount < _lastBuffer.Capacity ) return null;
            int targetIdxFromLast = _lastBuffer.Capacity - 1;
            int iNode = _lastBuffer.Count - 1;
            MatchedNode m;
            while( (m = _lastBuffer[iNode]).IdxTrivias.Count <= targetIdxFromLast )
            {
                targetIdxFromLast -= m.IdxTrivias.Count;
                --iNode;
            }
            if( m.IdxTrivias.Count == 1 ) return m;
            return new MatchedNode( m.Position, m.Node, new[] { m.IdxTrivias[m.IdxTrivias.Count - 1 - targetIdxFromLast] } );
        }

    }


}
