using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace FoW
{
    enum FogOfWarThreadState
    {
        Running,
        Stopping,
        Stopped
    }

    class FogOfWarThread
    {
        public bool isWaiting { get { return action == null; } }
        public FogOfWarThreadState state { get; private set; }
        public FogOfWarThreadPool threadPool { get; private set; }
        public System.Action action { get; private set; }
        Thread _thread;

        public FogOfWarThread(FogOfWarThreadPool pool)
        {
            threadPool = pool;
            state = FogOfWarThreadState.Running;
            _thread = new Thread(ThreadRun);
            _thread.Start();
        }

        void ThreadRun()
        {
            while (state == FogOfWarThreadState.Running)
            {
                if (action != null)
                {
                    try
                    {
                        action();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                    action = null;
                }
                else
                {
                    action = threadPool.RequestNewAction(this);
                    if (action == null)
                        Thread.Sleep(threadPool.sleepTime);
                }
            }
            state = FogOfWarThreadState.Stopped;
            _thread = null;
        }

        public void Run(System.Action newaction)
        {
            if (action != null)
                Debug.LogError("FogOfWarThread is trying to start process before another ends!");
            else
                action = newaction;
        }

        public void Stop()
        {
            if (state == FogOfWarThreadState.Running)
                state = FogOfWarThreadState.Stopping;
        }
    }

    public class FogOfWarThreadPool
    {
        public int maxThreads = 2;
        public int sleepTime { get { return 1; } }
        public bool hasAllFinished { get { return _actionQueue.Count == 0 && _threads.Find(t => !t.isWaiting) == null; } }

        List<FogOfWarThread> _threads = new List<FogOfWarThread>();
        List<System.Action> _actionQueue = new List<System.Action>();

        void RemoveStoppedThreads()
        {
            _threads.RemoveAll(t => t.state == FogOfWarThreadState.Stopped);
        }

        // this should be called once per frame
        public void Clean()
        {
            // remove any unneeded threads
            if (_threads.Count > maxThreads)
            {
                RemoveStoppedThreads();
                for (int i = maxThreads; i < _threads.Count; ++i)
                    _threads[i].Stop();
            }
        }

        public void Run(System.Action action)
        {
            // add to any waiting threads
            for (int i = maxThreads; i < _threads.Count; ++i)
            {
                if (_threads[i].state == FogOfWarThreadState.Running && _threads[i].isWaiting)
                {
                    _threads[i].Run(action);
                    return;
                }
            }

            // create thread
            if (_threads.Count < maxThreads)
            {
                FogOfWarThread newthread = new FogOfWarThread(this);
                _threads.Add(newthread);
                newthread.Run(action);
                return;
            }

            // no available threads, so just add it to the queue
            lock (_actionQueue)
                _actionQueue.Add(action);
        }

        internal System.Action RequestNewAction(FogOfWarThread thread)
        {
            lock (_actionQueue)
            {
                if (_actionQueue.Count > 0)
                {
                    System.Action newaction = _actionQueue[_actionQueue.Count - 1];
                    _actionQueue.RemoveAt(_actionQueue.Count - 1);
                    return newaction;
                }
            }
            return null;
        }

        public void StopAllThreads()
        {
            for (int i = maxThreads; i < _threads.Count; ++i)
                _threads[i].Stop();
        }

        public void WaitUntilFinished()
        {
            while (_actionQueue.Count > 0)
                Thread.Sleep(sleepTime);
            for (int i = 0; i < _threads.Count; ++i)
            {
                while (!_threads[i].isWaiting)
                    Thread.Sleep(sleepTime);
            }
        }
    }
}