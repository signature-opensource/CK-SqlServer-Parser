using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTypedIdentifier, SqlTokenTerminal, SqlBasicValue, SqlTokenIdentifier, SqlTokenIdentifier>;

    public sealed class SqlParameter : SqlNonTokenAutoWidth, ISqlServerParameter
    {
        static readonly Regex _rNotNull = new Regex( @"\bnot\s+null\b", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        readonly CNode _content;
        readonly SqlTokenType _inputMarkerTrivia;
        readonly SqlTokenType _notNullMarkerTrivia;

        public SqlParameter( SqlTypedIdentifier declVar, SqlTokenTerminal assignT = null, SqlBasicValue defaultValue = null, SqlTokenIdentifier outputT = null, SqlTokenIdentifier readonlyT = null )
            : base( null, null )
        {
            _content = new CNode( declVar, assignT, defaultValue, outputT, readonlyT );
            (_inputMarkerTrivia,_notNullMarkerTrivia) = CheckContent();
        }

        (SqlTokenType,SqlTokenType) CheckContent()
        {
            Helper.CheckIsVariable( Variable, nameof( Variable ) );
            Helper.CheckNullableToken( AssignT, nameof( AssignT ), SqlTokenType.Assign );
            Helper.CheckNullableToken( OutputT, nameof( OutputT ), SqlTokenType.Output, SqlTokenType.Out );
            Helper.CheckNullableToken( ReadOnlyT, nameof( ReadOnlyT ), SqlTokenType.Readonly );

            SqlTokenType inputMarker = SqlTokenType.None;
            SqlTokenType notNullMarker = SqlTokenType.None;
            foreach( SqlTrivia t in GetAllTrivias( this ) )
            {
                if( t.TokenType != SqlTokenType.None )
                {
                    if( OutputT != null && t.Text.Contains( "input" ) )
                    {
                        inputMarker = t.TokenType;
                        if( notNullMarker != SqlTokenType.None ) break;
                    }
                    if( _rNotNull.Match( t.Text ).Success )
                    {
                        notNullMarker = t.TokenType;
                        if( inputMarker != SqlTokenType.None || OutputT == null ) break;
                    }
                }
            }
            return (inputMarker, notNullMarker);
        }

        SqlParameter( SqlParameter o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null )
            {
                _content = o._content;
                _inputMarkerTrivia = o._inputMarkerTrivia;
                _notNullMarkerTrivia = o._notNullMarkerTrivia;
            }
            else
            {
                _content = new CNode( items );
                (_inputMarkerTrivia, _notNullMarkerTrivia) = CheckContent();
            }
        }

        static IEnumerable<SqlTrivia> GetAllTrivias( ISqlNode n )
        {
            return n.LeadingTrivias.Concat( n.TrailingTrivias ).Concat( n.ChildrenNodes.SelectMany( c => GetAllTrivias( c ) ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlParameter( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTypedIdentifier Variable => _content.V1;

        public string Name => Variable.Identifier.Name;

        public SqlTokenTerminal AssignT => _content.V2;

        /// <summary>
        /// Gets the default value or null if no default is defined.
        /// </summary>
        public SqlBasicValue DefaultValue => _content.V3;

        ISqlServerParameterDefaultValue ISqlServerParameter.DefaultValue => _content.V3;

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
        public bool IsInputOutput => _inputMarkerTrivia != SqlTokenType.None;

        /// <summary>
        /// Gets whether the parameter is marked with the '/*not null*/ comment.
        /// </summary>
        public bool IsNotNull => _notNullMarkerTrivia != SqlTokenType.None;

        public SqlTokenIdentifier OutputT => _content.V4;

        /// <summary>
        /// Gets whether the parameter is read only.
        /// </summary>
        public bool IsReadOnly => ReadOnlyT != null;

        public SqlTokenIdentifier ReadOnlyT => _content.V5;

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            bool mustWriteInputTrivia = (_inputMarkerTrivia == SqlTokenType.StarComment && w.SkipStarComment)
                                        || (_inputMarkerTrivia == SqlTokenType.LineComment && w.SkipLineComment);
            bool mustWriteNotNullTrivia = (_notNullMarkerTrivia == SqlTokenType.StarComment && w.SkipStarComment)
                                        || (_notNullMarkerTrivia == SqlTokenType.LineComment && w.SkipLineComment);

            if( mustWriteInputTrivia || mustWriteNotNullTrivia )
            {
                foreach( var t in _content )
                {
                    if( mustWriteInputTrivia && t.IsToken( SqlTokenType.Output ) )
                    {
                        if( mustWriteNotNullTrivia )
                        {
                            w.Write( SqlTokenType.StarComment, "/*not null, input*/", null, false );
                        }
                        else
                        {
                            w.Write( SqlTokenType.StarComment, "/*input*/", null, false );
                        }
                    }
                    t.Write( w );
                    if( mustWriteNotNullTrivia && t == Variable && !mustWriteInputTrivia )
                    {
                        w.Write( SqlTokenType.StarComment, "/*not null*/", true, null );
                    }
                }
            }
            else base.WriteWithoutTrivias( w );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
