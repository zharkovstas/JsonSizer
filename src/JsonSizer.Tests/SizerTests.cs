using System.Text;

using That;

namespace JsonSizer.Tests;

public class SizerTests
{
    public void Size_GivenNoData_ReturnsEmpty()
    {
        var result = Sizer.Size([]);

        Assert.That(result.Count == 0, $"result.Count; Expected: 0; But was: {result.Count}");
    }

    public void Size_SizesCorrectly()
    {
        Parallel.ForEach(new (string Json, string Expected)[]
        {
            ("{}", "@root=2"),
            ("{ }", "@root=3"),
            ("{\n}", "@root=3"),
            ("{\r\n}", "@root=4"),
            (" {} ", "@root=4"),
            ("[]", "@root=2"),
            ("[0]", "@root=2 @root;[i]=1"),
            (@"""""", "@root=2"),
            (@"""test""", "@root=6"),
            (@"""❤""", "@root=5"),
            (@"{""a"":""b""}", "@root=2 @root;a=7"),
            (@"{""a"":""b""  }   ", "@root=7 @root;a=7"),
            (@"{ ""a"": ""b""}", "@root=2 @root;a=9"),
            (@"{ ""a"": {  ""b"": ""c"" }  }", "@root=4 @root;a=9 @root;a;b=10"),
            (@"{""a"": ""b"", ""c"": ""d""}", "@root=2 @root;a=8 @root;c=10"),
            (@"[[], [[[]], []]]", "@root=2 @root;[i]=6 @root;[i];[i]=6 @root;[i];[i];[i]=2"),

            // invalid JSON
            (@" ", "@broken=1"),
            (@" x ", "@broken=3"),
            ("{", "@root=1"),
            ("{  ", "@root=3"),
            ("{x", "@broken=1 @root=1"),
        },
        x =>
        {
            var (json, expectedString) = x;

            var jsonBytes = Encoding.UTF8.GetBytes(json);

            var actual = Sizer.Size(jsonBytes);

            var actualBytes = actual.Sum(x => x.Value);

            Assert.That(
                actualBytes == jsonBytes.Length,
                $"{json}, actualBytes; Expected: {jsonBytes.Length}; But was: {actualBytes}");

            var actualString = string.Join(
                " ",
                actual.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));

            Assert.That(
                actualString == expectedString,
                $"{json}; Expected: {expectedString}; But was: {actualString}");
        });
    }

    public void Size_SizesExampleCorrectly()
    {
        const string json = @"{
  ""first_name"": ""John"",
  ""last_name"": ""Smith"",
  ""is_alive"": true,
  ""age"": 27,
  ""address"": {
    ""street_address"": ""21 2nd Street"",
    ""city"": ""New York"",
    ""state"": ""NY"",
    ""postal_code"": ""10021-3100""
  },
  ""phone_numbers"": [
    {
      ""type"": ""home"",
      ""number"": ""212 555-1234""
    },
    {
      ""type"": ""office"",
      ""number"": ""646 555-4567""
    }
  ],
  ""children"": [
    ""Catherine"",
    ""Thomas"",
    ""Trevor""
  ],
  ""spouse"": null
}";

        var jsonBytes = Encoding.UTF8.GetBytes(json);

        var actual = Sizer.Size(jsonBytes);

        var actualBytes = actual.Sum(x => x.Value);

        var expected = new Dictionary<string, long>
        {
            ["@root"] = 4,
            ["@root;address"] = 22,
            ["@root;address;city"] = 25,
            ["@root;address;postal_code"] = 34,
            ["@root;address;state"] = 20,
            ["@root;address;street_address"] = 39,
            ["@root;age"] = 14,
            ["@root;children"] = 23,
            ["@root;children;[i]"] = 47,
            ["@root;first_name"] = 24,
            ["@root;is_alive"] = 21,
            ["@root;last_name"] = 25,
            ["@root;phone_numbers"] = 28,
            ["@root;phone_numbers;[i]"] = 29,
            ["@root;phone_numbers;[i];number"] = 66,
            ["@root;phone_numbers;[i];type"] = 46,
            ["@root;spouse"] = 19
        };

        Parallel.ForEach(
            actual.Keys,
            key =>
            {
                Assert.That(expected.ContainsKey(key), $"{key} is extra");
                Assert.That(actual[key] == expected[key], $"{key}; Expected: {expected[key]}; But was: {actual[key]}");
                expected.Remove(key);
            });

        Assert.That(expected.Count == 0, $"Missing: {string.Join(", ", expected.Keys)}");
    }
}