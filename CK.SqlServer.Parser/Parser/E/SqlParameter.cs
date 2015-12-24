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
    public sealed class SqlParameter : SqlNode, ISqlServerParameter
    {
        readonly SNode<SqlTypedIdentifier, SqlParameterDefaultValue, SqlTokenIdentifier, SqlTokenIdentifier> _content;
        readonly SqlTokenType _inputTrivia;

        public SqlParameter( SqlTypedIdentifier declVar, SqlParameterDefaultValue defaultValue = null, SqlTokenIdentifier outputT = null, SqlTokenIdentifier readonlyT = null )
            : base( null, null )
        {
            _content = new SNode<SqlTypedIdentifier, SqlParameterDefaultValue, SqlTokenIdentifier, SqlTokenIdentifier>( declVar, defaultValue, outputT, readonlyT );
            _inputTrivia = CheckContent();
        }

        SqlTokenType CheckContent()
        {
            SNode.CheckIsVariable( Variable, nameof( Variable ) );
            SNode.CheckNullableToken( OutputT, nameof( OutputT ), SqlTokenType.Output );
            SNode.CheckNullableToken( ReadOnlyT, nameof( ReadOnlyT ), SqlTokenType.Readonly );
            return OutputT != null
                        ? GetAllTrivias( this )
                                    .Where( t => t.TokenType != SqlTokenType.None )
                                    .FirstOrDefault( t => t.Text.Contains( "input" ) ).TokenType
                        : SqlTokenType.None;
        }

        SqlParameter( SqlParameter o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null )
            {
                _content = o._content;
                _inputTrivia = o._inputTrivia;
            }
            else
            {
                _content = new SNode<SqlTypedIdentifier, SqlParameterDefaultValue, SqlTokenIdentifier, SqlTokenIdentifier>( items );
                _inputTrivia = CheckContent();
            }
        }

        static IEnumerable<SqlTrivia> GetAllTrivias( ISqlNode n )
        {
            return n.LeadingTrivias.Concat( n.TrailingTrivias ).Concat( n.ChildrenNodes.SelectMany( c => GetAllTrivias( c ) ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlParameter( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTypedIdentifier Variable => _content.V1;

        public string Name { get { return Variable.Identifier.Name; } }

        /// <summary>
        /// Gets the default value or null if no default are defined.
        /// </summary>
        public SqlParameterDefaultValue DefaultValue => _content.V2;

        ISqlServerParameterDefaultValue ISqlServerParameter.DefaultValue => DefaultValue;

        ISqlServerUnifiedTypeDecl ISqlServerParameter.SqlType => Variable.TypeDecl;

        string ISqlServerParameter.ToStringClean() => ChildrenNodes.ToStringCompact();

        /// <summary>
        /// Gets whether the parameter is a input only parameter.
        /// </summary>
        public bool IsPureInput => OutputT == null;
        
        /// <summary>
        /// Gets whether the parameter is an input parameter or an output one with a /*input*/ tag.
        /// </summary>
        public bool IsInput => OutputT == null || IsInputOutput;
        
        /// <summary>
        /// Gets whether the parameter is output. It can be /*input*/output (see <see cref="IsInputOutput"/>).
        /// </summary>
        public bool IsOutput => OutputT != null;

        /// <summary>
        /// Gets whether the parameter is an output only parameter (ie. it is <see cref="IsOutput"/> but not <see cref="IsInputOutput"/>).
        /// </summary>
        public bool IsPureOutput => IsOutput && !IsInputOutput;

        /// <summary>
        /// Gets whether the parameter is input and output (by ref).
        /// <see cref="IsOutput"/> is true: the parameter uses the '/*input*/output' syntax.
        /// </summary>
        public bool IsInputOutput => _inputTrivia != SqlTokenType.None;

        public SqlTokenIdentifier OutputT => _content.V3;

        /// <summary>
        /// Gets whether the parameter is read only.
        /// </summary>
        public bool IsReadOnly => ReadOnlyT != null;

        public SqlTokenIdentifier ReadOnlyT => _content.V4;

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            if( (_inputTrivia == SqlTokenType.StarComment && w.SkipStarComment)
                || (_inputTrivia == SqlTokenType.LineComment && w.SkipLineComment) )
            {
                foreach( var t in _content )
                {
                    if( t.IsToken( SqlTokenType.Output ) )
                    {
                        w.Write( "/*input*/", null, false );
                    }
                    t.Write( w );
                }
            }
            else base.WriteWithoutTrivias( w );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
