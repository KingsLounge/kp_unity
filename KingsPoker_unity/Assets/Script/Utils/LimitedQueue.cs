using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitedQueue<T> : Queue<T>
{
    private int _limit = 1;
    public int limit
    {
        get
        {
            return _limit;
        }
        set
        {
            _limit = value;
            for(int i = 0; i < limit - Count; i++)
            {
                base.Dequeue();
            }
        }
    }
    public LimitedQueue(int limit)
    {
        _limit = limit;
    }

    public new void Enqueue(T item)
    {
        base.Enqueue(item);

        for (int i = 0; i < limit - Count; i++)
        {
            Dequeue();
        }
    }
}
