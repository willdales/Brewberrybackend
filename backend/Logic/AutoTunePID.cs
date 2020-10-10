using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace backend.Logic
{
    public class AutoTunePID
    {
        
        public double Input;
        public double Output;
        bool justevaled;
        int peakCount;
        bool running;
        PIDParameter PIDparams;
        DateTime lastTime;
        int sampletime;
        int peakType;
        bool justchanged;
        double absMax, absMin;
        double setpoint;
        double outputStart;
        double oStep;
        double noiseband;
        bool isMax, isMin;
        int nLookBack;
        double[] lastInputs;
        double[] peaks;
        DateTime peak1, peak2;

        double Ku, Pu;


        public AutoTunePID(PIDParameter _setParams)
        {
            lastInputs = new double[101];
            peaks = new double[10];
            running = false;
            peakCount = 0;
            PIDparams = _setParams;
            lastTime = DateTime.Now;
            oStep = 30;
        }

        private void ResetParams(PIDParameter _setParams)
        {
            PIDparams = _setParams;
        }

        public int Runtime()
        {
            justevaled = false;

            if(peakCount > 9 && running)
            {
                running = false;
                FinishUp();
                return 1;
            }

            DateTime current = DateTime.Now;

            if (current.Subtract(lastTime).TotalMilliseconds < sampletime) return 0;
            lastTime = current;
            /////////////////////////////////////////////////////////////////////????????????????????????????????????????????
            double refVal = PIDparams.LastValue;

            justevaled = true;

            if(!running)
            {
                peakType = 0;
                peakCount = 0;
                justchanged = false;
                absMax = refVal;
                absMin = refVal;
                setpoint = refVal;
                running = true;
                outputStart = PIDparams.LastOutput;
                PIDparams.LastOutput = outputStart + oStep;
                
            }
            else
            {
                if (refVal > absMax) absMax = refVal;
                if (refVal < absMin) absMin = refVal;
            }

            if (refVal > setpoint + noiseband) PIDparams.LastOutput = outputStart - oStep;
            else if (refVal < setpoint - noiseband) PIDparams.LastOutput = outputStart + oStep;

            isMax = true;
            isMin = true;

            for(int i = nLookBack-1;i>=0; i--)
            {
                double val = lastInputs[i];
                if (isMax) isMax = refVal > val;
                if (isMin) isMin = refVal < val;

                lastInputs[i + 1] = lastInputs[i];
            }
            lastInputs[0] = refVal;

            if(nLookBack<9)
            {
                return 0;
            }

            if (isMax)
            {
                if (peakType == 0) peakType = 1;
                if (peakType == -1)
                {
                    peakType = 1;
                    justchanged = true;
                    peak2 = peak1;
                }
                peak1 = current;
                peaks[peakCount] = refVal;
            }
            else if (isMin)
            {
                if (peakType == 0) peakType = -1;
                if (peakType == 1)
                {
                    peakType = -1;
                    peakCount++;
                    justchanged = true;
                }

                if (peakCount < 10) peaks[peakCount] = refVal;
            }

            if(justchanged && peakCount >2)
            {
                double avgSeparation = (Math.Abs(peaks[peakCount - 1] - peaks[peakCount - 2]) + Math.Abs(peaks[peakCount - 2] - peaks[peakCount - 3])) / 2;
                if(avgSeparation < 0.05*(absMax -absMin))
                {
                    FinishUp();
                    running = false;
                    return 1;
                }

            }

            justchanged = false;
            return 0;


        }

        public void FinishUp()
        {
            PIDparams.LastOutput = outputStart;
            Ku = 4 * (2 * oStep) / ((absMax - absMin) * Math.PI);
            Pu = (double)(peak1.Subtract(peak2).TotalMilliseconds) / 1000;

        }

        public PIDParameter GetTuningValues()
        {
            PIDparams.Kp = 0.6 * Ku;
            PIDparams.Ki = 1.2 * Ku / Pu;
            PIDparams.Kd = 0.075 * Ku * Pu;

            return PIDparams;
        }


        public void SetOutputStep(double Step)
        {
            oStep = Step;
        }

        public void SetNoiseBand(double Band)
        {
            noiseband = Band;
        }

        public void SetLookbackSec(int value)
        {
            if (value < 1) value = 1;

            if(value<25)
            {
                nLookBack = value * 4;
                sampletime = 250;
            }
            else
            {
                nLookBack = 100;
                sampletime = value * 10;
            }
        }

        public double GetOutput()
        {
            return PIDparams.LastOutput;
        }

    }
}
