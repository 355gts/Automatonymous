// Copyright 2011-2014 Chris Patterson, Dru Sellers
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Automatonymous.Activities
{
    using System;
    using System.Threading.Tasks;
    using Events;


    /// <summary>
    /// In the event of an exception, redirects the activity to the exception handler behavior
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class RescueActivity<TInstance, TData> :
        Activity<TInstance, TData>
    {
        readonly Behavior<TInstance, Tuple<TData, Exception>> _exceptionHandler;

        public RescueActivity(Behavior<TInstance, Tuple<TData, Exception>> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        async Task Activity<TInstance, TData>.Execute(BehaviorContext<TInstance, TData> context, Behavior<TInstance, TData> next)
        {
            Exception exception = null;
            try
            {
                await next.Execute(context);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                var @event = new DataEvent<Tuple<TData, Exception>>(typeof(TData).Name + "." + exception.GetType().Name);
                BehaviorContext<TInstance, Tuple<TData, Exception>> exceptionContext = context.GetProxy(@event,
                    Tuple.Create(context.Data, exception));

                await _exceptionHandler.Execute(exceptionContext);
            }
        }

        void AcceptStateMachineInspector.Accept(StateMachineInspector inspector)
        {
            inspector.Inspect(this, x => _exceptionHandler.Accept(inspector));
        }
    }
}