using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

namespace UnityThreading
{
    public class Thread : MonoBehaviour
    {
        private static readonly object LOCK = new object();

        private static Thread sInstance;

        private static bool sInstanceCreated;

        private static Thread Instance
        {
            get
            {
                if (!sInstanceCreated)
                {
                    lock (LOCK)
                    {
                        if (!sInstanceCreated)
                        {
                            sInstance = GetInstance();
                            sInstanceCreated = true;
                        }
                    }
                }
                return sInstance;
            }
        }

        private static Thread GetInstance()
        {
            GameObject o = new GameObject("UnityThreading.Thread");
            return o.AddComponent<Thread>();
        }

        public static void InBackground(Action action)
        {
            Instance.SpawnThread(action);
        }

        public static void InForeground(Action action)
        {
            Instance.QueueEvent(action);
        }

        void OnDestroy()
        {
            lock (LOCK)
            {
                sInstanceCreated = false;
            }
        }

        private List<Action> mEventQueue;
        private List<Action> mDispatchQueue;

        void Start()
        {
            mEventQueue = new List<Action>();
            mDispatchQueue = new List<Action>();
        }

        void Update()
        {
            lock (mEventQueue)
            {
                mDispatchQueue.Clear();
                mDispatchQueue.AddRange(mEventQueue);
                mEventQueue.Clear();
            }
            foreach (Action action in mDispatchQueue)
            {
                action();
            }
        }

        void SpawnThread(Action action)
        {
            new System.Threading.Thread(new ThreadStart(action)).Start();
        }

        void QueueEvent(Action action)
        {
            lock (mEventQueue)
            {
                mEventQueue.Add(action);
            }
        }
    }
}
