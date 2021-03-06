﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
#if NET451
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#else
using System.Threading;
#endif

namespace Microsoft.Extensions.Logging
{
    public class HttpLoggerScope
    {
        internal HttpLoggerScope(object state)
        {
            State = state;
        }

        public Object State { get; private set; }
        public HttpLoggerScope Parent { get; private set; }

#if NET451
        // LogicalCallContext will flow through cross app domain calls.
        // Thus we make the FieldKey domain specific and wrap the EventFlowLoggerScope in ObjectHandle to avoid exceptions when cross app domain call happens.
        private static readonly string FieldKey = $"{typeof(EventFlowLoggerScope).FullName}_{AppDomain.CurrentDomain.Id}";
        public static EventFlowLoggerScope Current
        {
            get
            {
                ObjectHandle handle = (ObjectHandle)CallContext.LogicalGetData(FieldKey);

                // Unwrap the scope if it was set in the same AppDomain (as FieldKey is AppDomain-specific).
                if (handle != null)
                {
                    return (EventFlowLoggerScope)handle.Unwrap();
                }

                return null;
            }
            private set
            {
                CallContext.LogicalSetData(FieldKey, new ObjectHandle(value));
            }
        }
#else
        private static AsyncLocal<HttpLoggerScope> _value = new AsyncLocal<HttpLoggerScope>();
        public static HttpLoggerScope Current
        {
            set
            {
                _value.Value = value;
            }
            get
            {
                return _value.Value;
            }
        }
#endif

        public static IDisposable Push(object state)
        {
            var temp = Current;
            Current = new HttpLoggerScope(state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        public override string ToString()
        {
            return State?.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}