using System;

using System.Data;
using System.Linq;
using UtilityHelper.NonGeneric;
using AutoFixture;
using System.Collections;
using UtilityHelper.Generic;
using Xunit;

namespace UtilityHelper.Test
{
    public class UnitTest1
    {
        [Fact]
        public void TestTypeConversion()
        {
            var type = typeof(TimeSpanHelper);

            var stringArray = TypeHelper.AsString(type);
            var newType = TypeHelper.ToType(stringArray[0], stringArray[1], stringArray[2]);

            Assert.Equal(type, newType);
        }
              
        
        [Fact]
        public void TestMethod1()
        {
            var data = TestMethodCsv();
            var filtered = data.FilterIndex(new[] { ((string)"statecode") }, new[] { (IConvertible)StateCode.FL });

            Assert.Equal(data.Count(), filtered.Count());
        }

        [Fact]
        public void TestMethod2()
        {
            var data = TestMethodCsv();
            var filtered = data.FilterIndex(new[] { ((string)"construction") }, new[] { "Wood" });

            //Assert.AreEqual(data.Count(), filtered.Count());
        }

        [Fact]
        public void TestMethod3()
        {
            var data = TestMethodCsv();
            var filtered = data.FilterWithNull(new System.Collections.Generic.KeyValuePair<string, IConvertible>("construction", "Wood"));

            //Assert.AreEqual(data.Count(), filtered.Count());
        }

        [Fact]
        public void TestMethod4()
        {
            var data = TestMethodCsvDataTable();
            var filtered = data.Rows.FilterWithNull(new System.Collections.Generic.KeyValuePair<string, IConvertible>("construction", "Wood"));

        }

        [Fact]
        FL_insurance_sample[] TestMethodCsv()
        {
            return CsvHelper.ReadFromCsv<FL_insurance_sample>("../../Data/FL_insurance_sample.csv").ToArray();

            //var filtered = data.FilterIndex(new[] { ((string)filter) }, new[] { (IConvertible)filteron });
        }
        [Fact]
        DataTable TestMethodCsvDataTable()
        {
            var xx = TestMethodCsv();
            var dt = xx.ToDataTable();
            return dt;
            //var filtered = data.FilterIndex(new[] { ((string)filter) }, new[] { (IConvertible)filteron });
        }


        [Fact]
        public void TestMethodToCsvString()
        {
            var fixture = new AutoFixture.Fixture();
            var xx = UtilityHelper.CsvHelper.ToCSVString(fixture.CreateMany<FL_insurance_sample>(10));
            var tempfn = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempfn, xx);
            var xc = CsvHelper.ReadFromCsv<FL_insurance_sample>(tempfn).ToArray();
        }

        [Fact]
        public void TestMethodToCsvString2()
        {
            var fixture = new AutoFixture.Fixture();
            var xx = UtilityHelper.CsvHelper.ToCSVString(fixture.CreateMany<FL_insurance_sample>(10) as IEnumerable);
            var tempfn = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempfn, xx);
            var xc = CsvHelper.ReadFromCsv<FL_insurance_sample>(tempfn).ToArray();

        }

        [Fact]
        public void TestMethod_ToMultiDimensionalArray()
        {
            var dowArray = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select(a => a.ToString()).ToArray();
            var numbers = Enumerable.Range(0, 10).Select(a => a.ToString()).ToArray();
            var rand = new Random();

            var dict = Enumerable.Range(0, 30)
                            .Select(a => ((dowArray[rand.Next(0, dowArray.Length)], numbers[rand.Next(0, numbers.Length)]), rand.NextDouble()))
                            .GroupBy(a => a.Item1)
                            .Select(a => (a.Key, a.First().Item2))
                            .ToDictionary(a => a.Key, a => a.Item2);
            var arr = dict.ToMultiDimensionalArray();

        }


        [Fact]
        public void Test_JoinByName()
        {

            var joins = EnumHelper.JoinByName<StateCode, StateCodeBad>().ToArray();

            Assert.Equal(10, joins.Length);
        }

        [Fact]
        public void Test_MatchByName()
        {

            var match = EnumHelper.MatchByName<StateCodeBad>(StateCode.FL);

            Assert.Equal(StateCodeBad.fl_, match);
        }


        [Fact]
        public void Test_WeeksOfYear()
        {

            var match = DateTimeHelper.GetWeeksOfYear(2014).ToArray();

            // Assert.AreEqual(match, StateCode2.fl_);
        }

        [Fact]
        public void EnumHelper_GetAllValuesAndDescriptions()
        {

            var match = EnumHelper.SelectAllValuesAndDescriptions<StateCode>().ToArray(); //.GetWeeksOfYear(2014).ToArray();

            // Assert.AreEqual(match, StateCode2.fl_);
        }

        [Fact]
        public void Test_That_Guid_Int_Conversion_Goes_Both_Ways()
        {

            var match = GuidHelper.ToGuid(100);
            var hundred = GuidHelper.ToInt(match);

            Assert.Equal(100, hundred);
        }

        [Fact]
        public void Test_That_Guid_String_Conversion_Goes_Both_Ways()
        {
            var match = GuidHelper.ToGuid("100");
            var hundred = GuidHelper.ToString(match);

            Assert.Equal("100", hundred);
        }

        [Fact]
        public void Test_That_Guid_DateTime_Conversion_Goes_Both_Ways()
        {
            var date = new DateTime(2000, 2, 1, 4, 7, 2);
            var match = GuidHelper.ToGuid(date);
            var guidDate = GuidHelper.ToDateTime(match);

            Assert.Equal(guidDate, date);
        }


        [Fact]
        public void Test_Merge()
        {
            var guid = Guid.NewGuid();
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            var match = GuidHelper.Merge(guid, guid1, guid2, guid3);

            var match2 = GuidHelper.Merge(guid, guid1, guid2, guid3);

            Assert.Equal(match, match2);
        }


        //[Fact]
        //public void Test_That_GroupBy_Works()
        //{
        //    var many = new Fixture().CreateMany<Sample>(100);

        //    var groups = many.OrderBy(a => a.timestamp).GroupBy(TimeSpan.FromDays(365), a => a.timestamp).Select(a => a.Average(b => b.value)).ToArray();

        //}

        [Fact]
        public void Test_AssemblyMethod()
        {
            var ass = new AssemblyRepository(typeof(UnitTest1).Assembly);
            var d = ass.DependentAssemblyList;
            var l = ass.MissingAssemblyList;
        }

        [Fact]
        public void Test_AssemblyMethod_2()
        {
            var ass = new AssemblyRepository(typeof(TypeHelper).Assembly);
            var d = ass.DependentAssemblyList;
            var l = ass.MissingAssemblyList;
        }

        
        [Fact]
        public void Test_WhereNotNull()
        {
            //var ass = Observable.Return(true);
        }


        public class Sample
        {
            public DateTime timestamp;
            public double value;
        }

    }




}
