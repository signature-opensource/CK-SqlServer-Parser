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
    public class SqlExprParameter : SqlItem, ISqlServerParameter
    {
        readonly SqlTokenType _inputTrivia;

        public SqlExprParameter( SqlExprTypedIdentifier declVar, SqlExprParameterDefaultValue defaultValue = null, SqlTokenIdentifier outputClause = null, SqlTokenIdentifier readonlyClause = null )
            : this( null, Build( declVar, defaultValue, outputClause, readonlyClause ), null )
        {
        }

        static SqlNode[] Build( SqlExprTypedIdentifier declVar, SqlExprParameterDefaultValue defaultValue, SqlTokenIdentifier outputClause, SqlTokenIdentifier readonlyClause )
        {
            if( declVar == null ) throw new ArgumentNullException( "declVar" );
            if( !declVar.Identifier.IsVariable ) throw new ArgumentException( "Must be a @VariableName", "variable" );
            if( outputClause != null && outputClause.TokenType != SqlTokenType.Output )
            {
                throw new ArgumentException( "Must be out or output.", "outputClause" );
            }
            if( readonlyClause != null && readonlyClause.TokenType != SqlTokenType.Readonly )
            {
                throw new ArgumentException( "Must be readonly.", "readonlyClause" );
            }
            //
            if( defaultValue == null )
            {
                if( outputClause == null )
                {
                    if( readonlyClause == null )
                    {
                        return CreateArray<SqlNode>( declVar );
                    }
                    else
                    {
                        return CreateArray<SqlNode>( declVar, readonlyClause );
                    }
                }
                else
                {
                    if( readonlyClause == null )
                    {
                        return CreateArray<SqlNode>( declVar, outputClause );
                    }
                    else
                    {
                        return CreateArray<SqlNode>( declVar, outputClause, readonlyClause );
                    }
                }
            }
            else
            {
                if( outputClause == null )
                {
                    if( readonlyClause == null )
                    {
                        return CreateArray<SqlNode>( declVar, defaultValue );
                    }
                    else
                    {
                        return CreateArray<SqlNode>( declVar, defaultValue, readonlyClause );
                    }
                }
                else
                {
                    if( readonlyClause == null )
                    {
                        return CreateArray<SqlNode>( declVar, defaultValue, outputClause );
                    }
                    else
                    {
                        return CreateArray<SqlNode>( declVar, defaultValue, outputClause, readonlyClause );
                    }
                }
            }
        }

        SqlExprParameter( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            if( OutputT != null )
            {
                _inputTrivia = GetAllTrivias( this )
                                .Where( t => t.TokenType != SqlTokenType.None )
                                .FirstOrDefault( t => t.Text.Contains( "input" ) ).TokenType;
            }
        }

        static IEnumerable<SqlTrivia> GetAllTrivias( SqlNode n )
        {
            return n.LeadingTrivias.Concat( n.TrailingTrivias ).Concat( n.ChildrenNodes.SelectMany( c => GetAllTrivias( c ) ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprParameter( leading, EnsureArray( children ), trailing );
        }

        public SqlExprTypedIdentifier Variable { get { return (SqlExprTypedIdentifier)Slots[0]; } }

        public string Name { get { return Variable.Identifier.Name; } }

        /// <summary>
        /// Gets the default value or null if no default are defined.
        /// </summary>
        public SqlExprParameterDefaultValue DefaultValue { get { return Slots.Length > 1 ? Slots[1] as SqlExprParameterDefaultValue : null; } }

        ISqlServerParameterDefaultValue ISqlServerParameter.DefaultValue { get { return DefaultValue; } }

        ISqlServerUnifiedTypeDecl ISqlServerParameter.SqlType { get { return Variable.TypeDecl.ActualType; } }

        string ISqlServerParameter.ToStringClean() => ChildrenNodes.ToStringCompact();

        /// <summary>
        /// Gets whether the parameter is a input only parameter.
        /// </summary>
        public bool IsPureInput { get { return OutputT == null; } }
        
        /// <summary>
        /// Gets whether the parameter is an input parameter or an output one with a /*input*/ tag.
        /// </summary>
        public bool IsInput { get { return OutputT == null || IsInputOutput; } }
        
        /// <summary>
        /// Gets whether the parameter is output. It can be /*input*/output (see <see cref="IsInputOutput"/>).
        /// </summary>
        public bool IsOutput { get { return OutputT != null; } }

        /// <summary>
        /// Gets whether the parameter is an output only parameter (ie. it is <see cref="IsOutput"/> but not <see cref="IsInputOutput"/>).
        /// </summary>
        public bool IsPureOutput { get { return IsOutput && !IsInputOutput; } }

        /// <summary>
        /// Gets whether the parameter is input and output (by ref).
        /// <see cref="IsOutput"/> is true: the parameter uses the '/*input*/output' syntax.
        /// </summary>
        public bool IsInputOutput => _inputTrivia != SqlTokenType.None;

        /// <summary>
        /// Gets whether the parameter is read only.
        /// </summary>
        public bool IsReadOnly { get { return ReadOnlyT != null; } }

        public SqlTokenIdentifier ReadOnlyT { get { var t = LastTokenClause; return t != null && t.TokenType == SqlTokenType.Readonly ? t : null; } }

        public SqlTokenIdentifier OutputT 
        { 
            get 
            {
                var t = LastTokenClause;
                if( t == null ) return null;
                if( t.TokenType != SqlTokenType.Output )
                {
                    t = AnteLastTokenClause;
                    Debug.Assert( t == null || t.TokenType == SqlTokenType.Output );
                }
                return t;
            } 
        }

        SqlTokenIdentifier LastTokenClause { get { return Slots.Length > 1 ? Slots[Slots.Length - 1] as SqlTokenIdentifier : null; } }

        SqlTokenIdentifier AnteLastTokenClause { get { return Slots.Length > 2 ? Slots[Slots.Length - 2] as SqlTokenIdentifier : null; } }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            if( (_inputTrivia == SqlTokenType.StarComment && w.SkipStarComment)
                || (_inputTrivia == SqlTokenType.LineComment && w.SkipLineComment) )
            {
                foreach( var t in Slots )
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
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }

}
