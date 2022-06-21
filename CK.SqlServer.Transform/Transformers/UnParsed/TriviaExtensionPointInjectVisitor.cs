using CK.Core;
using CK.SqlServer.Parser;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


namespace CK.SqlServer.Transform.Transformers
{
    public class TriviaExtensionPointInjectVisitor : SqlNodeLocationVisitor
    {
        readonly SqlTInjectInto _injectInto;
        readonly TriviaExtensionPointMatcher _matcher;
        readonly LocationInserter _inserter;

        internal TriviaExtensionPointInjectVisitor( IActivityMonitor monitor, SqlTInjectInto injecter )
            : base( monitor )
        {
            _injectInto = injecter;
            _matcher = new TriviaExtensionPointMatcher( monitor, injecter.Target.Value, injecter.Content.Value );
            var loc = new LocationInfo( _matcher );
            _inserter = new LocationInserter( loc );
        }

        protected override bool BeforeVisitItem() => true;

        protected override ISqlNode AfterVisitItem( ISqlNode e )
        {
            Debug.Assert( !_inserter.CanStop );
            var m = _inserter.AddCandidate( Monitor, VisitContext.Position, e );
            if( m != null )
            {
                e = m.Apply( Monitor, _matcher.TextBefore, _matcher.TextAfter, false, _matcher.TextReplace );
                Debug.Assert( !_inserter.CanStop );
                SetHasUnParsedText();
            }
            if( VisitContext.Depth == 0  && _inserter.MatchCount == 0 )
            {
                Monitor.Error( $"Unable to find extension '{_injectInto.Target.Value}'." );
            }
            return e;
        }
    }
}
