/*
 * File: STM.cs
 * Project: src
 * Created Date: 20/08/2023
 * Author: Shun Suzuki
 * -----
 * Last Modified: 20/08/2023
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2023 Shun Suzuki. All rights reserved.
 * 
 */

#if UNITY_2018_3_OR_NEWER
#define USE_SINGLE
#endif

using System.Collections.Generic;
using System.Linq;

using AUTD3Sharp.Gain;
using AUTD3Sharp.Internal;

#if UNITY_2020_2_OR_NEWER
#nullable enable
#endif

#if UNITY_2018_3_OR_NEWER
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
#else
using Vector3 = AUTD3Sharp.Utils.Vector3d;
#endif

#if USE_SINGLE
using float_t = System.Single;
#else
using float_t = System.Double;
#endif

namespace AUTD3Sharp
{
    using Base = NativeMethods.Base;

    namespace STM
    {
        public abstract class STM : IBody
        {
            private readonly float_t? _freq;
            private readonly float_t? _samplFreq;
            private readonly uint? _samplFreqDiv;
            protected int StartIdxV;
            protected int FinishIdxV;

            protected STM(float_t? freq, float_t? samplFreq, uint? sampleFreqDiv)
            {
                _freq = freq;
                _samplFreq = samplFreq;
                _samplFreqDiv = sampleFreqDiv;
                StartIdxV = -1;
                FinishIdxV = -1;
            }

            public DatagramBodyPtr Ptr(Geometry geometry) => STMPtr(geometry);

            public abstract DatagramBodyPtr STMPtr(Geometry geometry);

            public ushort? StartIdx => StartIdxV == -1 ? null : (ushort?)StartIdxV;

            public ushort? FinishIdx => FinishIdxV == -1 ? null : (ushort?)FinishIdxV;

            protected STMPropsPtr Props()
            {
                var ptr = new STMPropsPtr();
                if (_freq != null)
                    ptr = Base.AUTDSTMProps(_freq.Value);
                if (_samplFreq != null)
                    ptr = Base.AUTDSTMPropsWithSamplingFreq(_samplFreq.Value);
                if (_samplFreqDiv != null)
                    ptr = Base.AUTDSTMPropsWithSamplingFreqDiv(_samplFreqDiv.Value);
                ptr = Base.AUTDSTMPropsWithStartIdx(ptr, StartIdxV);
                ptr = Base.AUTDSTMPropsWithFinishIdx(ptr, FinishIdxV);
                return ptr;
            }

            protected float_t FreqFromSize(int size) => Base.AUTDSTMPropsFrequency(Props(), (ulong)size);
            protected float_t SamplFreqFromSize(int size) => Base.AUTDSTMPropsSamplingFrequency(Props(), (ulong)size);
            protected uint SamplFreqDivFromSize(int size) => Base.AUTDSTMPropsSamplingFrequencyDivision(Props(), (ulong)size);
        }

        /// <summary>
        /// FocusSTM is an STM for moving a focal point
        /// </summary>
        /// <remarks>
        /// <para>The sampling timing is determined by hardware, thus the sampling time is precise.</para>
        /// <para>FocusSTM has following restrictions:</para>
        /// <list>
        /// <item>The maximum number of sampling points is 65536.</item>
        /// <item>The sampling frequency is <see cref="AUTD3.FpgaSubClkFreq">AUTD3.FpgaSubClkFreq</see>/N, where `N` is a 32-bit unsigned integer and must be at 4096.</item>
        /// </list></remarks>
        public sealed class FocusSTM : STM
        {
            private readonly List<float_t> _points;
            private readonly List<byte> _shifts;

            private FocusSTM(float_t? freq, float_t? samplFreq, uint? sampleFreqDiv) : base(freq, samplFreq, sampleFreqDiv)
            {
                _points = new List<float_t>();
                _shifts = new List<byte>();
            }

            public FocusSTM(float_t freq) : this(freq, null, null)
            {
            }

            public static FocusSTM WithSamplingFrequency(float_t freq)
            {
                return new FocusSTM(null, freq, null);
            }

            public static FocusSTM WithSamplingFrequencyDivision(uint freqDiv)
            {
                return new FocusSTM(null, null, freqDiv);
            }

            /// <summary>
            /// Add focus point
            /// </summary>
            /// <param name="point">Focus point</param>
            /// <param name="shift">Duty shift. Duty ratio of ultrasound will be `50% >> duty_shift`. If `duty_shift` is 0, duty ratio is 50%, which means the amplitude is the maximum.</param>
            /// <returns></returns>
            public FocusSTM AddFocus(Vector3 point, byte shift = 0)
            {
                _points.Add(point.x);
                _points.Add(point.y);
                _points.Add(point.z);
                _shifts.Add(shift);
                return this;
            }

