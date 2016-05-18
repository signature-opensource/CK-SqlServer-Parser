using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    class TriviaLocationMatcher
    {
        readonly string _before;
        readonly string _after;
        readonly Func<SqlTrivia, bool> _matcher;
        readonly bool _fromFirst;
        readonly bool _checkSingle;

        struct Match
        {
            public int Position;
            public ISqlNode Node;
            public int IdxTrivia;
            public bool IsLeading;

            internal ISqlNode Apply( ISqlNode e, string before, string after )
            {
                var trivias = IsLeading ? e.LeadingTrivias : e.TrailingTrivias;
                if( before != null )
                {
                    trivias = trivias.Insert( IdxTrivia, new SqlTrivia( SqlTokenType.None, before ) );
                }
                if( after != null )
                {
                    trivias = trivias.Insert( IdxTrivia + 1, new SqlTrivia( SqlTokenType.None, after ) );
                }
                return IsLeading ? e.SetTrivias( trivias, e.TrailingTrivias ) : e.SetTrivias( e.LeadingTrivias, trivias );
            }
        }

        readonly FIFOBuffer<Match> _fromLast;
        Match _success;
        int _remainingMatchCount;
        bool _hasError;

        public TriviaLocationMatcher( SqlTInsert ins )
        {
            InsertClause = ins;
            if( ins.Location.IsBefore ) _before = ins.TextContent;
            else _after = ins.TextContent;

            ISqlHasStringValue t = (ISqlHasStringValue)ins.Location.RangeOrString;
            if( !t.Value.StartsWith( "--" ) ) throw new ArgumentException( "Must be a line comment" );
            string lineComment = t.Value.Substring( 2 );
            _matcher = trivia => trivia.TokenType == SqlTokenType.LineComment && trivia.Text == lineComment;

            int index = 0;
            if( ins.Location.FirstOrLastOrSingleT != null )
            {
                _checkSingle = ins.Location.FirstOrLastOrSingleT.TokenType == SqlTokenType.Single;
                _fromFirst = _checkSingle || ins.Location.FirstOrLastOrSingleT.TokenType == SqlTokenType.First;
                if( ins.Location.Offset != null ) index = ins.Location.Offset.Value;
            }
            else _fromFirst = true;

            if( _fromFirst ) _remainingMatchCount = index;
            else _fromLast = new FIFOBuffer<Match>( index + 1 );
        }

        public readonly SqlTInsert InsertClause;

        public bool Found => _success.Node != null;

        public ISqlNode GetResult( ISqlNode e )
        {
            Debug.Assert( Found );
            return _success.Apply( e, _before, _after );
        }

        public bool AddCandidate( IActivityMonitor monitor, int position, ISqlNode n )
        {
            Match m = DoMatch( position, n );
            if( m.Node != null )
            {
                if( _fromFirst )
                {
                    if( _checkSingle && _success.Node != null )
                    {
                        _hasError = true;
                        monitor.Error().Send( $"Multiple match found for: '{InsertClause.ToStringHyperCompact()}'." );
                    }
                    else if( --_remainingMatchCount < 0 )
                    {
                        _success = m;
                        return true;
                    }
                }
                else
                {
                    _fromLast.Push( m );
                }
            }
            return false;
        }

        public bool CanStop => _hasError || (_success.Node != null && !_checkSingle);

        public bool RequiresConclude => !_fromFirst;

        internal SqlNodeLocation Conclude( ISqlNodeLocationManager ns )
        {
            Debug.Assert( !_fromFirst );
            if( _fromLast.Count == _fromLast.Capacity )
            {
                _success = _fromLast.Peek();
            }
            return Found ? ns.GetQualifiedLocation( _success.Position, _success.Node ) : null;
        }

        Match DoMatch( int position, ISqlNode n )
        {
            int idx = n.LeadingTrivias.IndexOf( _matcher );
            if( idx >= 0 ) return new Match() { Position = position, Node = n, IdxTrivia = idx, IsLeading = true };
            idx = n.TrailingTrivias.IndexOf( _matcher );
            if( idx >= 0 ) return new Match() { Position = position, Node = n, IdxTrivia = idx, IsLeading = false };
            return new Match();
        }
    }


}
