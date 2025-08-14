using UnityEngine;
using System.Collections.Generic;

public class ObjectPool2<T>
{
    private Queue<T> queue = new Queue<T>();
    public delegate T GenerateObjectCallback();
    public delegate void DisableObjectCallback(T obj);
    public delegate void EnableObjectCallback(T obj);
    public delegate void RemoveObjectCallback(T obj);
    public GenerateObjectCallback generator = null;
    public DisableObjectCallback deactivator = null;
    public EnableObjectCallback activator = null;
    public RemoveObjectCallback remover = null;


    public void ReturnObject(T obj)
    {
        deactivator(obj);
        queue.Enqueue(obj);
    }

    public T GetObject()
    {
        T temp;
        if (queue.Count == 0)
        {
            temp = generator();
        }
        else
        {
            temp = queue.Dequeue();
        }
        activator(temp);
        return temp;
    }

    public void Clear()
    {
        while (queue.Count > 0)
        {
            remover(queue.Dequeue());
        }
    }
}