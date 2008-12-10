using System;
using System.Collections.Generic;
using System.Text;

namespace CharDetSharp.UniversalCharDet
{
    public class SBCSGroupProber : ICharSetProber
    {
        List<ICharSetProber> probers = new List<ICharSetProber>();
        bool isActive;
        ProbingState state;
        ICharSetProber bestGuess;
        int activeNum;

        public SBCSGroupProber()
        {
            probers.Add(bestGuess = new Koi8RCharSetProber());
            probers.Add(new Koi8RCharSetProber());
            probers.Add(new Win1251CharSetProber());
            probers.Add(new Latin5CharSetProber());
            probers.Add(new MacCyrillicCharSetProber());
            probers.Add(new Ibm855CharSetProber());
            probers.Add(new Ibm866CharSetProber());

            activeNum = probers.Count;
            isActive = true;
        }

        public string CharSetName
        {
            get { return bestGuess.CharSetName; }
        }

        public Encoding CharSet
        {
            get { return bestGuess.CharSet; }
        }

        public ProbingState State
        {
            get { return state; }
        }

        public float Confidence
        {
            get
            {
                float bestConf = 0.0f, cf;

                switch (state)
                {
                    case ProbingState.FoundIt:
                        return (float)0.99; //sure yes
                    case ProbingState.NotMe:
                        return (float)0.01;  //sure no
                    default:
                        foreach (ICharSetProber prober in probers)
                        {
                            if (!prober.IsActive)
                                continue;
                            cf = prober.Confidence;
                            if (bestConf < cf)
                            {
                                bestConf = cf;
                                bestGuess = prober;
                            }
                        }
                        break;
                }
                return bestConf;
            }
        }

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (!isActive && value)
                    Reset();
                isActive = value;
            }
        }

        public ProbingState HandleData(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("The buffer cannot be null");

            return HandleData(buffer, 0, buffer.Length);
        }

        public ProbingState HandleData(byte[] buffer, int start, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException("The buffer cannot be null");

            // if we are not active, we needn't do any work.
            if (!isActive) return state;
            // otherwise, we continue, even if we've made up our mind.

            foreach (ICharSetProber prober in probers)
            {
                if (!prober.IsActive) continue;

                ProbingState st = prober.HandleData(buffer, start, length);
                if (st == ProbingState.FoundIt)
                {
                    bestGuess = prober;
                    state = ProbingState.FoundIt;
                    break;
                }
                else if (st == ProbingState.NotMe)
                {
                    prober.IsActive = false;
                    activeNum--;
                    if (activeNum <= 0)
                    {
                        state = ProbingState.NotMe;
                        isActive = false;
                        break;
                    }
                }
            }

            return state;
        }

        public void Reset()
        {
            foreach (ICharSetProber prober in probers)
            {
                prober.Reset();
                prober.IsActive = true;
            }
            isActive = true;
            activeNum = probers.Count;
        }
    }
}
