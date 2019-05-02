using UnityEngine;
using System.Collections;

public class DynamicEnvironmentHotkeys : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private DynamicEnviroment dynEnvironment;
#pragma warning restore 0649

    public void SetWindy() => dynEnvironment.SetEnvironmentState(EnvironmentState.Windy);
    public void SetCalm() => dynEnvironment.SetEnvironmentState(EnvironmentState.Calm);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetCalm();
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWindy();
    }
}
