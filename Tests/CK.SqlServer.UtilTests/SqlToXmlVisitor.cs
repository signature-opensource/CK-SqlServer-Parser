using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CK.SqlServer.UtilTests
{
    public class SqlToXmlVisitor : SqlNodeVisitor
    {
        readonly bool _combineElementType;
        readonly HashSet<string> _shortForms;
        XElement _current;

        public SqlToXmlVisitor( bool combineElementType = false, params string[] shortForms )
        {
            _combineElementType = combineElementType;
            _shortForms = new HashSet<string>( shortForms );
        }

        public XElement ToXml( string name, ISqlNode item, params object[] xElements )
        {
            if( _shortForms.Contains( name ) )
            {
                var sE = new XElement( name );
                sE.Add( item.ToString() );
                return sE;
            }
            var prev = _current;
            var e = _current = new XElement( name );
            e.Add( xElements );
            VisitItem( item );
            _current = prev;
            return e;
        }

        XElement StartNode( ISqlNode e )
        {
            string typeName;
            if( e is SqlEnclosableCommaList ) typeName = "CommaList";
            else
            {
                typeName = e.GetType().Name;
                if( typeName.StartsWith( "SqlToken" ) ) typeName = typeName.Substring( 8 );
                else if( typeName.StartsWith( "Sql" ) ) typeName = typeName.Substring( 3 );
            }

            if( !_combineElementType )
            {
                var c = new XElement( typeName );
                _current.Add( c );
                _current = c;
            }
            else _current.Add( new XAttribute( "EType", typeName ) );
            OnStartNode( e );
            return _current;
        }

        void OnStartNode( ISqlNode e )
        {
            if( e is ISqlEnclosable && !(e is ISqlStructurallyEnclosed) && ((ISqlEnclosable)e).IsEnclosed )
            {
                _current.Add( new XAttribute( "IsEnclosed", "true" ) );
            }
            if( e is ISqlStatement )
            {
                ISqlNamedStatement n = e as ISqlNamedStatement;
                if( n != null )
                {
                    string sn = n.GetStatementName();
                    if( sn != _current.Name && sn+"Statement" != _current.Name )
                    {
                        _current.Add( new XAttribute( "StatementName", sn ) );
                    }
                }
                if( ((ISqlStatement)e).StatementTerminator != null )
                {
                    _current.Add( new XAttribute( "HasTerminator", "true" ) );
                }
            }
        }

        void StartNode( ISqlNode e, Action<XElement> config )
        {
            XElement x = StartNode( e );
            if( _shortForms.Contains( x.Name.ToString() ) ) x.Add( e.ToString() );
            else config( x );
        }

        protected override ISqlNode VisitStandard( ISqlNode e )
        {
            StartNode( e, x =>
            {
                var props = e.GetType().GetProperties()
                    .Where( p => p.Name != "UnPar"
                                 && p.Name != "WithT"
                                 && p.Name != "OverT"
                                 && p.Name != "FromT"
                                 && p.Name != "OptionT"
                                 && p.Name != "SetT"
                                 && p.Name != "StatementTerminator"
                                 && (p.Name != "Opener" || p.PropertyType != typeof( SqlTokenOpenPar ))
                                 && (p.Name != "Closer" || p.PropertyType != typeof( SqlTokenClosePar ))
                          )
                    .Where( p => typeof( ISqlNode ).IsAssignableFrom( p.PropertyType )
                                    && p.GetIndexParameters().Length == 0 )
                    .Select( p => new { Name = p.Name, Value = (ISqlNode)p.GetValue( e ) } )
                    .Where( o => o.Value != null );
                if( !props.Any() && e is IEnumerable<ISqlNode> )
                {
                    props = ((IEnumerable<ISqlNode>)e).Select( p => new { Name = "Item", Value = p } );
                }
                x.Add( props.Select( o => ToXml( o.Name, o.Value ) ) );
            } );
            return e;
        }

        protected override ISqlNode VisitTokenStandard( SqlToken e )
        {
            StartNode( e ).Add( e.ToString() );
            return e;
        }

        protected override ISqlNode VisitTypeDeclStandard( ISqlUnifiedTypeDecl e )
        {
            StartNode( e, x => 
                x.Add( new XAttribute( "DBType", e.DbType ), new XAttribute( "Text", e.ToString() ) ) 
                );
            return e;
        }

        protected override ISqlNode Visit( SqlSelectStatement e )
        {
            StartNode( e, x => 
                x.Add( ToXml( "Select", e.Select ) ) 
                );
            return e;
        }

        protected override ISqlNode Visit( SelectCombine e )
        {
            StartNode( e )
                .Add( new XAttribute( "SelectType", e.SelectOperator.ToString() ),
                      ToXml( "Left", e.Left ),
                      ToXml( "Right", e.Right ) );
            return e;
        }

        protected override ISqlNode Visit( SelectDecorator e )
        {
            StartNode( e )
                .Add( ToXml( "Select", e.Select ),
                      e.HasOrderBy ? ToXml( nameof(e.OrderBy), e.OrderBy ) : null,
                      e.HasFor ? ToXml( nameof( e.For ), e.For ) : null,
                      e.HasOption ? ToXml( nameof(e.Option), e.Option ) : null
                      );
            return e;
        }

        protected override ISqlNode Visit( SelectOrderBy e )
        {
            StartNode( e, x => x.Add(
                    ToXml( nameof( e.OrderByColumns ), e.OrderByColumns ),
                    e.HasOffset ? ToXml( nameof( e.OffsetExpression ), e.OffsetExpression ) : null,
                    e.HasFetch ? ToXml( nameof( e.FetchExpression ), e.FetchExpression ) : null )
                    );
            return e;
        }

        protected override ISqlNode Visit( SelectFor e )
        {
            StartNode( e, x => x.Add(
                    new XAttribute( nameof( e.TargetType ), e.TargetType.ToString() ),
                    ToXml( nameof( e.Format ), e.Format ) )
                    );
            return e;
        }


        protected override ISqlNode Visit( SqlOrderByItem e )
        {
            StartNode( e )
                .Add( e.IsDesc ? new XAttribute( "Desc", "true" ) : null,
                      ToXml( "Definition", e.Definition ) );
            return e;
        }

        protected override ISqlNode Visit( SelectSpec e )
        {
            StartNode( e )
                .Add( SelectHeaderTopToXml( e.Header ),
                      ToXml( "Columns", e.Columns ),
                      e.IntoClause != null ? ToXml( "Into", e.IntoClause ) : null,
                      e.FromClause != null ? ToXml( "From", e.FromClause ) : null,
                      e.WhereExpression != null ? ToXml( "WhereExpression", e.WhereExpression ) : null,
                      e.GroupByClause != null ? ToXml( "GroupBy", e.GroupByClause ) : null );
            return e;
        }

        XElement SelectHeaderTopToXml( SelectHeader h )
        {
            if( h.TopT == null ) return null;
            if( _shortForms.Contains("Top") )
            {
                return new XElement( "Top", new ISqlNode[] { h.TopExpression, h.PercentT, h.WithT }.ToStringCompact() );
            }
            else
            {
                return new XElement( "Top",
                                        ToXml( "TopExpression", h.TopExpression ),
                                        h.PercentT != null ? new XAttribute( "Percent", "true" ) : null,
                                        h.WithT != null ? new XAttribute( "WithTies", "true" ) : null );
            }
        }

        protected override ISqlNode Visit( SqlOutputClause e )
        {
            StartNode( e ).Add( 
                e.HasTargetTable ? new XAttribute( "TargetTable", e.TargetTable.ToString() ) : null,
                e.TargetTableColumnNames != null ? ToXml( "TargetTableColumnNames", e.TargetTableColumnNames ) : null,
                ToXml( "Columns", e.Columns ) );
            return e;
        }

        protected override ISqlNode Visit( SelectColumn e )
        {
            StartNode( e )
                .Add( e.ColumnName != null ? new XAttribute( "ColumnName", e.ColumnName.ToString() ) : null,
                      ToXml( "Definition", e.Definition ) );
            return e;
        }

        protected override ISqlNode Visit( SqlStatement e )
        {
            StartNode( e ).Add( ToXml( "Content", e.Content ) );
            return e;
        }

        protected override ISqlNode Visit( SqlIf e )
        {
            StartNode( e, x =>
                x.Add( ToXml( "Condition", e.Condition ),
                      ToXml( "Then", e.Then ),
                      e.HasElse ? ToXml( "Else", e.Else ) : null )
                      );
            return e;
        }

        protected override ISqlNode Visit( SqlTableValues e )
        {
            StartNode( e, x =>
                x.Add( ToXml( "Values", e.Values ) )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlInsertStatement e )
        {
            StartNode( e, x => x.Add(
                e.Header.HasTop
                            ? ToXml( "Top", e.Header.TopExpression,
                                        e.Header.PercentT != null ? new XAttribute( "Percent", "true" ) : null )
                            : null,
                ToXml( "Target", e.Target.Target ),
                e.Target.HasWithTableHints ? ToXml( "WithTableHints", e.Target.WithTableHints ) : null,
                e.HasColumns ? ToXml( "Columns", e.Columns ) : null,
                e.HasOutputClause ? ToXml( "OutputClause", e.OutputClause ) : null,
                ToXml( "Values", e.Values ) ) 
                );
            return e;
        }

        protected override ISqlNode Visit( SqlUpdateStatement e )
        {
            StartNode( e, x => x.Add(
                e.Header.HasTop
                            ? ToXml( "Top", e.Header.TopExpression,
                                        e.Header.PercentT != null ? new XAttribute( "Percent", "true" ) : null )
                            : null,
                ToXml( "Target", e.Target.Target ),
                e.Target.HasWithTableHints ? ToXml( "WithTableHints", e.Target.WithTableHints ) : null,
                ToXml( "Assigns", e.Assigns ),
                e.HasOutputClause ? ToXml( "OutputClause", e.OutputClause ) : null,
                e.HasFrom ? ToXml( "From", e.From ) : null,
                e.HasWhere ? ToXml( "WhereExpression", e.WhereExpression ) : null,
                e.HasOptions ? ToXml( "Options", e.Options ) : null )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlMergeStatement e )
        {
            StartNode( e ).Add(
                e.Header.HasTop ? ToXml( "Top", e.Header.TopExpression ) : null,
                e.HasIntoTarget ? ToXml( "Into", e.TargetTable, 
                                        e.HasTargetAliasName ? new XAttribute( "TargetAliasName", e.TargetAliasName.ToString() ) : null ) 
                                        : null,
                e.HasWithMergeHints ? ToXml( "WithMergeHints", e.WithMergeHints ) : null,
                ToXml( "UnmodeledRemaider", e.UnmodeledRemaider ) );
            return e;
        }

        protected override ISqlNode Visit( SqlOpenJSON e )
        {
            StartNode( e, x => x.Add(
                ToXml( nameof( e.Parameters ), e.Parameters ),
                e.HasSchema ? ToXml( nameof( e.Schema ), e.Schema ) : null )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlOpenXml e )
        {
            StartNode( e, x => x.Add(
                ToXml( nameof( e.Parameters ), e.Parameters ),
                e.HasSchemaDefinition 
                    ? ToXml( nameof( e.SchemaDefinition ), e.SchemaDefinition ) 
                    : (e.HasSchemaTable
                        ? ToXml( nameof( e.SchemaTable ), e.SchemaTable ) 
                        : null))
                );
            return e;
        }

        protected override ISqlNode Visit( SqlParameter e )
        {
            StartNode( e, x =>
                x.Add( new XAttribute( "Name", e.Name ),
                      new XAttribute( "Direction", e.IsInputOutput ? "InputOutput" : (e.IsPureInput ? "Input" : "Output" ) ),
                      e.IsReadOnly ? new XAttribute( "IsReadOnly", "true" ) : null,
                      ToXml( "Type", e.Variable.TypeDecl ),
                      e.DefaultValue != null ? ToXml( "DefaultValue", e.DefaultValue ) : null )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlStoredProcedure e )
        {
            StartNode( e ).Add(
                    e.CreateOrAlter.StatementPrefix != CreateOrAlterStatementPrefix.Create
                        ? new XAttribute( "IsAlter", "true" )
                        : null,
                    new XElement( "Name", e.FullName.ToString() ),
                    ToXml( "Parameters", e.Parameters ),
                    e.HasOptions ? ToXml( "Options", e.Options ) : null,
                    ToXml( "Body", e.Body )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlTokenIdentifier e )
        {
            StartNode( e, x =>
            {
                x.Add( e.ToString() );
                if( e.TokenType.IsVariable() ) x.Add( new XAttribute( "IsVariable", "true" ) );
                if( e.TokenType.IsQuotedIdentifier() ) x.Add( new XAttribute( "IsQuoted", "true" ) );
                if( e.TokenType.IsDbType() ) x.Add( new XAttribute( "IsDbType", "true" ) );
                if( e.TokenType.IsIdentifierSpecial() ) x.Add( new XAttribute( "IsSpecial", "true" ) );
                if( e.TokenType.IsReservedKeyword() ) x.Add( new XAttribute( "IsReservedKeyword", "true" ) );
            } );
            return e;
        }

        protected override ISqlNode Visit( SqlMultiIdentifier e )
        {
            StartNode( e, x =>
            {
                x.Add( e.ToString() );
                if( e.IsVariable ) x.Add( new XAttribute( "IsVariable", "true" ) );
                if( e.IsOpenDataSource ) x.Add( new XAttribute( "IsOpenDataSouce", "true" ) );
            } );
            return e;
        }

        protected override ISqlNode Visit( SqlOpenDataSource e )
        {
            StartNode( e ).Add( e.ToString() );
            return e;
        }

        protected override ISqlNode Visit( SqlTokenLiteralString e )
        {
            base.Visit( e );
            if( e.IsUnicode ) _current.Add( new XAttribute( "IsUnicode", "true" ) );
            return e;
        }

        protected override ISqlNode Visit( SqlCollate e )
        {
            StartNode( e, x => x.Add(
                new XAttribute( "CollationName", e.CollationName ),
                ToXml( "Left", e.Left ) )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlBetween e )
        {
            StartNode( e, x => x.Add(
                e.IsNotBetween ? new XAttribute( "IsNotBetween", "true" ) : null,
                ToXml( "Left", e.Left ),
                ToXml( "Start", e.Start ),
                ToXml( "Stop", e.Stop ) )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlLike e )
        {
            StartNode( e, x=> x.Add(
                e.IsNotLike ? new XAttribute( "IsNotLike", "true" ) : null,
                ToXml( "Left", e.Left ),
                ToXml( "Pattern", e.Pattern ) )
                );
            return e;
        }

        protected override ISqlNode Visit( SqlIsNull e )
        {
            StartNode( e, x => x.Add( e.IsNotNull ? new XAttribute( "IsNotNull", "true" ) : null,
                                ToXml( "Left", e.Left ) ) );
            return e;
        }

        protected override ISqlNode Visit( SqlNextValueFor e )
        {
            StartNode( e ).Add( e.SequenceName.ToString() );
            return e;
        }

        protected override ISqlNode Visit( SqlKoCall e )
        {
            StartNode( e, x => x.Add( 
                new XAttribute( "FunName", e.FunName.ToString() ), 
                ToXml( "Parameters", e.Parameters ),
                e.WithinGroup != null ? ToXml( "WithinGroup", e.WithinGroup ) : null,
                e.OverClause != null ? ToXml( "OverClause", e.OverClause ) : null ) );
            return e;
        }

        protected override ISqlNode Visit( SelectGroupBy e )
        {
            StartNode( e ).Add( ToXml( "GroupExpression", e.GroupExpression ),
                                e.HasHaving ? ToXml( "Having", e.HavingExpression ) : null );
            return e;
        }

        protected override ISqlNode Visit( SqlExecuteStatement e )
        {
            StartNode( e, x => x.Add(
                new XElement( nameof( e.Name ), e.Name.ToString() ),
                ToXml( nameof( e.Parameters ), e.Parameters ),
                e.Options != null ? ToXml( nameof( e.Options ), e.Options ) : null ) );
            return e;
        }

        protected override ISqlNode Visit( SqlExecuteStringStatement e )
        {
            StartNode( e, x => x.Add(
                ToXml( nameof( e.Arguments ), e.Arguments ),
                e.Options != null ? ToXml( nameof( e.Options ), e.Options ) : null ) );
            return e;
        }

        protected override ISqlNode Visit( SqlCTEStatement e )
        {
            StartNode( e ).Add(
                ToXml( nameof( e.Names ), e.Names ),
                ToXml( nameof( e.OuterStatement ), e.OuterStatement ) );
            return e;
        }

        protected override ISqlNode Visit( SqlCTEName e )
        {
            StartNode( e, x => x.Add(
                new XAttribute( nameof(e.Name ), e.Name.ToString() ),
                e.ColumnNames != null 
                    ? new XAttribute( nameof(e.ColumnNames), string.Join( ", ", e.ColumnNames.Select( c => c.ToString() ) ) )
                    : null,
                ToXml( nameof(e.Select), e.Select ) ) );
            return e;
        }

        protected override ISqlNode Visit( SqlCase e )
        {
            StartNode( e, x => x.Add( ToXml( nameof( e.WhenList ), e.WhenList ) ) );
            return e;
        }

        protected override ISqlNode Visit( SqlCaseWhenSelector e )
        {
            StartNode( e, x => x.Add( 
                ToXml( nameof( e.Expression ), e.Expression ),
                ToXml( nameof( e.Value ), e.Value ) )
                );
            return e;
        }
    }
}
