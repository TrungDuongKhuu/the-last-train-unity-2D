using System;
using UnityEngine;

public static class PlayerPrefsMonitor
{
    // Sự kiện khi bất kỳ pref nào thay đổi
    public static event Action<string> OnChanged;

    // Gọi hàm này sau SetInt / SetFloat / SetString
    public static void NotifyChanged(string key)
    {
        OnChanged?.Invoke(key);
    }
}
