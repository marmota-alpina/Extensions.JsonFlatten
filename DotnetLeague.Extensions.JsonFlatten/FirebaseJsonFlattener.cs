using System.Text.Json;
using System.Text.Json.Nodes;

namespace DotnetLeague.Extensions.JsonFlatten;

/// <summary>
/// Provides utility methods to flatten JSON objects into a dictionary with a key structure
/// similar to Firebase Realtime Database paths. This is particularly useful for performing
/// atomic multi-path updates in Firebase.
/// </summary>
public static class FirebaseJsonFlattener
{
    /// <summary>
    /// Flattens a given object into a dictionary using default options.
    /// Null and empty string values are included, and the root prefix is "/".
    /// </summary>
    /// <param name="data">The object to flatten.</param>
    /// <returns>A dictionary representing the flattened object.</returns>
    public static Dictionary<string, object?> Flatten(object data)
        => Flatten(data, true, "/");

    /// <summary>
    /// Flattens a given object into a dictionary, specifying a custom root prefix.
    /// Null and empty string values are included by default.
    /// </summary>
    /// <param name="data">The object to flatten.</param>
    /// <param name="root">The root prefix to prepend to all keys (e.g., "/my_collection").</param>
    /// <returns>A dictionary representing the flattened object.</returns>
    public static Dictionary<string, object?> Flatten(object data, string root)
        => Flatten(data, true, root);

    /// <summary>
    /// Flattens a given object into a dictionary, controlling whether null and empty values are included.
    /// The root prefix defaults to "/".
    /// </summary>
    /// <param name="data">The object to flatten.</param>
    /// <param name="includeNullAndEmptyValues">
    /// If set to <c>true</c>, null values and empty/whitespace strings will be included in the flattened dictionary.
    /// Otherwise, they will be omitted.
    /// </param>
    /// <returns>A dictionary representing the flattened object.</returns>
    public static Dictionary<string, object?> Flatten(object data, bool includeNullAndEmptyValues)
        => Flatten(data, includeNullAndEmptyValues, "/");

    /// <summary>
    /// Flattens a given object into a dictionary, with full control over inclusion of null/empty values and the root prefix.
    /// </summary>
    /// <param name="data">The object to flatten. This object will be serialized to JSON and then flattened.</param>
    /// <param name="includeNullAndEmptyValues">
    /// If set to <c>true</c>, null values and empty/whitespace strings will be included in the flattened dictionary.
    /// Otherwise, they will be omitted.
    /// </param>
    /// <param name="root">The root prefix to prepend to all keys (e.g., "/my_collection"). If the data itself is null
    /// and <paramref name="includeNullAndEmptyValues"/> is <c>true</c>, this root will be added as a key with a null value.
    /// </param>
    /// <returns>A dictionary where keys are Firebase-like paths (e.g., "/field/subfield") and values are the corresponding data.</returns>
    /// <remarks>
    /// This method is particularly useful for preparing data for Firebase Realtime Database's
    /// multi-path update operations, which expect a dictionary of paths to values.
    /// </remarks>
    public static Dictionary<string, object?> Flatten(object data, bool includeNullAndEmptyValues, string root)
    {
        var result = new Dictionary<string, object?>();
        JsonNode? node = JsonSerializer.SerializeToNode(data);

        // If the entire input 'data' is null, SerializeToNode will return null.
        // In this case, if we're configured to include nulls, we add the root itself as null.
        if (node is null)
        {
            if (includeNullAndEmptyValues)
            {
                result[root.TrimEnd('/')] = null;
            }
            return result;
        }

        // Ensure the root prefix does not end with a slash for consistent path generation.
        root = root.TrimEnd('/');

        FlattenNode(node, root, result, includeNullAndEmptyValues);
        return result;
    }

    /// <summary>
    /// Recursively flattens a JsonNode into the result dictionary.
    /// </summary>
    /// <param name="node">The current JsonNode being processed (can be an object, array, or value).</param>
    /// <param name="prefix">The current path prefix for the node (e.g., "/users/123").</param>
    /// <param name="result">The dictionary to accumulate the flattened key-value pairs.</param>
    /// <param name="includeNullAndEmptyValues">Determines if null values and empty/whitespace strings should be included.</param>
    private static void FlattenNode(JsonNode node, string prefix, Dictionary<string, object?> result, bool includeNullAndEmptyValues)
    {
        switch (node)
        {
            case JsonObject obj:
                // Iterate through properties of a JSON object.
                foreach (KeyValuePair<string, JsonNode?> kvp in obj)
                {
                    // Construct the new path for the current property.
                    string newPrefix = $"{prefix}/{kvp.Key}";
                    if (kvp.Value == null)
                    {
                        // If the property's value is null, add it if allowed.
                        if (includeNullAndEmptyValues)
                        {
                            result[newPrefix] = null;
                        }
                    }
                    else
                    {
                        // Recursively flatten the property's value.
                        FlattenNode(kvp.Value, newPrefix, result, includeNullAndEmptyValues);
                    }
                }
                break;

            case JsonArray array:
                // Iterate through elements of a JSON array.
                for (int i = 0; i < array.Count; i++)
                {
                    // Construct the new path for the current array element (using index as segment).
                    string newPrefix = $"{prefix}/{i}";
                    if (array[i] == null)
                    {
                        // If the array element is null, add it if allowed.
                        if (includeNullAndEmptyValues)
                        {
                            result[newPrefix] = null;
                        }
                    }
                    else
                    {
                        // Recursively flatten the array element.
                        FlattenNode(array[i]!, newPrefix, result, includeNullAndEmptyValues);
                    }
                }
                break;

            case JsonValue valueNode:
                // Handle primitive JSON values (string, number, boolean, null).
                object? value = valueNode.GetValue<object?>();

                if (!includeNullAndEmptyValues)
                {
                    // If not including null/empty values, check and return early.
                    if (value == null || value is string s && string.IsNullOrWhiteSpace(s))
                    {
                        return;
                    }
                }

                // Add the flattened value to the result dictionary.
                result[prefix] = value;
                break;
        }
    }
}
