using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class StartButton : MonoBehaviour
{
    public InstructionWindow InstructionWindow;
    public DataWindow DataWindow;

    public Clock clock;

    public TMP_Text text;

    private bool running = false;

    public void Run()
    {
        running = !running;
        if (running)
        {
            if (InstructionWindow.Modified || DataWindow.Modified)
            {
                var reusable = FindObjectsOfType<MonoBehaviour>().OfType<IReset>();

                foreach (var component in reusable)
                {
                    component.ResetComponent();
                }

                Debug.Log($"Reset completed: {reusable.Count()} components reseted");

                InstructionWindow.UploadMemory();
                DataWindow.UploadMemory();

                InstructionWindow.SetModified(false);
                DataWindow.SetModified(false);
            }
        }

        text.text = running ? "||" : "►";

        clock.ToggleStart(running);
    }

    public void Stop()
    {
        running = false;
        clock.ToggleStart(false);
    }
}
