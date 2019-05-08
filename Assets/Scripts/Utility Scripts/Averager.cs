using UnityEngine;
using System.Collections;

public class Averager
{
    private float[] floats;

    public Averager(int bufferSize)
    {
        if (bufferSize <= 3) throw new BadBufferSizeException("Please pass a bufferSize value greater than three");
        floats = new float[bufferSize];
    }

    public void Track(float newFloat)
    {
        for (int i = 1; i < floats.Length; i++)
        {
            floats[i] = floats[i - 1];
        }
        floats[0] = newFloat;
    }
    public float GetAverage()
    {
        var avg = 0f;
        //Debug.Log("Begin new avg");
        foreach (float storedFloat in floats)
        {
            //Debug.Log(storedFloat);
            avg += storedFloat;
        }
        return avg / floats.Length;
    }
}
