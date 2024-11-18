using Kantab.Interfaces;
using Kantab.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantab.Classes.PenStateProviders {
    public class KRelayPenStateProvider : IPenStateProvider {
        public bool Extensible => true;

        /// <summary>
        /// By default the relayed pen states are extended because they can get tilt from browser or OTD driver
        /// </summary>
        public bool Extended {get; set;} = true;

        public PenState CurrentPenState {get; private set;}

        public void SetExtended(bool extend) {
            Extended = extend;
        }

        private void UpdatePosition(object? sender, PenState state) {
            CurrentPenState = state;
        }

        public KRelayPenStateProvider(KantabServer server) {
            server.PositionRelayed += UpdatePosition;

        }

        ~KRelayPenStateProvider() {
            // Should I desubscribe the event from the server here...?
        }
    }
}
