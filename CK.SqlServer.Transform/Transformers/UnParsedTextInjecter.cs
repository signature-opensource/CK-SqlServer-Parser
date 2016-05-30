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
    public class UnParsedTextInjecter : SqlNodeLocationVisitor
    {
        readonly LocationInserter _matcher;
        readonly string _before;
        readonly string _after;
        readonly SqlTNodeSimplePattern _nodePattern;
        // The little magic to promote SelectSpec match to
        // its SelectDecorator.
        // When descending, we capture the to level decorator at its position so that
        // we know that matching at this exact position is useless: only a SelectSpec (or
        // another decorator) may match and if it is the case, we do not want it to match!
        readonly Stack<KeyValuePair<SelectDecorator, int>> _selectDecorator;

        public UnParsedTextInjecter( SqlTInject injecter )
        {
            if( injecter == null ) throw new ArgumentNullException( nameof( injecter ) );
            _matcher = new LocationInserter( injecter );
            _before = injecter.TextBefore;
            _after = injecter.TextAfter;

            _nodePattern = injecter.Location.Pattern as SqlTNodeSimplePattern;
            if( _nodePattern != null && _nodePattern.IsMatchPart )
            {
                _selectDecorator = new Stack<KeyValuePair<SelectDecorator, int>>();
            }
        }

        protected override bool BeforeVisitItem()
        {
            if( _selectDecorator != null )
            {
                var d = VisitContext.VisitedNode as SelectDecorator;
                if( d != null 
                    && (_selectDecorator.Count == 0 || _selectDecorator.Peek().Value != VisitContext.Position ) )
                {
                    _selectDecorator.Push( new KeyValuePair<SelectDecorator, int>( d, VisitContext.Position ) );
                }
            }
            return true;
        }

        protected override ISqlNode AfterVisitItem( ISqlNode e )
        {
            e = HandleNode( e );
            if( VisitContext.Depth == 0 )
            {
                if( _matcher.MatchCount == 0 )
                {
                    Monitor.Error().Send( $"Not found: '{_matcher.InsertClause}'." );
                }
                else if( _matcher.ExpectedMatchCount != 0 && _matcher.MatchCount < _matcher.ExpectedMatchCount )
                {
                    Monitor.Error().Send( $"Missing matches in '{_matcher.InsertClause}', expecting {_matcher.ExpectedMatchCount}, found {_matcher.MatchCount}." );
                }
                else if( _matcher.RequiresConclude )
                {
                    var m = _matcher.Conclude();
                    if( m != null )
                    {
                        var toChange = VisitContext.LocationManager.GetQualifiedLocation( m.Position, m.Node );
                        var oldLeaf = toChange.Node;
                        var newLeaf = m.Apply( _before, _after );
                        while( (toChange = toChange.Parent) != null )
                        {
                            newLeaf = toChange.Node.ReplaceContentNode( ( n, i ) => n == oldLeaf ? newLeaf : n );
                            oldLeaf = toChange.Node;
                        }
                        SetHasUnParsedText();
                        return newLeaf;
                    }
                }
            }
            return e;
        }

        int _previousMatchPos;

        bool HandleDecoratorCovering()
        {
            if( _selectDecorator == null || _selectDecorator.Count == 0 ) return true;
            var h = _selectDecorator.Peek();
            if( h.Value == VisitContext.Position )
            {
                if( h.Key == VisitContext.VisitedNode )
                {
                    _selectDecorator.Pop();
                    return true;
                }
                return false;
            }
            return true;
        }

        private ISqlNode HandleNode( ISqlNode e )
        {
            if( _matcher.CanStop ) return e;
            if( _nodePattern != null )
            {
                if( !HandleDecoratorCovering()
                    || VisitContext.Position < _previousMatchPos
                    || !_nodePattern.Match( e ) )
                {
                    return e;
                }
                _previousMatchPos = VisitContext.Position + e.Width;
            }
            var m = _matcher.AddCandidate( Monitor, VisitContext.Position, e );
            if( m != null )
            {
                e = m.Apply( _before, _after );
                if( _matcher.CanStop ) StopVisit( true );
                else SetHasUnParsedText();
            }
            return e;
        }
    }
}
