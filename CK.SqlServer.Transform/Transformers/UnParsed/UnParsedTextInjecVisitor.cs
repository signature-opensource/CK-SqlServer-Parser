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
        readonly DepthFirstNodeMatcherHelper _nodeMatcher;


        internal UnParsedTextInjecVisitor( UnparsedInjectInfo injecter )
        {
            _info = injecter;
            _inserter = new LocationInserter( _info.Location );
            if( _info.Location.NodeMatcher != null )
            {
                if( _info.Location.IsNodeMatchRange ) throw new ArgumentException();
                _nodeMatcher = new DepthFirstNodeMatcherHelper( _info.Location.IsNodeMatchPart, _info.Location.NodeMatcher );
            }
        }

        protected override bool BeforeVisitItem()
        {
            if( _nodeMatcher != null ) _nodeMatcher.OnBeforeVisitItem( VisitContext );
            return true;
        }

        protected override ISqlNode AfterVisitItem( ISqlNode e )
        {
            Debug.Assert( e.Width == VisitContext.VisitedNode.Width );
            e = HandleNode( e );
            if( VisitContext.Depth == 0 )
            {
                if( _inserter.MatchCount == 0 )
                {
                    Monitor.Error( $"Unable to find: " + _info.Location.ToString() );
                }
                else if( _inserter.ExpectedMatchCount != 0 && _inserter.MatchCount < _inserter.ExpectedMatchCount )
                {
                    Monitor.Error( $"Missing matches: expecting {_inserter.ExpectedMatchCount} {_info.Location.WhatDescription}, found {_inserter.MatchCount}." );
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

        ISqlNode HandleNode( ISqlNode e )
        {
            if( _inserter.CanStop ) return e;
            if( _nodeMatcher != null && !_nodeMatcher.Match( VisitContext, e ) ) return e;
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
