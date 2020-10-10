using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Logic
{
    public class PIDController
    {
            private double kp, ki, kd;
            private double dispKp, dispKi, dispKd;

            //private double Input, Output, Setpoint;
            private DateTime lastTime;
            private double lastInput;
            private double outputSum;
            private PIDDirection controllerDirection;
            private double outMin, outMax;

            private bool pOnE, inAuto;
            private static int P_ON_M = 0;
            private static int P_ON_E = 1;
            const int REVERSE = 1;
            const int AUTOMATIC = 1;
            const int MANUAL = 0;
            const int DIRECT = 0;
            private int pOn;

            //In Milliseconds
            public double SampleTime { get; set; }


            public double PIDInput { get; set; }
            public double PIDOutput { get; set; }
            public double PIDSetpoint { get; set; }

            public double GetKp() { return dispKp; }
            public double GetKi() { return dispKi; }
            public double GetKd() { return dispKd; }
            public int GetMode() { return inAuto ? AUTOMATIC : MANUAL; }
            public PIDDirection GetDirection() { return controllerDirection; }

            public PIDController(
                double Kp, double Ki, double Kd, int POn)
            {
                inAuto = false;
                this.SetOutputLimits(0, 100);

                SampleTime = 1000;

                SetControllerDirection(PIDDirection.DIRECT);
                setTunings(Kp, Ki, Kd, POn);

                lastTime = DateTime.Now - TimeSpan.FromMilliseconds(SampleTime);
            }

            public bool Compute(double Input)
            {
                PIDInput = Input;
                if (!inAuto) return false;
                DateTime dtNow = DateTime.Now;
                double timeChange = (dtNow.Subtract(lastTime)).TotalMilliseconds;

                if (timeChange >= SampleTime)
                {
                    double input = PIDInput;
                    double error = PIDSetpoint - input;
                    double dInput = (input - lastInput);
                    outputSum += (ki * error);

                    if (!pOnE) outputSum -= kp * dInput;

                    if (outputSum > outMax) outputSum = outMax;
                    else if (outputSum < outMin) outputSum = outMin;

                    double output;

                    if (pOnE) output = kp * error;
                    else output = 0;

                    output += outputSum - kd * dInput;

                    if (output > outMax) output = outMax;
                    else if (output < outMin) output = outMin;
                    PIDOutput = output;

                    lastInput = input;
                    lastTime = dtNow;
                    return true;

                }
                else return false;
            }

            public void setTunings(double Kp, double Ki, double Kd, int POn)
            {
                if (Kp < 0 || Ki < 0 || Kd < 0) return;

                pOn = POn;
                pOnE = POn == P_ON_E;

                dispKp = Kp;
                dispKi = Ki;
                dispKd = Kd;

                double SampleTimeInSec = SampleTime / 1000;

                kp = Kp;
                ki = Ki * SampleTimeInSec;
                kd = Kd / SampleTimeInSec;

                if (controllerDirection == PIDDirection.REVERSE)
                {
                    kp = (0 - kp);
                    ki = (0 - ki);
                    kd = (0 - kd);
                }

            }

            public void setTunings(double Kp, double Ki, double Kd)
            {
                setTunings(Kp, Ki, Kd, pOn);
            }

            /// <summary>
            /// Set Sample Time in milliseconds
            /// </summary>
            /// <param name="NewSampleTime"></param>
            public void SetSampleTime(int NewSampleTime)
            {
                if (NewSampleTime > 0)
                {
                    double ratio = NewSampleTime / SampleTime;

                    ki *= ratio;
                    kd /= ratio;
                    SampleTime = NewSampleTime;
                }
            }

            public void SetOutputLimits(double Min, double Max)
            {
                if (Min >= Max) return;
                outMin = Min;
                outMax = Max;

                if (inAuto)
                {
                    if (PIDOutput > outMax) PIDOutput = outMax;
                    else if (PIDOutput < outMin) PIDOutput = outMin;

                    if (outputSum > outMax) outputSum = outMax;
                    else if (outputSum < outMin) outputSum = outMin;
                }
            }

            public void Setup(PIDMode Mode, double Setpoint)
            {
                PIDSetpoint = Setpoint;

                bool newAuto = (Mode == PIDMode.AUTOMATIC);
                if (newAuto && !inAuto)
                {
                    Initialize();
                }
                inAuto = newAuto;
            }

            public void Initialize()
            {
                outputSum = PIDOutput;
                lastInput = PIDInput;
                if (outputSum > outMax) outputSum = outMax;
                else if (outputSum < outMin) outputSum = outMin;
            }

            public void SetControllerDirection(PIDDirection Direction)
            {
                if (inAuto && (int)Direction != (int)controllerDirection)
                {
                    kp = (0 - kp);
                    ki = (0 - ki);
                    kd = (0 - kd);
                }
                controllerDirection = Direction;
            }
        }

        public enum PIDDirection
        {
            DIRECT = 0,
            REVERSE = 1
        }

        public enum PIDMode
        {
            MANUAL = 0,
            AUTOMATIC = 1
        }
    }

