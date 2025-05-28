namespace DotnetLeague.Extensions.JsonFlatten.UnitTest;

public class FirebaseJsonFlattenerTests
{
    [Fact]
    public void Flatten_SimpleObject_ShouldReturnFlatDictionary()
    {
        var obj = new
        {
            name = "John",
            age = 30
        };

        Dictionary<string, object?> result = FirebaseJsonFlattener.Flatten(obj);

        Assert.Equal(2, result.Count);
        Assert.Equal("John", result["/name"]?.ToString());
        Assert.Equal("30", result["/age"]?.ToString());
    }

    [Theory]
    [InlineData("/usuarios")]
    [InlineData("/users")]
    public void Flatten_NestedObjectAndArray_ShouldUseSlashNotation(string root)
    {
        var obj = new
        {
            user = new
            {
                name = "Alice",
                tags = new[] { "admin", "editor" }
            }
        };

        Dictionary<string, object?> result = FirebaseJsonFlattener.Flatten(obj, root: root);

        Assert.Equal("Alice", result[$"{root}/user/name"]?.ToString());
        Assert.Equal("admin", result[$"{root}/user/tags/0"]?.ToString());
        Assert.Equal("editor", result[$"{root}/user/tags/1"]?.ToString());
    }

    [Fact]
    public void Flatten_WithNullValue_ShouldIncludeWhenEnabled()
    {
        var obj = new
        {
            name = "Maria",
            info = (object?)null
        };

        Dictionary<string, object?> result = FirebaseJsonFlattener.Flatten(obj, includeNullAndEmptyValues: true);

        Assert.True(result.ContainsKey("/info"));
        Assert.Null(result["/info"]);
    }

    [Fact]
    public void Flatten_WithNullValue_ShouldIgnoreWhenDisabled()
    {
        var obj = new
        {
            name = "Carlos",
            data = (object?)null
        };

        Dictionary<string, object?> result = FirebaseJsonFlattener.Flatten(obj, includeNullAndEmptyValues: false);

        Assert.True(result.ContainsKey("/name"));
        Assert.False(result.ContainsKey("/data"));
    }

    [Fact]
    public void Flatten_ArrayRoot_ShouldHandleCorrectly()
    {
        var data = new[]
        {
            new { name = "Item1" },
            new { name = "Item2" }
        };

        Dictionary<string, object?> result = FirebaseJsonFlattener.Flatten(data);

        Assert.Equal("Item1", result["/0/name"]?.ToString());
        Assert.Equal("Item2", result["/1/name"]?.ToString());
    }
}
