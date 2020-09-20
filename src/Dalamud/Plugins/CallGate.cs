using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Dalamud.Plugins
{
    public class CallGate : DynamicObject
    {
        Dictionary< string, object > _funcs = new Dictionary< string, object >();

        public override bool TryGetMember( GetMemberBinder binder, out object? result )
        {
            var name = binder.Name;
            return _funcs.TryGetValue( name, out result );
        }

        public override bool TrySetMember( SetMemberBinder binder, object? value )
        {
            return false;
        }

        public void RegisterAction( string name, Action expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1 >( string name, Action< T1 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2 >( string name, Action< T1, T2 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3 >( string name, Action< T1, T2, T3 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4 >( string name, Action< T1, T2, T3, T4 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5 >( string name, Action< T1, T2, T3, T4, T5 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6 >( string name, Action< T1, T2, T3, T4, T5, T6 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7 >( string name, Action< T1, T2, T3, T4, T5, T6, T7 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8 >( string name, Action< T1, T2, T3, T4, T5, T6, T7, T8 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15 > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterAction< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16 >( string name,
            Action< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16 > expr ) =>
            _funcs[ name ] = expr;


        public void RegisterFunc< T1, TRet >( string name, Func< T1, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, TRet >( string name, Func< T1, T2, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, TRet >( string name, Func< T1, T2, T3, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, TRet >( string name, Func< T1, T2, T3, T4, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, TRet >( string name, Func< T1, T2, T3, T4, T5, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, TRet >( string name, Func< T1, T2, T3, T4, T5, T6, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, TRet >( string name, Func< T1, T2, T3, T4, T5, T6, T7, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet > expr ) =>
            _funcs[ name ] = expr;

        public void RegisterFunc< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet >( string name,
            Func< T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet > expr ) =>
            _funcs[ name ] = expr;
    }
}
