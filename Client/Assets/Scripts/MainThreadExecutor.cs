using System;
using UnityEngine;
using System.Collections.Generic;

public class MainThreadExecutor : MonoBehaviour
{
    static Queue<Action> actions = new Queue<Action>(); // 非同期タスク

    void Update()
    {
        lock (actions)
        {
            while (actions.Count != 0)
            {
                var action = actions.Dequeue();
                action();
            }
        }
    }

    public static void Enqueue(Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }
}
