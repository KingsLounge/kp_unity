using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using System;

public class AsyncOperationAwaiter : INotifyCompletion
{
    private AsyncOperation asyncOp;
    private Action continuation;

    public AsyncOperationAwaiter(AsyncOperation asyncOp)
    {
        this.asyncOp = asyncOp;
        asyncOp.completed += OnRequestCompleted;
    }

    public bool IsCompleted { get { return asyncOp.isDone; } }

    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        this.continuation = continuation;
    }

    private void OnRequestCompleted(AsyncOperation obj)
    {
        continuation();
    }
}

public static class ExtensionMethods
{
    public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation asyncOp)
    {
        return new AsyncOperationAwaiter(asyncOp);
    }
}