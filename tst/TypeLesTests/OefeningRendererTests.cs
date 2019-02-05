using FluentAssertions;
using TypeLes;
using Xunit;

namespace TypeLesTests
{
    public class OefeningRendererTests
    {
        private const string Oef = "abcd def ghus alsjd as sala";

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
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Data.Oefeningen[0].Zin, Data.Oefeningen[0].Zin);
            klaar.Should().BeTrue();
        }

        [Fact]
        public void TestX1()
        {
            var (vb, fb, klaar) = OefeningRenderer.LiveFeedback(Data.Oefeningen[0].Zin, Data.Oefeningen[0].Zin.Substring(0, Data.Oefeningen[0].Zin.Length-4));
            klaar.Should().BeFalse();
        }

    }
}