            /// <summary>
            /// Add foci
            /// </summary>
            /// <param name="iter">Enumerable of foci</param>
            public FocusSTM AddFociFromIter(IEnumerable<Vector3> iter)
            {
                return iter.Aggregate(this, (stm, point) => stm.AddFocus(point));
            }

            /// <summary>
            /// Add foci
            /// </summary>
            /// <param name="iter">Enumerable of foci and duty shifts</param>
            public FocusSTM AddFociFromIter(IEnumerable<(Vector3, byte)> iter)
            {
                return iter.Aggregate(this, (stm, point) => stm.AddFocus(point.Item1, point.Item2));
            }

            public FocusSTM WithStartIdx(ushort? startIdx)
            {
                StartIdxV = startIdx ?? -1;
                return this;
            }

            public FocusSTM WithFinishIdx(ushort? finishIdx)
            {
                FinishIdxV = finishIdx ?? -1;
                return this;
            }

            public float_t Frequency => FreqFromSize(_shifts.Count);
            public float_t SamplingFrequency => SamplFreqFromSize(_shifts.Count);
            public uint SamplingFrequencyDivision => SamplFreqDivFromSize(_shifts.Count);

            public override DatagramBodyPtr STMPtr(Geometry geometry)
            {
                return Base.AUTDFocusSTM(Props(), _points.ToArray(), _shifts.ToArray(), (ulong)_shifts.Count);
            }
        }

        /// <summary>
        /// FocusSTM is an STM for moving Gains
        /// </summary>
        /// <remarks>
        /// <para>The sampling timing is determined by hardware, thus the sampling time is precise.</para>
        /// <para>FocusSTM has following restrictions:</para>
        /// <list>
        /// <item>The maximum number of sampling Gain is 2048 (Legacy mode) or 1024 (Advanced/AdvancedPhase mode).</item>
        /// <item>The sampling frequency is <see cref="AUTD3.FpgaSubClkFreq">AUTD3.FpgaSubClkFreq</see>/N, where `N` is a 32-bit unsigned integer and must be at 4096.</item>
        /// </list></remarks>
        public sealed class GainSTM : STM
        {
            private readonly List<IGain> _gains;
            private GainSTMMode? _mode;

            private GainSTM(float_t? freq, float_t? samplFreq, uint? sampleFreqDiv) : base(freq, samplFreq, sampleFreqDiv)
            {
                _gains = new List<IGain>();
                _mode = GainSTMMode.PhaseDutyFull;
            }

            public GainSTM(float_t freq) : this(freq, null, null)
            {
            }

            public static GainSTM WithSamplingFrequency(float_t freq)
            {
                return new GainSTM(null, freq, null);
            }

            public static GainSTM WithSamplingFrequencyDivision(uint freqDiv)
            {
                return new GainSTM(null, null, freqDiv);
            }

            /// <summary>
            /// Add Gain
            /// </summary>
            /// <param name="gain">Gain</param>
            /// <returns></returns>
            public GainSTM AddGain(IGain gain)
            {
                _gains.Add(gain);
                return this;
            }

            /// <summary>
            /// Add Gains
            /// </summary>
            /// <param name="iter">Enumerable of Gains</param>
            public GainSTM AddGainsFromIter(IEnumerable<IGain> iter)
            {
                return iter.Aggregate(this, (stm, gain) => stm.AddGain(gain));
            }

            public GainSTM WithStartIdx(ushort? startIdx)
            {
                StartIdxV = startIdx ?? -1;
                return this;
            }

            public GainSTM WithFinishIdx(ushort? finishIdx)
            {
                FinishIdxV = finishIdx ?? -1;
                return this;
            }

            public GainSTM WithMode(GainSTMMode mode)
            {
                _mode = mode;
                return this;
            }

            public float_t Frequency => FreqFromSize(_gains.Count);
            public float_t SamplingFrequency => SamplFreqFromSize(_gains.Count);
            public uint SamplingFrequencyDivision => SamplFreqDivFromSize(_gains.Count);

            public override DatagramBodyPtr STMPtr(Geometry geometry)
            {
                return _gains.Aggregate(_mode.HasValue ? Base.AUTDGainSTMWithMode(Props(), _mode.Value) : Base.AUTDGainSTM(Props()), (current, gain) => Base.AUTDGainSTMAddGain(current, gain.GainPtr(geometry)));
            }
        }
    }
}

#if UNITY_2020_2_OR_NEWER
#nullable restore
#endif
