using System;
using Xunit;
using ErrorOr;
using Application.Commands.Publications;

namespace Application.UnitTests {
    public class ValidationTest {
        [Fact]
        public void TestIErrorOr() {
            Assert.True(typeof(IErrorOr).IsAssignableFrom(typeof(ErrorOr<Guid>)));
        }
    }
}
