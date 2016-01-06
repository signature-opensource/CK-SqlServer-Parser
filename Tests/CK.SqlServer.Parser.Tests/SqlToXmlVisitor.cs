using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CK.SqlServer.Parser
{
    public class SqlToXmlVisitor : SqlItemVisitor
    {
        readonly bool _combineElementType;
        XElement _current;

        public SqlToXmlVisitor( bool combineElementType = false )
        {
            _combineElementType = combineElementType;
        }

        public XElement ToXml( string name, ISqlNode item, params object[] xElements )
        {
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

        protected override ISqlNode VisitStandard( ISqlNode e )
        {
            var props = e.GetType().GetProperties()
                                .Where( p => p.Name != "UnPar"
                                             && p.Name != "WithT"
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
                props = ((IEnumerable<ISqlNode>)e).Select( x => new { Name = "Item", Value = x } );
            }
            StartNode( e )
                .Add( props.Select( o => ToXml( o.Name, o.Value ) ) );
            return e;
        }

        protected override ISqlNode VisitTokenStandard( SqlToken e )
        {
            StartNode( e ).Add( e.ToString() );
            return e;
        }

        protected override ISqlNode VisitTypeDeclStandard( ISqlUnifiedTypeDecl e )
        {
            StartNode( e )
                .Add( new XAttribute( "Text", e.ToString() ) );
            return e;
        }

        public override ISqlNode Visit( SqlSelectStatement e )
        {
            StartNode( e )
                .Add( ToXml( "Select", e.Select ) );
            return e;
        }

        public override ISqlNode Visit( SelectCombine e )
        {
            StartNode( e )
                .Add( new XAttribute( "SelectType", e.SelectOperator.ToString() ),
                      ToXml( "Left", e.Left ),
                      ToXml( "Right", e.Right ) );
            return e;
        }

        public override ISqlNode Visit( SelectFor e )
        {
            StartNode( e )
                .Add( new XAttribute( "TargetType", e.TargeType.ToString() ),
                      ToXml( "Select", e.Select ),
                      ToXml( "ForExpression", e.ForExpression ) );
            return e;
        }

        public override ISqlNode Visit( SelectOrderBy e )
        {
            StartNode( e )
                .Add( ToXml( "Select", e.Select ),
                      ToXml( "OrderByColumns", e.OrderByColumns ),
                      e.OffsetClause != null ? ToXml( "OffsetClause", e.OffsetClause ) : null );
            return e;
        }

        public override ISqlNode Visit( SqlOrderByItem e )
        {
            StartNode( e )
                .Add( e.IsDesc ? new XAttribute( "Desc", "true" ) : null,
                      ToXml( "Definition", e.Definition ) );
            return e;
        }

        public override ISqlNode Visit( SelectOrderByOffset e )
        {
            StartNode( e )
                .Add( ToXml( "OffsetExpression", e.OffsetExpression ),
                      e.HasFetchClause ? ToXml( "FetchExpression", e.FetchExpression ) : null );
            return e;
        }

        public override ISqlNode Visit( SelectSpec e )
        {
            StartNode( e )
                .Add( e.Header.TopT != null 
                            ? new XElement( "Top",
                                e.Header.PercentT != null ? new XAttribute( "Percent", "true" ) : null,
                                e.Header.WithT != null ? new XAttribute( "WithTies", "true" ) : null,
                                ToXml( "TopExpression", e.Header.TopExpression ) )
                            : null,
                      ToXml( "Columns", e.Columns ),
                      e.IntoClause != null ? ToXml( "Into", e.IntoClause ) : null,
                      e.FromClause != null ? new XElement( "From", e.FromClause.ToString() ) : null,
                      e.WhereExpression != null ? ToXml( "WhereExpression", e.WhereExpression ) : null,
                      e.GroupByClause != null ? ToXml( "GroupBy", e.GroupByClause ) : null );
            return e;
        }

        public override ISqlNode Visit( SqlOutputClause e )
        {
            StartNode( e ).Add( 
                e.HasTargetTable ? new XAttribute( "TargetTable", e.TargetTable.ToString() ) : null,
                e.TargetTableColumnNames != null ? ToXml( "TargetTableColumnNames", e.TargetTableColumnNames ) : null,
                ToXml( "Columns", e.Columns ) );
            return e;
        }

        public override ISqlNode Visit( SelectColumn e )
        {
            StartNode( e )
                .Add( e.ColumnName != null ? new XAttribute( "ColumnName", e.ColumnName.ToString() ) : null,
                      ToXml( "Definition", e.Definition ) );
            return e;
        }

        public override ISqlNode Visit( SqlStatement e )
        {
            StartNode( e ).Add( ToXml( "Content", e.Content ) );
            return e;
        }

        public override ISqlNode Visit( SqlIf e )
        {
            StartNode( e )
                .Add( ToXml( "Condition", e.Condition ),
                      ToXml( "Then", e.Then ),
                      e.HasElse ? ToXml( "Else", e.Else ) : null );
            return e;
        }

        public override ISqlNode Visit( SqlTableValues e )
        {
            StartNode( e )
                .Add( ToXml( "Values", e.Values ) );
            return e;
        }

        public override ISqlNode Visit( SqlInsertStatement e )
        {
            StartNode( e ).Add(
                e.Header.HasTop
                            ? ToXml( "Top", e.Header.TopExpression,
                                        e.Header.PercentT != null ? new XAttribute( "Percent", "true" ) : null )
                            : null,
                ToXml( "Target", e.Target.Target ),
                e.Target.HasWithTableHints ? ToXml( "WithTableHints", e.Target.WithTableHints ) : null,
                e.HasColumns ? ToXml( "Columns", e.Columns ) : null,
                e.HasOutputClause ? ToXml( "OutputClause", e.OutputClause ) : null,
                ToXml( "Values", e.Values ) );
            return e;
        }

        public override ISqlNode Visit( SqlUpdateStatement e )
        {
            StartNode( e ).Add(
                e.Header.HasTop
                            ? ToXml( "Top", e.Header.TopExpression,
                                        e.Header.PercentT != null ? new XAttribute( "Percent", "true" ) : null )
                            : null,
                ToXml( "Target", e.Target.Target ),
                e.Target.HasWithTableHints ? ToXml( "WithTableHints", e.Target.WithTableHints ) : null,
                ToXml( "Assigns", e.Assigns ),
                e.HasOutputClause ? ToXml( "OutputClause", e.OutputClause ) : null,
                e.HasFrom ? new XElement( "From", e.From.ToString() ) : null,
                e.HasWhere ? ToXml( "WhereExpression", e.WhereExpression ) : null,
                e.HasOptions ? ToXml( "Options", e.Options ) : null );
            return e;
        }

        public override ISqlNode Visit( SqlMergeStatement e )
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

        public override ISqlNode Visit( SqlParameter e )
        {
            StartNode( e )
                .Add( new XAttribute( "Name", e.Name ),
                      new XAttribute( "Direction", e.IsInputOutput ? "InputOutput" : (e.IsPureInput ? "Input" : "Output" ) ),
                      e.IsReadOnly ? new XAttribute( "IsReadOnly", "true" ) : null,
                      ToXml( "Type", e.Variable.TypeDecl ),
                      e.DefaultValue != null ? ToXml( "DefaultValue", e.DefaultValue ) : null
                );
            return e;
        }

        public override ISqlNode Visit( SqlStoredProcedure e )
        {
            StartNode( e ).Add(
                    e.IsAlter ? new XAttribute( "IsAlter", "true" ) : null,
                    new XElement( "Name", e.Name.ToString() ),
                    ToXml( "Parameters", e.Parameters ),
                    e.HasOptions ? ToXml( "Options", e.Options ) : null,
                    ToXml( "Body", e.Body )
                );
            return e;
        }

        public override ISqlNode Visit( SqlTokenIdentifier e )
        {
            StartNode( e ).Add( e.ToString() );
            if( e.IsVariable ) _current.Add( new XAttribute( "IsVariable", "true" ) );
            if( e.IsQuoted ) _current.Add( new XAttribute( "IsQuoted", "true" ) );
            if( e.IsDbType ) _current.Add( new XAttribute( "IsDbType", "true" ) );
            if( e.IsSpecial ) _current.Add( new XAttribute( "IsSpecial", "true" ) );
            if( e.IsReservedKeyword ) _current.Add( new XAttribute( "IsReservedKeyword", "true" ) );
            return e;
        }

        public override ISqlNode Visit( SqlMultiIdentifier e )
        {
            StartNode( e ).Add( e.ToString() );
            if( e.IsVariable ) _current.Add( new XAttribute( "IsVariable", "true" ) );
            if( e.IsOpenDataSouce ) _current.Add( new XAttribute( "IsOpenDataSouce", "true" ) );
            return e;
        }

        public override ISqlNode Visit( SqlOpenDataSource e )
        {
            StartNode( e ).Add( e.ToString() );
            return e;
        }

        public override ISqlNode Visit( SqlTokenLiteralString e )
        {
            base.Visit( e );
            if( e.IsUnicode ) _current.Add( new XAttribute( "IsUnicode", "true" ) );
            return e;
        }

        public override ISqlNode Visit( SqlCollate e )
        {
            StartNode( e ).Add(
                new XAttribute( "CollationName", e.CollationName ),
                ToXml( "Left", e.Left ) );
            return e;
        }

        public override ISqlNode Visit( SqlBetween e )
        {
            StartNode( e ).Add(
                e.IsNotBetween ? new XAttribute( "IsNotBetween", "true" ) : null,
                ToXml( "Left", e.Left ),
                ToXml( "Start", e.Start ),
                ToXml( "Stop", e.Stop ) );
            return e;
        }

        public override ISqlNode Visit( SqlLike e )
        {
            StartNode( e ).Add(
                e.IsNotLike ? new XAttribute( "IsNotLike", "true" ) : null,
                ToXml( "Left", e.Left ),
                ToXml( "Pattern", e.Pattern ) );
            return e;
        }

        public override ISqlNode Visit( SqlIsNull e )
        {
            StartNode( e ).Add( e.IsNotNull ? new XAttribute( "IsNotNull", "true" ) : null,
                                ToXml( "Left", e.Left ) );
            return e;
        }

        public override ISqlNode Visit( SqlNextValueFor e )
        {
            StartNode( e ).Add( e.SequenceName.ToString() );
            return e;
        }

        public override ISqlNode Visit( SqlKoCall e )
        {
            StartNode( e ).Add( 
                new XAttribute( "FunName", e.FunName.ToString() ), 
                ToXml( "Parameters", e.Parameters ),
                e.OverClause != null ? ToXml( "OverClause", e.OverClause ) : null );
            return e;
        }

        public override ISqlNode Visit( SelectGroupBy e )
        {
            StartNode( e ).Add( ToXml( "GroupExpression", e.GroupExpression ),
                                e.HasHaving ? ToXml( "Having", e.HavingExpression ) : null );
            return e;
        }

        public override ISqlNode Visit( SqlExecuteStatement e )
        {
            StartNode( e ).Add(
                new XElement( "Name", e.Name.ToString() ),
                ToXml( "Parameters", e.Parameters ),
                e.Options != null ? ToXml( "Options", e.Options ) : null );
            return e;
        }

        public override ISqlNode Visit( SqlExecuteStringStatement e )
        {
            StartNode( e ).Add(
                ToXml( "Arguments", e.Arguments ),
                e.Options != null ? ToXml( "Options", e.Options ) : null );
            return e;
        }

        public override ISqlNode Visit( SqlCTEStatement e )
        {
            StartNode( e ).Add(
                ToXml( "Names", e.Names ),
                ToXml( "OuterStatement", e.OuterStatement ) );
            return e;
        }

        public override ISqlNode Visit( SqlCTEName e )
        {
            StartNode( e ).Add(
                new XAttribute( "Name", e.Name.ToString() ),
                e.ColumnNames != null 
                    ? new XAttribute( "ColumnNames", string.Join( ", ", e.ColumnNames.Select( c => c.ToString() ) ) )
                    : null,
                ToXml( "Select", e.Select ) );
            return e;
        }

    }
}
