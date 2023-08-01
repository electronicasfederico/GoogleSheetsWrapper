using GoogleSheetsWrapper.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GoogleSheetsWrapper.Tests
{
    public class BaseRepositoryTests
    {
        public string number = "";
        public string serialformatDate;

        [SetUp]
        public void Setup()
        {
            number = "";
            if (CultureInfo.CurrentCulture.Name == "en-US")
            {
                number = "1,234.56";
            }
            else
            {
                number = "1.234,56";
            }

            if (CultureInfo.CurrentCulture.Name == "en-US")
            {
                serialformatDate = "33.625";
            }
            else
            {
                serialformatDate = "33,625";
            }


        }


        [Test]
        public void VerifySchemaMatches()
        {
            List<object> sampleHeader = new List<object>()
            {
                "Name",
                "Number",
                "Price Amount",
                "Date",
                "Quantity"
            };

            var sheetsHelper = new SheetHelper<TestRecord>("", "", "");
            var repo = new TestRepository(sheetsHelper);

            var result = repo.ValidateSchema(sampleHeader);

            Assert.AreEqual(true, result.IsValid);
        }

        [Test]
        public void VerifySchemaDoesNotMatch()
        {
            List<object> sampleHeader = new List<object>()
            {
                "Name",
                "Price Amount"
            };

            var sheetsHelper = new SheetHelper<TestRecord>("", "", "");
            var repo = new TestRepository(sheetsHelper);

            var result = repo.ValidateSchema(sampleHeader);

            Assert.AreEqual(false, result.IsValid);
        }

        [Test]
        public void Test_Record_Creation()
        {
            

            List<object> row = new List<object>()
            {
                "Steve",
                "+1(703)-999-2222",
                "$ 100.00",
                serialformatDate,  // DateTime in serial format for the date time of February 1, 1900 at 3:00 PM
                number
            };

            var record = new TestRecord(row, 1);

            Assert.AreEqual("Steve", record.Name);
            Assert.AreEqual(7039992222, record.PhoneNumber);
            Assert.AreEqual(100, record.PriceAmount);
            Assert.AreEqual(1234.56, record.Quantity);

            var dt = new DateTime(1900, 2, 1, 15, 0, 0);
            Assert.AreEqual(dt, record.DateTime);
        }

        [Test]
        public void Test_Record_Creation_Column_Offset()
        {
            List<object> row = new List<object>()
            {
                "Steve",
                "+1(703)-999-2222",
                "$ 100.00",
                serialformatDate,  // DateTime in serial format for the date time of February 1, 1900 at 3:00 PM
                number
            };

            var attributes = SheetFieldAttributeUtils.GetAllSheetFieldAttributes<TestRecordOffset>();

            var minColumnId = attributes.Min(a => a.Key.ColumnID);

            var record = new TestRecordOffset(row, 1, minColumnId);

            Assert.AreEqual("Steve", record.Name);
            Assert.AreEqual(7039992222, record.PhoneNumber);
            Assert.AreEqual(100, record.PriceAmount);
            Assert.AreEqual(1234.56, record.Quantity);

            var dt = new DateTime(1900, 2, 1, 15, 0, 0);
            Assert.AreEqual(dt, record.DateTime);
        }

        [Test]
        public void Test_Record_Creation_Empty_Phone_Number()
        {
            List<object> row = new List<object>()
            {
                "Steve",
                "",
                "$ 100.00",
                serialformatDate,  // DateTime in serial format for the date time of February 1, 1900 at 3:00 PM
                number
            };

            var record = new TestRecord(row, 1);

            Assert.IsNull(record.PhoneNumber);
        }

        [Test]
        public void Test_Record_Creation_Empty_Currency()
        {
            List<object> row = new List<object>()
            {
                "Steve",
                "+1(703)-999-2222",
                "",
                serialformatDate,  // DateTime in serial format for the date time of February 1, 1900 at 3:00 PM
                number
            };

            var record = new TestRecord(row, 1);

            Assert.IsNull(record.PriceAmount);
        }

        [Test]
        public void Test_Record_Creation_Empty_DateTime()
        {
            List<object> row = new List<object>()
            {
                "Steve",
                "+1(703)-999-2222",
                "$ 100.00",
                "",  // DateTime in serial format for the date time of February 1, 1900 at 3:00 PM
                number
            };

            var record = new TestRecord(row, 1);

            Assert.IsNull(record.DateTime);
        }

        [Test]
        public void Test_Record_Creation_Empty_DateTime_Non_Nullable()
        {
            List<object> row = new List<object>()
            {
                "Steve",
                "+1(703)-999-2222",
                "$ 100.00",
                "",  // DateTime in serial format for the date time of February 1, 1900 at 3:00 PM
                number
            };

            var record = new TestRecordNonNullableDateTime(row, 1);

            Assert.AreEqual("Steve", record.Name);
            Assert.AreEqual(7039992222, record.PhoneNumber);
            Assert.AreEqual(100, record.PriceAmount);
            Assert.AreEqual(1234.56, record.Quantity);

            Assert.AreEqual(new DateTime(), record.DateTime);
        }
    }
}