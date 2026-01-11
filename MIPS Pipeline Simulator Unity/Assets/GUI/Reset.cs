using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IReset
{
    public void ResetComponent();
}

public class Reset : MonoBehaviour
{
    public void ResetAll()
    {
        var reusable = FindObjectsOfType<MonoBehaviour>().OfType<IReset>();

        foreach (var component in reusable)
        {
            component.ResetComponent();
        }

        Debug.Log($"Reset completed: {reusable.Count()} components reseted");
    }
}
