﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PactNet.Models
{
    public class ProviderStates
    {
        public Action SetUp { get; private set; }
        public Action TearDown { get; private set; }

        private List<ProviderState> _providerStates;

        public ProviderStates(Action setUp = null, Action tearDown = null)
        {
            SetUp = setUp;
            TearDown = tearDown;
        }

        public void Add(ProviderState providerState)
        {
            _providerStates = _providerStates ?? new List<ProviderState>();

            if (_providerStates.Any(x => x.ProviderStateDescription == providerState.ProviderStateDescription))
            {
                throw new ArgumentException(
                    $"providerState '{providerState.ProviderStateDescription}' has already been added");
            }

            _providerStates.Add(providerState);
        }

        public ProviderState Find(string providerState)
        {
            if (providerState == null)
            {
                throw new ArgumentNullException(nameof(providerState));
            }

            if (_providerStates != null && _providerStates.Any(x => x.ProviderStateDescription == providerState))
            {
                return _providerStates.FirstOrDefault(x => x.ProviderStateDescription == providerState);
            }

            return null;
        }
    }
}