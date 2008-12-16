using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using CharDetSharp.UniversalCharDet;

namespace CharDetSharp.UnitTests
{
    [TestFixture]
    public class GreekProbersTestFixture
    {
        [Test]
        public void TestKoi8RCharSetProber()
        {
            RunGreekTest(Encoding.GetEncoding("ISO-8859-7"));
        }

        [Test]
        public void Win1253CharSetProber()
        {
            RunGreekTest(Encoding.GetEncoding("windows-1253"));
        }

        internal void RunGreekTest(Encoding enc)
        {
            Console.Out.WriteLine("Testing [{0}]", enc.WebName);

            ICharSetProber p_lat = new Latin7CharSetProber();
            ICharSetProber p_1253 = new Win1253CharSetProber();

            ICharSetProber p_grp = new SBCSGroupProber();

            float c_lat = p_lat.Confidence;
            float c_1253 = p_1253.Confidence;

            float c_grp = p_grp.Confidence;

            using (StreamReader reader = File.OpenText(@"Samples\el.utf-8.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.Out.WriteLine(line);
                    byte[] bytes = enc.GetBytes(line+"\n");
                    p_lat.HandleData(bytes);
                    p_1253.HandleData(bytes);

                    p_grp.HandleData(bytes);

                    c_lat = p_lat.Confidence;
                    c_1253 = p_1253.Confidence;

                    c_grp = p_grp.Confidence;

                    Console.Out.WriteLine("{0}\t{1}\t[{2}]", c_lat, c_1253, c_grp);

                    continue;
                }
            }

            Console.Out.WriteLine("Expected: [{0}]   Got: [{1}]  Confidence: [{2}]", enc.WebName, p_grp.CharSet.WebName, p_grp.Confidence);

            Assert.AreEqual(enc, p_grp.CharSet);

            p_grp.Reset();
        }
    }
}
