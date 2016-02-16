//using CK.SqlServer.Parser;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CK.SqlServer.Transform
//{
//    public class BeforeTokenLocator
//    {
//        bool _success;

//        readonly SqlTokenBaseLiteral _literal;
//        readonly SqlTokenIdentifier _identifier;
//        readonly SqlTokenTerminal _terminal;

//        public BeforeTokenLocator( SqlTokenBaseLiteral t )
//        {
//            _literal = t;
//        }

//        public BeforeTokenLocator( SqlTokenIdentifier t )
//        {
//            _identifier = t;
//        }

//        public BeforeTokenLocator( SqlTokenTerminal t )
//        {
//            _terminal = t;
//        }

//        void Reset()
//        {
//            _success = true;
//        }

//        bool Accept( ISqlNode e )
//        {
//            Debug.Assert( _success );
//            if( _terminal != null && e.IsToken( _terminal.TokenType ) )
//            {
//                _success
//            }
//        }

//    }
//}
