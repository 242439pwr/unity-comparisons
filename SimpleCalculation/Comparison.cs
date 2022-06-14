using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

public class Comparison : MonoBehaviour
{
    [SerializeField]
    private int _iterations;
    private enum Method
    {
        Standard,
        Tasks,
        JobsArray,
        Coroutine,
        Print,
        Threads
    }

    [SerializeField]
    private Method _method;
    private readonly Stopwatch _stopWatch = new Stopwatch();

    private NativeArray<JobHandle> _nativeHandles;
    private Task[] _tasks;

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            _nativeHandles = new NativeArray<JobHandle>(
                          _iterations, Allocator.Temp);
            _tasks = new Task[_iterations];

            switch (_method)
            {
                case Method.Standard:
                    {
                        _stopWatch.Start();
                        Profiler.BeginSample("dupa");
                        for (int i = 0; i < _iterations; ++i)
                        {
                            DumbTest();
                        }
                        Profiler.EndSample();
                        _stopWatch.Stop();
                        UnityEngine.Debug.Log(_method.ToString() + " " +
                                   _stopWatch.ElapsedMilliseconds + "ms");
                        _stopWatch.Reset();
                        break;
                    }
                case Method.Tasks:
                    {
                        _stopWatch.Start();
                        for (int i = 0; i < _iterations; ++i)
                        {
                            _tasks[i] = Task.Factory.StartNew(DumbTest);
                        }
                        Task.WaitAll(_tasks);
                        _stopWatch.Stop();
                        UnityEngine.Debug.Log(_method.ToString() + " " +
                                    _stopWatch.ElapsedMilliseconds + "ms");
                        _stopWatch.Reset();
                        break;
                    }
                case Method.Threads:
                    {
                        _stopWatch.Start();
                        Enumerable.Range(0, _iterations).Select(x =>
                        {
                            Thread t = new Thread(DumbTest);
                            t.Start();
                            return t;
                        }).ToList().ForEach(t => t.Join());
                        _stopWatch.Stop();
                        UnityEngine.Debug.Log(_method.ToString() + " " +
                                   _stopWatch.ElapsedMilliseconds + "ms");
                        _stopWatch.Reset();
                        break;
                    }
                case Method.JobsArray:
                    {
                        _stopWatch.Start();
                        for (int i = 0; i < _iterations; ++i)
                        {
                            _nativeHandles[i] = CreateJobHandle();
                        }
                        JobHandle.CompleteAll(_nativeHandles);
                        _stopWatch.Stop();
                        UnityEngine.Debug.Log(_method.ToString() + " " +
                                   _stopWatch.ElapsedMilliseconds + "ms");
                        _stopWatch.Reset();
                        break;
                    }
                case Method.Coroutine:
                    {
                        _stopWatch.Start();
                        for (int i = 0; i < _iterations; ++i)
                        {
                            DumbCoroutine();
                        }
                        _stopWatch.Stop();
                        UnityEngine.Debug.Log(_method.ToString() + " " +
                                   _stopWatch.ElapsedMilliseconds + "ms");
                        _stopWatch.Reset();
                        break;
                    }
            }
        }
    }

    IEnumerator DumbCoroutine()
    {
        DumbTest();
        yield return null;
    }

    public JobHandle CreateJobHandle()
    {
        var job = new TestJob();
        return job.Schedule();
    }

    [BurstCompile]
    public struct TestJob : IJob
    {
        public void Execute()
        {
            DumbTest();
        }
    }

    public static void DumbTest()
    {
        float value = 0f;
        for (int i = 0; i < 60000; ++i)
        {
            value += Mathf.Exp(i) * Mathf.Sqrt(value);
        }
    }
}
