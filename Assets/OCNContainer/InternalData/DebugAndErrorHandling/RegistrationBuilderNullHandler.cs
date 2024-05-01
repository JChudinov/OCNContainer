using System;
using UnityEngine;

namespace OCNContainer.InternalData.DebugAndErrorHandling
{
    //Set of empty methods not to produce any further errors if main registration method failed due its own reasons. Error handling in registration.
    public class RegistrationBuilderNullHandler : IRegistrationBuilder, IRegistrationFromInstanceBuilder
    {
        void IRegistrationFacadeBuilder.AsFacade()
        {

        }

        void IRegistrationLazyStateBuilder.AsLazy()
        {

        }
    }
}