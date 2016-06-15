using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    class DepthFirstNodeMatcherHelper
    {
        /// <summary>
        /// This is used to promote a SelectSpec match to its containing SelectDecorator (if it exists).
        /// When descending, we capture the top level decorator at its position so that
        /// we know that matching at this exact position is useless: only a SelectSpec (or
        /// another decorator) may match and if it is the case, we do not want the subordinated select to match.
        /// </summary>
        readonly Stack<KeyValuePair<SelectDecorator, int>> _selectDecorator;
        readonly Func<ISqlNode, bool> _matcher;
        int _previousMatchPos;

        public DepthFirstNodeMatcherHelper( bool isPartMatch, Func<ISqlNode, bool> matcher )
        {
            _matcher = matcher;
            if( isPartMatch ) _selectDecorator = new Stack<KeyValuePair<SelectDecorator, int>>();
        }

        public void Reset()
        {
            _previousMatchPos = 0;
        }

        /// <summary>
        /// Gets whether this is a structural part match.
        /// </summary>
        public bool IsPartMatch => _selectDecorator != null;

        /// <summary>
        /// Called from <see cref="SqlNodeLocationVisitor.BeforeVisitItem"/>.
        /// </summary>
        /// <param name="c">The visit context.</param>
        public void OnBeforeVisitItem( IVisitContext c )
        {
            if( _selectDecorator != null )
            {
                var d = c.VisitedNode as SelectDecorator;
                if( d != null
                    && (_selectDecorator.Count == 0 || _selectDecorator.Peek().Value != c.Position) )
                {
                    _selectDecorator.Push( new KeyValuePair<SelectDecorator, int>( d, c.Position ) );
                }
            }
        }

        /// <summary>
        /// This is called from <see cref="SqlNodeLocationVisitor.AfterVisitItem(ISqlNode)"/>.
        /// </summary>
        /// <param name="c">The visit context.</param>
        /// <param name="e">The result of the visit on the <see cref="IVisitContext.VisitedNode"/>.</param>
        /// <returns>True if this node matches, false otherwise.</returns>
        public bool Match( IVisitContext c, ISqlNode e )
        {
            if( IsCovered( c )
                || c.Position < _previousMatchPos
                || !c.RangeFilterStatus.IsIncludedInFilteredRange()
                || !_matcher( e ) )
            {
                return false;
            }
            _previousMatchPos = c.Position + c.VisitedNode.Width;
            return true;
        }

        bool IsCovered( IVisitContext c )
        {
            if( _selectDecorator == null || _selectDecorator.Count == 0 ) return false;
            var h = _selectDecorator.Peek();
            if( h.Value == c.Position )
            {
                if( h.Key == c.VisitedNode )
                {
                    _selectDecorator.Pop();
                    return false;
                }
                return true;
            }
            return false;
        }
    }

}
