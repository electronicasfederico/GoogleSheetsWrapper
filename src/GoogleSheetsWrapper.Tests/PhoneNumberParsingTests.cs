using GoogleSheetsWrapper.Utils;
using NUnit.Framework;

namespace GoogleSheetsWrapper.Tests
{
    public class PhoneNumberParsingTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RemoveExtraPhoneNumberCharacters()
        {
            var result = PhoneNumberParsing.RemoveExtraCharactersFromPhoneNumber("(703)111-2222");

            Assert.AreEqual("7031112222", result);
        }

        [Test]
        public void RemoveInternationalCodeRemovesUSCode()
        {
            var result = PhoneNumberParsing.RemoveUSInterationalPhoneCode("+17031112222");

            Assert.AreEqual("7031112222", result);
        }

        [Test]
        public void RemoveInternationalCodeRemovesUSCodeAndSpecialCharacters()
        {
            var result = PhoneNumberParsing.RemoveUSInterationalPhoneCode("+1(703)111-2222");

            Assert.AreEqual("7031112222", result);
        }
    }
}