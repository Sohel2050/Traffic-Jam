using UnityEngine;
using System.Collections;
public class Timer
{
    private float alarmTime;
    private float currentTime;
    private Timer() { }
    public Timer(float time)
    {
        alarmTime = time;
        currentTime = 0f;
    }
    public bool Tick(float time)
    {
        currentTime += time;
        if (currentTime > alarmTime)
        {
            currentTime = 0f;
            return true;
        }
        return false;
    }
    public void Clear()
    {
        currentTime = 0f;
    }


    private static MonoBehaviour behaviour;
    public delegate void Task();

    public static void Schedule(MonoBehaviour _behaviour, float delay, Task task)
    {
        behaviour = _behaviour;
        if (!behaviour.gameObject.activeInHierarchy) return;
        behaviour.StartCoroutine(DoTask(task, delay));
    }

    public static void StopSchedule(MonoBehaviour _behaviour)
    {
        behaviour = _behaviour;
        if (!behaviour.gameObject.activeInHierarchy) return;
        behaviour.StopAllCoroutines();
    }

    private static IEnumerator DoTask(Task task, float delay)
    {
        yield return new WaitForSeconds(delay);
        task();
    }
}