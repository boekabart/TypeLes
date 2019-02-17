using FluentAssertions;
using TypeLes;
using Xunit;
using Xunit.Sdk;

namespace TypeLesTests
{
    public class OefeningRendererTests
    {
        private const string Oef = "abcd def ghus alsjd as sala";
        private const string OefZinnen = "abcd def ghus\nalsjd as sala";

        [Fact]
        public void Test1()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "a b c ");
            vb.Should().Be("abcd def ghus *alsjd* as sala");
            fb.Should().Be("*    *   *    ");
        }

        [Fact]
        public void Test1b()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "a b c");
            vb.Should().Be("abcd def *ghus* alsjd as sala");
            fb.Should().Be("*    *   *");
        }

        [Fact]
        public void Test2()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "aaaa bbb cccc ");
            vb.Should().Be("abcd def ghus *alsjd* as sala");
            fb.Should().Be("**** *** **** ");
        }

        [Fact]
        public void Test3()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "aaaa bbb cccc");
            vb.Should().Be("abcd def *ghus* alsjd as sala");
            fb.Should().Be("**** *** ****");
        }

        [Fact]
        public void Test3b()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "aaaa  bbb cccc");
            vb.Should().Be("abcd def *ghus* alsjd as sala");
            fb.Should().Be("**** *** ****");
        }

        [Fact]
        public void Test00()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "");
            vb.Should().Be("*abcd* def ghus alsjd as sala");
            fb.Should().Be("");
        }

        [Fact]
        public void Test00s()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, " ");
            vb.Should().Be("*abcd* def ghus alsjd as sala");
            fb.Should().Be("");
        }

        [Fact]
        public void Test0()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "a");
            vb.Should().Be("*abcd* def ghus alsjd as sala");
            fb.Should().Be("*");
        }

        [Fact]
        public void TestBijnaKlaar()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "abcd def ghus alsjd as sal");
            vb.Should().Be("abcd def ghus alsjd as *sala*");
            fb.Should().Be("**** *** **** ***** ** ***");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestBijnaKlaar2()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "abcd def ghus alsjd as ");
            vb.Should().Be("abcd def ghus alsjd as *sala*");
            fb.Should().Be("**** *** **** ***** ** ");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestK()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Oef, "abcd def ghus alsjd as sala");
            vb.Should().Be("abcd def ghus alsjd as sala");
            fb.Should().Be("**** *** **** ***** ** ****");
            klaar.Should().BeTrue();
        }
        [Fact]
        public void TestX()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Data.Oefeningen[0].Zinnen, Data.Oefeningen[0].Zinnen);
            klaar.Should().BeTrue();
        }

        [Fact]
        public void TestX1()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Data.Oefeningen[0].Zinnen, Data.Oefeningen[0].Zinnen.Substring(0, Data.Oefeningen[0].Zinnen.Length-4));
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen0()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "");
            vb.Should().Be("*abcd* def ghus⏎\nalsjd as sala");
            fb.Should().Be("");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen1()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abc");
            vb.Should().Be("*abcd* def ghus⏎\nalsjd as sala");
            fb.Should().Be("***");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen2()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abcde def ghu");
            vb.Should().Be("abcd def *ghus⏎*\nalsjd as sala");
            fb.Should().Be("***+ *** ***");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen3()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abcd def ghus");
            vb.Should().Be("abcd def *ghus⏎*\nalsjd as sala");
            fb.Should().Be("**** *** ****");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen4()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abcd def ghus\n");
            vb.Should().Be("abcd def ghus⏎\n*alsjd* as sala");
            fb.Should().Be("**** *** ****⏎\n");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen5()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abcd def ghus ");
            vb.Should().Be("abcd def ghus⏎\n*alsjd* as sala");
            fb.Should().Be("**** *** ****\n");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen6()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abcd def ghus   \n  ");
            vb.Should().Be("abcd def ghus⏎\n*alsjd* as sala");
            fb.Should().Be("**** *** ****⏎\n");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen7()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abcd def\nghu");
            vb.Should().Be("abcd def *ghus⏎*\nalsjd as sala");
            fb.Should().Be("**** ***⏎***");
            klaar.Should().BeFalse();
        }

        [Fact]
        public void TestZinnen8()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(OefZinnen, "abcd\ndef ghu");
            vb.Should().Be("abcd def *ghus⏎*\nalsjd as sala");
            fb.Should().Be("****⏎*** ***");
            klaar.Should().BeFalse();
        }
    }
}
