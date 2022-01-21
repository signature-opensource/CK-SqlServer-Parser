using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures 
    ///     { table_alias | table_or_view | rowset_function_limited  [WITH( Table_Hint_Limited [ ...n] )] }
    ///       | @table_variable }
    /// </summary>
    public sealed class IUDTarget : SqlNonTokenAutoWidth
    {
        readonly SNode<ISqlNode, SqlWithParOptions> _content;

        public IUDTarget( ISqlNode target, SqlWithParOptions withTableHints = null, ImmutableList<SqlTrivia>? leading = null, ImmutableList<SqlTrivia>? trailing = null )
            : base( leading, trailing )
        {
            _content = new SNode<ISqlNode, SqlWithParOptions>( target, withTableHints );
            CheckContent();
        }

        IUDTarget( IUDTarget o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlWithParOptions>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new IUDTarget( this, leading, content, trailing );
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Target, nameof( Target ) );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Target => _content.V1;

        public bool HasWithTableHints => _content.V2 != null;

        public SqlWithParOptions WithTableHints => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
