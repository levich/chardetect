using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using CharDetSharp.UniversalCharDet;

namespace CharDetSharp.UnitTests
{
    [TestFixture]
    public class CyrillicDetectionTestFixture
    {
        [Test]
        public void TestKoi8RDetection()
        {
            RunCyrillicTest(Encoding.GetEncoding("koi8-r"));
        }

        [Test]
        public void TestWin1251Detection()
        {
            RunCyrillicTest(Encoding.GetEncoding("windows-1251"));
        }

        [Test]
        public void TestLatin5Detection()
        {
            RunCyrillicTest(Encoding.GetEncoding("iso-8859-5"));
        }

        [Test]
        public void TestMacCyrillicDetection()
        {
            RunCyrillicTest(Encoding.GetEncoding("x-mac-cyrillic"));
        }

        [Test]
        public void TestIbm855Detection()
        {
            RunCyrillicTest(Encoding.GetEncoding("IBM855"));
        }

        [Test]
        public void TestIbm866Detection()
        {
            RunCyrillicTest(Encoding.GetEncoding("cp866"));
        }

        [Test]
        public void TestUtf8Detection()
        {
            RunCyrillicTest(Encoding.UTF8);
        }

        internal void RunCyrillicTest(Encoding enc)
        {
            Console.Out.WriteLine("Testing [{0}]", enc.WebName);

            ICharSetProber p_koi = new Koi8RCharSetProber();
            ICharSetProber p_1251 = new Win1251CharSetProber();
            ICharSetProber p_lat = new Latin5CharSetProber();
            ICharSetProber p_mac = new MacCyrillicCharSetProber();
            ICharSetProber p_855 = new Ibm855CharSetProber();
            ICharSetProber p_866 = new Ibm866CharSetProber();

            ICharSetProber p_grp = new SBCSGroupProber();

            float c_koi = p_koi.Confidence;
            float c_1251 = p_1251.Confidence;
            float c_lat = p_lat.Confidence;
            float c_mac = p_mac.Confidence;
            float c_855 = p_855.Confidence;
            float c_866 = p_866.Confidence;

            float c_grp = p_grp.Confidence;

            using (StreamReader reader = File.OpenText(@"Samples\ru.utf-8.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    byte[] bytes = enc.GetBytes(line+"\n");
                    p_koi.HandleData(bytes);
                    p_1251.HandleData(bytes);
                    p_lat.HandleData(bytes);
                    p_mac.HandleData(bytes);
                    p_855.HandleData(bytes);
                    p_866.HandleData(bytes);

                    p_grp.HandleData(bytes);

                    c_koi = p_koi.Confidence;
                    c_1251 = p_1251.Confidence;
                    c_lat = p_lat.Confidence;
                    c_mac = p_mac.Confidence;
                    c_855 = p_855.Confidence;
                    c_866 = p_866.Confidence;

                    c_grp = p_grp.Confidence;

                    Console.Out.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t[{6}]", c_koi, c_1251, c_lat, c_mac, c_855, c_866, c_grp);

                    continue;
                }
            }

            Console.Out.WriteLine("Expected: [{0}]   Got: [{1}]  Confidence: [{2}]", enc.WebName, p_grp.CharSet.WebName, p_grp.Confidence);

            Assert.AreEqual(enc, p_grp.CharSet);

            p_grp.Reset();
        }
    }
}

