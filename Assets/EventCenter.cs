using System;
using System.Collections.Generic;

public static class EventCenter
{
    private static Dictionary<Type, Delegate> eventDic =
        new Dictionary<Type, Delegate>();

    // 注册
    public static void AddListener<T>(Action<T> action)
    {
        Type type = typeof(T);

        if (eventDic.ContainsKey(type))
            eventDic[type] = Delegate.Combine(eventDic[type], action);
        else
            eventDic[type] = action;
    }

    // 移除
    public static void RemoveListener<T>(Action<T> action)
    {
        Type type = typeof(T);

        if (!eventDic.ContainsKey(type)) return;

        var newDel = Delegate.Remove(eventDic[type], action);

        if (newDel == null)
            eventDic.Remove(type);
        else
            eventDic[type] = newDel;
    }

    // 触发
    public static void Trigger<T>(T eventData)
    {
        Type type = typeof(T);

        if (eventDic.ContainsKey(type))
        {
            var action = eventDic[type] as Action<T>;
            action?.Invoke(eventData);
        }
    }
}