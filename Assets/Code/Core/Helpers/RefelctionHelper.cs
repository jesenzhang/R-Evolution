using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class RefelctionHelper
{
    public static T CreateNew<T>() where T : class
    {
        // 获取私有构造函数
        var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // 获取无参构造函数
        var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);

        if (ctor == null)
        {
            throw new Exception("Constructor() not found! in " + typeof(T));
        }

        // 通过构造函数，常见实例
        var mInstance = ctor.Invoke(null) as T;
        return mInstance;
    }
}
