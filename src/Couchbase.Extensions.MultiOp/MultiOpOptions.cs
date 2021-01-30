using System;
using System.Diagnostics;
using Couchbase.Extensions.MultiOp.Internal;

namespace Couchbase.Extensions.MultiOp
{
    /// <summary>
    /// Options to control parallelization.
    /// </summary>
    public class MultiOpOptions
    {
        private static readonly int DefaultDegreeOfParallelism = Environment.ProcessorCount * 4;

        internal static MultiOpOptions Default { get; }

        static MultiOpOptions()
        {
            // Do this in a static constructor to be sure DefaultDegreeOfParallelism is set first
            Default = new MultiOpOptions();
        }

        private int _degreeOfParallelism = DefaultDegreeOfParallelism;

        /// <summary>
        /// The maximum number of operations to execute simultaneously.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value should be tuned to maximize efficiency. The more CPU cores
        /// available and the more Couchbase data nodes, the higher this number may be.
        /// Making the value too high may reduce efficiency due to context switching and
        /// serializing too many operations in advance of available network capacity.
        ///</para>
        /// <para>
        /// In standalone applications, such as bulk imports, a higher number will generally
        /// be more effective. In multi-use applications, such as web applications, a lower
        /// number will help reduce interference between multiple incoming requests. The
        /// default value is Environment.ProcessorCount * 4.
        /// </para>
        /// </remarks>
        public int DegreeOfParallelism
        {
            get => _degreeOfParallelism;
            set
            {
                Debug.Assert(!ReferenceEquals(this, Default));
                if (value < 1)
                {
                    ThrowHelper.ThrowArgumentOutOfRange(nameof(value));
                }

                _degreeOfParallelism = value;
            }
        }
    }
}
