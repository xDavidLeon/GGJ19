using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMoon : MonoBehaviour
{
    public Transform sun;
    public Transform moon;

    public float speed = 1.0f;

    void Update()
    {
        sun.Rotate(speed * Time.deltaTime, 0, 0);
        moon.Rotate(speed * Time.deltaTime, 0, 0);

        float intensitySun = sun.localEulerAngles.x % 180;
        intensitySun = 1.0f - Mathf.Abs(intensitySun - 90) / 90.0f;
        sun.GetComponent<Light>().shadowStrength = intensitySun;

        float intensityMoon = moon.localEulerAngles.x % 180;
        intensityMoon = 1.0f - Mathf.Abs(intensityMoon - 90) / 90.0f;
        moon.GetComponent<Light>().shadowStrength = intensityMoon;

    }
}
