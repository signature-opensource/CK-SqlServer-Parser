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
    public class InsertUnParsedTextAroundTrivia : SqlNodeLocationVisitor
    {
        readonly TriviaLocationMatcher _matcher;

        public InsertUnParsedTextAroundTrivia( SqlTInsert insertInTrivia )
        {
            if( insertInTrivia == null ) throw new ArgumentNullException( nameof( insertInTrivia ) );
            _matcher = new TriviaLocationMatcher( insertInTrivia );
        }

        ISqlNode _foundNode;

        protected override bool BeforeVisitItem()
        {
            if( _matcher.AddCandidate( Monitor, VisitContext.Position, VisitContext.VisitedNode ) )
            {
                _foundNode = VisitContext.VisitedNode;
            }
            if( _matcher.CanStop ) StopVisit( true );
            else SetHasUnParsedText();
            // If we called StopVisit, returning true below does not trigger
            // the visit of the children.
            return true;
        }

        protected override ISqlNode AfterVisitItem( ISqlNode e )
        {
            if( VisitContext.VisitedNode == _foundNode ) e = _matcher.GetResult( e );
            if( _matcher.CanStop ) return e;
            if( VisitContext.Depth == 0 )
            {
                if( _matcher.RequiresConclude )
                {
                    var toChange = _matcher.Conclude( VisitContext.LocationManager );
                    if( toChange != null )
                    {
                        var oldLeaf = toChange.Node;
                        var newLeaf = _matcher.GetResult( toChange.Node );
                        while( (toChange = toChange.Parent) != null )
                        {
                            newLeaf = toChange.Node.ReplaceContentNode( ( n, i ) => n == oldLeaf ? newLeaf : n );
                            oldLeaf = toChange.Node;
                        }
                        return newLeaf;
                    }
                }
                if( !_matcher.Found ) Monitor.Error().Send( $"Not found: '{_matcher.InsertClause}'." );
            }
            return e;
        }

    }
}
