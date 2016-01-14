using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures 
    ///     { table_alias | table_or_view | rowset_function_limited  [WITH( Table_Hint_Limited [ ...n] )] }
    ///       | @table_variable }
    /// </summary>
    public sealed class IUDTarget : SqlNode
    {
        readonly SNode<ISqlNode, SqlWithParOptions> _content;

        public IUDTarget( ISqlNode target, SqlWithParOptions withTableHints = null, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new IUDTarget( this, leading, children, trailing );
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Target, nameof( Target ) );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode Target => _content.V1;

        public bool HasWithTableHints => _content.V2 != null;

        public SqlWithParOptions WithTableHints => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
