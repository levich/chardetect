/* 
 * C# port of Mozilla Character Set Detector
 * 
 * Original Mozilla License Block follows
 * 
 */

#region License Block
// Version: MPL 1.1/GPL 2.0/LGPL 2.1
//
// The contents of this file are subject to the Mozilla Public License Version
// 1.1 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
// for the specific language governing rights and limitations under the
// License.
//
// The Original Code is Mozilla Universal charset detector code.
//
// The Initial Developer of the Original Code is
// Netscape Communications Corporation.
// Portions created by the Initial Developer are Copyright (C) 2001
// the Initial Developer. All Rights Reserved.
//
// Contributor(s):
//          Shy Shalom <shooshX@gmail.com>
//
// Alternatively, the contents of this file may be used under the terms of
// either the GNU General Public License Version 2 or later (the "GPL"), or
// the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
// in which case the provisions of the GPL or the LGPL are applicable instead
// of those above. If you wish to allow use of your version of this file only
// under the terms of either the GPL or the LGPL, and not to allow others to
// use your version of this file under the terms of the MPL, indicate your
// decision by deleting the provisions above and replace them with the notice
// and other provisions required by the GPL or the LGPL. If you do not delete
// the provisions above, a recipient may use your version of this file under
// the terms of any one of the MPL, the GPL or the LGPL.
#endregion

using System;
using System.Text;

using CharDetSharp.UniversalCharDet.Model;

namespace CharDetSharp.UniversalCharDet
{
    public abstract class SingleByteCharSetProber : ICharSetProber
    {
        const int SAMPLE_SIZE = 64;
        const int SB_ENOUGH_REL_THRESHOLD = 1024;
        const float POSITIVE_SHORTCUT_THRESHOLD = 0.95f;
        const float NEGATIVE_SHORTCUT_THRESHOLD = 0.05f;
        const float SURE_YES = 0.99f;
        const float SURE_NO = 0.01f;
        const int SYMBOL_CAT_ORDER = 250;
        const int NUMBER_OF_SEQ_CAT = 4;
        const int POSITIVE_CAT = (NUMBER_OF_SEQ_CAT - 1);
        const int NEGATIVE_CAT = 0;

        protected ProbingState mState;
        protected SequenceModel mModel;
        protected bool mReversed; // true if we need to reverse every pair in the model lookup
        protected bool isActive = true;
        protected byte mLastOrder; //char order of last character
        protected int mTotalSeqs;
        protected int[] mSeqCounters = new int[NUMBER_OF_SEQ_CAT];
        protected int mTotalChar;
        protected int mFreqChar; //characters that fall in our sampling range

        protected SingleByteCharSetProber(SequenceModel mModel)
        {
            this.mModel = mModel;
        }

        public string CharSetName { get { return mModel.charSet.WebName; } }
        public Encoding CharSet { get { return mModel.charSet; } }
        public ProbingState State { get { return mState; } }
        
        public float Confidence
        {
            get
            {
#if NEGATIVE_APPROACH
                if (mTotalSeqs > 0)
                    if (mTotalSeqs > mSeqCounters[NEGATIVE_CAT] * 10)
                        return ((float)(mTotalSeqs - mSeqCounters[NEGATIVE_CAT] * 10)) / mTotalSeqs * mFreqChar / mTotalChar;
                return SURE_NO;
#else  //POSITIVE_APPROACH
                float r;

                if (mTotalSeqs > 0)
                {
                    r = ((float)mSeqCounters[POSITIVE_CAT]) / mTotalSeqs / mModel.mTypicalPositiveRatio;
                    r = r * mFreqChar / mTotalChar;
                    if (r >= 1.00f)
                        r = SURE_YES;
                    return r;
                }
                return SURE_NO;
#endif
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                // not Active -> active
                if (!isActive && value)
                    Reset();
                else
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
            if (!isActive) return mState;
            // otherwise, we continue, even if we've made up our mind.

            byte order;

            int end = start + length;
            for (int i = start; i < buffer.Length && i < end; ++i)
            {
                order = mModel.charToOrderMap[buffer[i]];

                if (order < SYMBOL_CAT_ORDER)
                    mTotalChar++;
                if (order < SAMPLE_SIZE)
                {
                    mFreqChar++;

                    if (mLastOrder < SAMPLE_SIZE)
                    {
                        mTotalSeqs++;
                        if (!mReversed)
                            ++(mSeqCounters[mModel.precedenceMatrix[mLastOrder * SAMPLE_SIZE + order]]);
                        else // reverse the order of the letters in the lookup
                            ++(mSeqCounters[mModel.precedenceMatrix[order * SAMPLE_SIZE + mLastOrder]]);
                    }
                }
                mLastOrder = order;
            }

            if (mState == ProbingState.Detecting)
                if (mTotalSeqs > SB_ENOUGH_REL_THRESHOLD)
                {
                    float cf = Confidence;
                    if (cf > POSITIVE_SHORTCUT_THRESHOLD)
                        mState = ProbingState.FoundIt;
                    else if (cf < NEGATIVE_SHORTCUT_THRESHOLD)
                        mState = ProbingState.NotMe;
                    //else
                    //  stay Detecting
                }

            return mState;
        }

        public void Reset()
        {
            this.mState = ProbingState.Detecting;
            this.mLastOrder = 255;
            for (int i = 0; i < NUMBER_OF_SEQ_CAT; i++)
                mSeqCounters[i] = 0;
            this.mTotalSeqs = 0;
            this.mTotalChar = 0;
            this.mFreqChar = 0;
            this.isActive = true;
        }
    }
}
