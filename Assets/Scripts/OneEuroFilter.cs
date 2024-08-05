using System;
using UnityEngine;

public class OneEuroFilter
{
    private double freq;
    private double minCutoff;
    private double beta;
    private double dCutoff;
    private double xPrev;
    private double dxPrev;
    private double tPrev;
    private double minInput;
    private double maxInput;
    private double minOutput;
    private double maxOutput;
    private bool isOneEuro;
    private double multiplier;
    private double inputRange;
    private double outputRange;
    private double filterMultiplier;

    public OneEuroFilter(double minCutoff, double beta,double minInput, double maxInput,double minOutput, double maxOutput, bool isOneEuro, double multiplier, double dCutoff = 1.0, double freq = 120.0)
    {
        this.freq = freq;
        this.minCutoff = minCutoff;
        this.beta = beta;
        this.dCutoff = dCutoff;
        this.minInput = minInput;
        this.maxInput = maxInput;
        this.minOutput = minOutput;
        this.maxOutput = maxOutput;
        this.multiplier = multiplier;
        this.inputRange = maxInput - minInput;
        this.outputRange = maxOutput - minOutput;
        this.filterMultiplier = inputRange / outputRange;
        this.isOneEuro = isOneEuro;
        this.xPrev = 0.0;
        this.dxPrev = 0.0;
        this.tPrev = Time.time;
    }

    private double Alpha(double cutoff)
    {
        double te = 1.0 / freq;
        double tau = 1.0 / (2.0 * Math.PI * cutoff);
        return 1.0 / (1.0 + tau / te);
    }

    private double Filter(double x, double xPrev, double alpha)
    {
        return alpha * x + (1.0 - alpha) * xPrev;
    }

    public double CalculateValue(double value)
    {
        value *= multiplier;
        if (value<minInput)
        {
            value = 0;
        }
        else if(value>=maxInput)
        {
            value = maxInput;
        }
        if(this.isOneEuro)
        {
            value = OneEuroFilterCalculation(value);
        }
        value = (value-minInput)*filterMultiplier;
        value += minOutput;

        if (value>maxOutput)
        {
            return maxOutput > 100 ? 100 : maxOutput;
        }
        else
        {
            return value;
        }
    }

    private double OneEuroFilterCalculation(double value)
    {
        double t = Time.time;
        double dt = t - tPrev;

        // Protect against division by zero or near zero
        if (dt <= 0.0)
        {
            dt = 1.0 / freq;
        }

        freq = 1.0 / dt;

        // Estimate the current derivative of the signal
        double dx = (value - xPrev) / dt;
        double edx = Filter(dx, dxPrev, Alpha(dCutoff));

        // Use the estimated derivative to compute the cutoff frequency
        double cutoff = minCutoff + beta * Math.Abs(edx);
        double alpha = Alpha(cutoff);

        // Filter the value
        double result = Filter(value, xPrev, alpha);

        // Update state
        xPrev = result;
        dxPrev = edx;
        tPrev = t;

        return result;
    }
}
