using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonTest
{
    [TestClass]
    public class TypeConverterTest
    {
        [TestMethod]
        public void TestTimeSpan()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TimeSpan));

            Assert.IsNotNull(converter);

            Assert.IsTrue(converter.CanConvertFrom(typeof(string)));

            TimeSpan time = (TimeSpan)converter.ConvertFromString("0:15:0");

            Assert.AreEqual(time, TimeSpan.FromMinutes(15));
        }

        [TestMethod]
        public void TestCustomerTypeConvert()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(A));

            Assert.IsNotNull(converter);
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
        }


        private sealed class A { }
    }
}
