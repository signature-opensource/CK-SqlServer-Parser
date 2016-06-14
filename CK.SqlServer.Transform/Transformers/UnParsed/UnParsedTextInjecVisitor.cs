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
    public class UnParsedTextInjecVisitor : SqlNodeLocationVisitor
    {
        readonly UnparsedInjectInfo _info;
        readonly LocationInserter _inserter;
        // The little magic to promote SelectSpec match to
        // its SelectDecorator.
        // When descending, we capture the top level decorator at its position so that
        // we know that matching at this exact position is useless: only a SelectSpec (or
        // another decorator) may match and if it is the case, we do not want it to match!
        readonly Stack<KeyValuePair<SelectDecorator, int>> _selectDecorator;

        internal UnParsedTextInjecVisitor( UnparsedInjectInfo injecter )
        {
            _info = injecter;
            _inserter = new LocationInserter( _info.Location );
            if( _info.Location.NodeMatcher != null )
            {
                if( _info.Location.IsNodeMatchRange ) throw new ArgumentException();
                if( _info.Location.IsNodeMatchPart )
                {
                    _selectDecorator = new Stack<KeyValuePair<SelectDecorator, int>>();
                }
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
                if( _inserter.MatchCount == 0 )
                {
                    Monitor.Error().Send( $"Pattern not found." );
                }
                else if( _inserter.ExpectedMatchCount != 0 && _inserter.MatchCount < _inserter.ExpectedMatchCount )
                {
                    Monitor.Error().Send( $"Missing matches: expecting {_inserter.ExpectedMatchCount}, found {_inserter.MatchCount}." );
                }
                else if( _inserter.RequiresConclude )
                {
                    var m = _inserter.Conclude();
                    if( m != null )
                    {
                        var toChange = VisitContext.LocationManager.GetQualifiedLocation( m.Position, m.Node );
                        SetHasUnParsedText();
                        return toChange.ChangeNode( m.Apply( Monitor, _info.TextBefore, _info.TextAfter, _info.ClearStarComments, null ) );
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
            if( _inserter.CanStop ) return e;
            if( _info.Location.NodeMatcher != null )
            {
                if( !HandleDecoratorCovering()
                    || VisitContext.Position < _previousMatchPos
                    || !_info.Location.NodeMatcher( e ) )
                {
                    return e;
                }
                _previousMatchPos = VisitContext.Position + e.Width;
            }
            var m = _inserter.AddCandidate( Monitor, VisitContext.Position, e );
            if( m != null )
            {
                e = m.Apply( Monitor, _info.TextBefore, _info.TextAfter, _info.ClearStarComments, null );
                if( _inserter.CanStop ) StopVisit( true );
                else SetHasUnParsedText();
            }
            return e;
        }
    }
}
