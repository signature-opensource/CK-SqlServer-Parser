using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Internal abstraction required by a <see cref="SqlNodeLocationVisitor"/> that is in charge of computing 
    /// the <see cref="SqlNodeLocation"/> during the visit.
    /// </summary>
    interface ISqlNodeLocationBuilder
    {
        /// <summary>
        /// Resets the builder on a <see cref="Root"/>.
        /// </summary>
        /// <param name="root">The new root.</param>
        void Reset( LocationRoot root );

        /// <summary>
        /// Gets the current location root.
        /// </summary>
        LocationRoot Root { get; }

        /// <summary>
        /// Gets the current visit depth.
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Gets the current visit position.
        /// </summary>
        int Position { get; }

        /// <summary>
        /// Called before visiting a node.
        /// </summary>
        /// <param name="n">The node to be visited.</param>
        void Enter( ISqlNode n );

        /// <summary>
        /// Called after having visited a node.
        /// </summary>
        /// <param name="n">The visited node.</param>
        /// <param name="skipped">True if the node's children have been skipped.</param>
        void Leave( ISqlNode n, bool skipped );

        /// <summary>
        /// Obtains the location of the currently visited node.
        /// When no nodes are beeing visited, <see cref="Root"/> must be returned.
        /// </summary>
        /// <param name="overridePos">This parameter is used only by <see cref="LightLocationBuilder"/>.</param>
        /// <param name="current">Must be the current node: the concrete builder does not have to manage a stack.</param>
        /// <param name="qualifiedLocation">True to force the obtention of a qualified location.</param>
        /// <returns>A raw or qualified location.</returns>
        SqlNodeLocation GetCurrent( int overridePos, ISqlNode current, bool qualifiedLocation = false );
    }

}
