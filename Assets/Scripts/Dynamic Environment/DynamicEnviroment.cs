using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicEnviroment : MonoBehaviour
{
    private Effect physEffect;
    [SerializeField] private EnvironmentState environmentState;

    private void Awake()
    {
        SetEnvironmentState(environmentState);
    }

    public void SetEnvironmentState(EnvironmentState newState)
    {
        Debug.Log("Called");

        // Destroy old component
        if (physEffect != null)
        {
            Debug.Log(physEffect.name);
            SafeDestroy.Object(physEffect);
        }

        // Adjust environment by state
        /*  > Add new physics effect
         *  > Change sky colour
         */
        switch (newState)
        {
            case EnvironmentState.Calm:
                break;

            case EnvironmentState.Windy:
                physEffect = gameObject.AddComponent<WindEffect>();
                break;

            default: goto case EnvironmentState.Calm;
        }

        environmentState = newState;
    }
}
public enum EnvironmentState
{
    Calm,
    Bubbly,
    Windy
}