using System;
using System.Runtime.InteropServices;

namespace Vena
{
    public interface IPairwiseLifeCycleActor<in T> where T : Enum
    {
        void OnEnterPairwiseState(T phase);

        bool OnTickPairwiseLifeCycle(T phase, float time, float deltaTime);

        void OnExitPairwiseState(T phase);
    }

    /// <summary>
    /// 成对生命周期 状态机
    /// </summary>
    /// <typeparam name="TActor"></typeparam>
    /// <typeparam name="T"></typeparam>
    public sealed class PairwiseLifeCycle<TActor, T> where TActor : class, IPairwiseLifeCycleActor<T> where T : Enum
    {
        static readonly Phase[] Phases;

        public readonly TActor actor;
        private sbyte _phaseIndex;

        public sbyte PhaseIndex => _phaseIndex;

        static PairwiseLifeCycle()
        {
            // 获取枚举类型的所有值， 并且转换成数组
            T[] values = (T[])Enum.GetValues(typeof(T));
            int length = values.Length;
            if (length % 2 != 0)
            {
                throw new ArgumentException($"{typeof(T)} must be an even number of enumerated type");
            }

            if (length >= sbyte.MaxValue)
            {
                throw new ArgumentException($"{typeof(T)} must be less than {sbyte.MaxValue}");
            }

            for (int i = 0; i < length; i++)
            {
                int index = Convert.ToInt32(values[i]);
                if (i != index)
                {
                    throw new ArgumentException($"{typeof(T)} must be an enumerated type from 0 to {length - 1}");
                }
            }

            // 构造阶段映射表 - 构造成对阶段
            Phases = new Phase[length];
            for (int index = 0, pairIndex = length - 1; index < pairIndex; index++, pairIndex--)
            {
                T phase = values[index];
                Phases[index] = new Phase(phase, (sbyte)index, (sbyte)pairIndex);

                T pairPhase = values[pairIndex];
                Phases[pairIndex] = new Phase(pairPhase, (sbyte)pairIndex, -1);
            }
        }

        public PairwiseLifeCycle(TActor actor)
        {
            this.actor = actor;
            _phaseIndex = -1;
        }

        public void Reset()
        {
            _phaseIndex = -1;

            _ChangePhaseByIndex(0, 0, 0);
        }

        /// <summary>
        /// 切換生命周期状态，是不能够跨构造周期的（因爲构造流程是依赖异步流程完成的）
        /// 成对的生命周期是存在依赖，析构必须在构造执行后再执行（存在资源依赖）
        /// </summary>
        /// <param name="nextPhaseIndex">目标生命周期阶段</param>
        public void SetPhase(in int nextPhaseIndex)
        {
            if (_phaseIndex < 0)
            {
                throw new Exception($"_current no initilized ！！");
            }

            if (_phaseIndex + 1 == nextPhaseIndex)
            {
                _ChangePhaseByIndex((sbyte)nextPhaseIndex, 0, 0);
                return;
            }

            if (_phaseIndex == nextPhaseIndex)
            {
                return;
            }

            int indexMid = Phases.Length - 1;
            int currAbs = System.Math.Abs(_phaseIndex * 2 - indexMid);
            int nextAbs = System.Math.Abs(nextPhaseIndex * 2 - indexMid);

            // 不能够跨构造周期函数
            ref readonly var currPhase = ref Phases[_phaseIndex];
            ref readonly var nextPhase = ref Phases[nextPhaseIndex];
            if (nextAbs < currAbs)
            {
                throw new Exception($"[{currPhase.self}] can't Change To [{nextPhase.self}] ) ！！");
            }

            int startIndex, endIndex;
            if (currPhase.pair < 0)
            {
                if (nextPhaseIndex < currPhase.index)
                {
                    startIndex = currPhase.index + 1;
                    endIndex = nextPhase.pair;
                }
                else
                {
                    startIndex = currPhase.index + 1;
                    endIndex = nextPhaseIndex - 1;
                }
            }
            else
            {
                if (nextPhaseIndex < currPhase.index)
                {
                    startIndex = currPhase.pair;
                    endIndex = nextPhase.pair;
                }
                else
                {
                    startIndex = currPhase.pair;
                    endIndex = nextPhaseIndex - 1;
                }
            }

            for (int index = startIndex; index <= endIndex; index++)
            {
                _ChangePhaseByIndexUnCheck((sbyte)index);
            }

            _ChangePhaseByIndex((sbyte)nextPhaseIndex, 0, 0);
        }

        public void CheckCondition(float time, float deltaTime)
        {
            if (_phaseIndex >= 0)
            {
                ref readonly var currPhase = ref Phases[_phaseIndex];

                if (actor.OnTickPairwiseLifeCycle(currPhase.self, time, deltaTime))
                {
                    int phaseIndex = (currPhase.index + 1) % Phases.Length;

                    _ChangePhaseByIndex((sbyte)phaseIndex, time, deltaTime);
                }
            }
        }

        private void _ChangePhaseByIndexUnCheck(sbyte index)
        {
            if (_phaseIndex >= 0)
            {
                ref readonly var currPhase = ref Phases[_phaseIndex];
                actor.OnExitPairwiseState(currPhase.self);
            }

            _phaseIndex = index;

            if (_phaseIndex >= 0)
            {
                ref readonly var currPhase = ref Phases[_phaseIndex];
                actor.OnEnterPairwiseState(currPhase.self);
            }
        }

        private void _ChangePhaseByIndex(sbyte index, float time, float deltaTime)
        {
            if (_phaseIndex != index)
            {
                if (_phaseIndex >= 0)
                {
                    ref readonly var currPhase = ref Phases[_phaseIndex];
                    actor.OnExitPairwiseState(currPhase.self);
                }

                _phaseIndex = index;

                if (_phaseIndex >= 0)
                {
                    ref readonly var currPhase = ref Phases[_phaseIndex];
                    actor.OnEnterPairwiseState(currPhase.self);
                    CheckCondition(time, deltaTime);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        readonly struct Phase
        {
            public readonly sbyte index;
            public readonly sbyte pair;
            public readonly T self;

            public Phase(T self, sbyte index, sbyte pair)
            {
                this.self = self;
                this.index = index;
                this.pair = pair;
            }
        }
    }
}