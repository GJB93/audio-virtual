using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/** References: 
 * https://youtu.be/wtXirrO-iNA
 * https://youtu.be/4Av788P9stk
 * 
**/

[RequireComponent (typeof (AudioSource))]
public class AudioAnalyser
{
    private const int SAMPLE_SIZE = 1024;
    private const int BAND_SIZE = 7;

    private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float songMaxFrequency;
    private float hzPerInterval;

    private AudioAnalyser()
    {
        this.samples = new float[SAMPLE_SIZE];
        this.spectrum = new float[SAMPLE_SIZE];
    }

    public AudioAnalyser(AudioSource source, sampleRate)
    {
        AudioAnalyser();
        this.source = source;
        this.songMaxFrequency = sampleRate / 2;
        /*
         * Assuming a common song frequency of ~22kHz
         * 22000 / 1024 = 21.5 Hz a interval
         */
        this.hzPerInterval = this.songMaxFrequency / this.SAMPLE_SIZE;
        Debug.Log(this.sampleRate);
    }

    public float GetRmsValue(float[] samples)
    {
        float sum = 0;
        for (int i = 0; i < SAMPLE_SIZE; i += 1)
        {
            sum += samples[i] * samples[i];
        }
        // Debug.Log("RMSValue is " + Mathf.Sqrt(sum / SAMPLE_SIZE));
        return Mathf.Sqrt(sum / SAMPLE_SIZE);
    }

    public float GetDBValue(float measuredVoltage, float referenceVoltage)
    {
        // Debug.Log("dbValue is " + (20 * Mathf.Log10(rmsValue / 0.1f)));
        return 20 * Mathf.Log10(this.rmsValue / 0.1f);
    }

    public float GetVoltageRatio(float decibelValue)
    {
        Debug.Log("VoltageRatio is " + (Mathf.Pow(10, (decibelValue / 20))));
        return Mathf.Pow(10, (decibelValue/20));
    }

    public float GetPitchValue(float[] spectrum)
    {
        float maxV = 0;
        var maxN = 0;
        for (int i = 0; i < SAMPLE_SIZE; i += 1)
        {
            if (!(spectrum[i] > maxV) || !(spectrum[i] > 0.0f))
                continue;

            maxV = spectrum[i];
            maxN = i;
        }

        float freqN = maxN;
        if (maxN > 0 && maxN < SAMPLE_SIZE - 1)
        {
            var dL = spectrum[maxN - 1] / spectrum[maxN];
            var dR = spectrum[maxN - 1] / spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }

        return freqN * this.hzPerInterval;
    }

    public List<float> GetBandAverages(float[] spectrum)
    {
        /*
         * Sub-Bass:            20Hz - 60Hz         => 40Hz bandwidth
         * Bass:                60Hz - 250Hz        => 190Hz bandwidth
         * Low Midrange =       250Hz - 500Hz       => 250Hz bandwidth
         * Midrange =           500Hz - 2kHz        => 1.5kHz bandwidth
         * Upper Midrange =     2kHz - 4kHz         => 2kHz bandwidth
         * Presence =           4kHz - 6kHz         => 2kHz bandwidth
         * Brilliance =         6kHz - 20kHz        => 14kHz bandwidth
         */

        List<float> averages = new List<float>();

        List<float> subBass       = new List<float>();
        List<float> bass          = new List<float>();
        List<float> lowMidrange   = new List<float>();
        List<float> midrange      = new List<float>();
        List<float> upperMidrange = new List<float>();
        List<float> presence      = new List<float>();
        List<float> brilliance    = new List<float>();

        int subBassRange          = (int)(40 / this.hzPerInterval);
        int bassRange             = (int)(190 / this.hzPerInterval) + subBassRange;
        int lowMidrangeRange      = (int)(250 / this.hzPerInterval) + bassRange;
        int midrangeRange         = (int)(1500 / this.hzPerInterval) + lowMidrangeRange;
        int upperMidrangeRange    = (int)(2000 / this.hzPerInterval) + midrangeRange;
        int presenceRange         = (int)(2000 / this.hzPerInterval) + upperMidrangeRange;
        int brillianceRange       = (int)(14000 / this.hzPerInterval) + presenceRange;

        for (int interval = 1; interval <= brillianceRange; interval += 1)
        {
            if (interval <= subBassRange)
            {
                subBass.Add(spectrum[interval]);
            }
            else if (interval > subBassRange && interval <= bassRange)
            {
                bass.Add(spectrum[interval]);
            }
            else if (interval > bassRange && interval <= lowMidrangeRange)
            {
                lowMidrange.Add(spectrum[interval]);
            }
            else if (interval > lowMidrangeRange && interval <= midrangeRange)
            {
                midrange.Add(spectrum[interval]);
            }
            else if (interval > midrangeRange && interval <= upperMidrangeRange)
            {
                upperMidrange.Add(spectrum[interval]);
            }
            else if (interval > upperMidrangeRange && interval <= presenceRange)
            {
                presence.Add(spectrum[interval]);
            }
            else
            {
                brilliance.Add(spectrum[interval]);
            }
        }

        averages.Add(subBass.Average());
        averages.Add(bass.Average());
        averages.Add(lowMidrange.Average());
        averages.Add(midrange.Average());
        averages.Add(upperMidrange.Average());
        averages.Add(presence.Average());
        averages.Add(brilliance.Average());

        return averages;
    }
}
