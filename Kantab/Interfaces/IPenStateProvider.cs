using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Kantab.Structs;

namespace Kantab.Interfaces
{
    public interface IPenStateProvider
    {
        /// <summary>
        /// Whether this Pen State Provider can change its output to be Extended
        /// </summary>
        bool Extensible { get; }

        /// <summary>
        /// Whether this Pen State Provider prefers Extended state
        /// </summary>
        bool Extended { get; }

        /// <summary>
        /// Tells the Pen Provider to prefer Extended state or not
        /// </summary>
        /// <param name="extend">Prefer Extended state?</param>
        void SetExtended(bool extend);

        PenState CurrentPenState { get; }

    }
}
