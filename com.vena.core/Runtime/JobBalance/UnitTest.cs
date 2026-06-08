using UnityEngine;

namespace Vena.Test
{
    public static class FpsBalancerUnitTest
    {
        class TestJob : ISteppedJob
        {
            private int _totalStep;
            
            private int _priority;
            
            private string _name;
            
            private int _index;
            
            private Matrix4x4 _matrix4 = Matrix4x4.TRS(Vector3.forward, Quaternion.identity, Vector3.one);

            public TestJob(string name, int totalStep, int priority)
            {
                _name = name;
                
                _totalStep = totalStep;
                
                _priority = priority;
                
                _index = 0;
            }

            public int priority => _priority;

            public JobResult ExecuteStep()
            {
                Debug.Warning($"{this} : ExecuteStep( {_index} )");
                
                Vector3 vector3 = new Vector3(_index, _index, _index);
                
                for (int j = 0; j < _index; j++)
                {
                    for (int k = 0; k < 100; k++)
                    {
                        vector3 = _matrix4.MultiplyPoint3x4(vector3);
                    }
                }

                _index++;
                
                return new JobResult(_index >= _totalStep);
            }

            public override string ToString()
            {
                return $"[Job]_{_name},_totalStep:{_totalStep},_priority:{_priority}";
            }
        }


        public static void Test()
        {
            for (int i = 0; i < 25; i++)
            {
                var c = i + 'A';
                
                int priority = Random.Range(0, 100);
                
                TestJob job = new TestJob($"{(char)c}", 50, priority);
                
                //M<IJobBalancer>.Inst.DoJob(job);
            }
        }
    }
}