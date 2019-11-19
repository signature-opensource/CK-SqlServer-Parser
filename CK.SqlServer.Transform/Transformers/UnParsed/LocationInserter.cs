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
    class LocationInserter
    {
        readonly LocationInfo _finderInfo;
        readonly FIFOBuffer<MatchedNode> _lastBuffer;
        int _matchCount;
        bool _hasError;

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

            public ISqlNode Apply( IActivityMonitor monitor, string before, string after, bool clearStarComments, SqlTrivia[] triviaReplacement )
            {
                var e = Node;
                if( clearStarComments )
                {
                    var cleaner = new TriviaCleaner( false, true, true ) { Monitor = monitor };
                    e = cleaner.VisitRoot( e );
                }
                int deltaInsert = 0;
                bool inTrailing = false;
                if( IdxTrivias == null )
                {
                    Debug.Assert( triviaReplacement == null && (after != null || before != null), "When matching nodes, only before/after are handled." );
                    ImmutableList<SqlTrivia> leading, trailing;
                    if( clearStarComments )
                    {
                        e = e.LiftBothTrivias();
                        leading = before != null
                                    ? e.LeadingTrivias.Add( new SqlTrivia( SqlTokenType.None, before ) )
                                    : e.LeadingTrivias;
                        trailing = after != null
                                    ? e.TrailingTrivias.Insert( 0, new SqlTrivia( SqlTokenType.None, after ) )
                                    : e.TrailingTrivias;
                    }
                    else
                    {
                        leading = before != null 
                                    ? e.LeadingTrivias.Insert( 0, new SqlTrivia( SqlTokenType.None, before ) ) 
                                    : e.LeadingTrivias;
                        trailing = after != null 
                                    ? e.TrailingTrivias.Add( new SqlTrivia( SqlTokenType.None, after ) )
                                    : e.TrailingTrivias;
                    }
                    return e.SetTrivias( leading, trailing );
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
                    if( triviaReplacement != null )
                    {
                        Debug.Assert( before == null && after == null );
                        trivias = trivias.RemoveAt( actualIdx );
                        trivias = trivias.InsertRange( actualIdx, triviaReplacement );
                        // Replacing trivias is only for first autoclosed extension tag injection.
                        // Updating the deltaInsert is actually useless but this is clearer
                        // to keep this.
                        deltaInsert += 2;
                    }
                    else
                    {
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
                    }
                    e = idx >= 0 ? e.SetTrivias( trivias, e.TrailingTrivias ) : e.SetTrivias( e.LeadingTrivias, trivias );
                }
                return e;
            }
        }

        public LocationInserter( in LocationInfo finderInfo )
        {
            _finderInfo = finderInfo;
            if( !_finderInfo.Card.FromFirst ) _lastBuffer = new FIFOBuffer<MatchedNode>( _finderInfo.Card.Offset + 1 );
        }

        public int MatchCount => _matchCount;

        /// <summary>
        /// Gets the expected total match count. 
        /// Zero when not applicable (when there is no 'out of n' specified).
        /// </summary>
        public int ExpectedMatchCount => _finderInfo.Card.ExpectedMatchCount;

        public bool CanStop => _hasError 
                                || (_finderInfo.Card.FromFirst 
                                    && _finderInfo.Card.ExpectedMatchCount == 0 
                                    && !_finderInfo.Card.All 
                                    && _matchCount == _finderInfo.Card.Offset + 1);

        public bool RequiresConclude => !_finderInfo.Card.FromFirst && !_hasError;

        public MatchedNode AddCandidate( IActivityMonitor monitor, int position, ISqlNode n )
        {
            List<int> matchPos = null;
            if( _finderInfo.TriviaMatcher == null )
            {
                if( !HandleMatchCount( monitor, ref matchPos, int.MaxValue ) ) return null;
            }
            else
            {
                int idx = 0;
                foreach( var t in n.LeadingTrivias ) 
                {
                    if( _finderInfo.TriviaMatcher( t ) && !HandleMatchCount( monitor, ref matchPos, idx ) && _hasError ) return null;
                    ++idx;
                }
                idx = 0;
                foreach( var t in n.TrailingTrivias )
                {
                    if( _finderInfo.TriviaMatcher( t ) && !HandleMatchCount( monitor, ref matchPos, ~idx ) && _hasError ) return null;
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
            if( ++_matchCount > 1 && (_finderInfo.Card.ExpectedMatchCount > 0 && _matchCount > _finderInfo.Card.ExpectedMatchCount) )
            {
                monitor.Error( $"Too many matches found for (max is {_finderInfo.Card.ExpectedMatchCount})." );
                _hasError = true;
            }
            else if( !_finderInfo.Card.FromFirst || (_finderInfo.Card.All || _matchCount == _finderInfo.Card.Offset + 1) )
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
            Debug.Assert( !_finderInfo.Card.FromFirst );
            if( _matchCount < _lastBuffer.Capacity ) return null;
            if( _finderInfo.TriviaMatcher == null )
            {
                return _lastBuffer.PeekLast();
            }
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
