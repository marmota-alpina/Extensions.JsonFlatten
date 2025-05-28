# DotnetLeague.JsonFlatten

[![NuGet Version](https://img.shields.io/nuget/v/DotnetLeague.JsonFlatten?style=flat-square)](https://www.nuget.org/packages/DotnetLeague.JsonFlatten/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg?style=flat-square)](https://opensource.org/licenses/Apache-2.0)
[![GitHub Repository](https://img.shields.io/badge/GitHub-Repository-brightgreen?style=flat-square&logo=github)](https://github.com/dotnetleague/Extensions.JsonFlatten)

A lightweight and efficient .NET library to flatten complex C# objects and JSON structures into a `Dictionary<string, object?>`. This is especially useful for integrating with NoSQL databases like **Firebase Realtime Database**, enabling **atomic multi-path updates** and simplifying data manipulation.

---

## 🌟 Why Use DotnetLeague.JsonFlatten?

When working with tree-structured databases like Firebase Realtime Database, managing deeply nested JSON can lead to inefficiencies, especially for updates. Firebase's `UpdateAsync` method (and similar operations in other NoSQL DBs) thrives on a flat dictionary of paths to values for atomic operations.

This library bridges that gap, allowing you to:

* **Perform Atomic Multi-Path Updates in Firebase:** Easily transform your rich C# objects into the exact `Dictionary<string, object?>` format required by Firebase for consistent, single-operation updates across various nodes.
* **Improve Data Consistency:** Ensure that related data changes are applied together or not at all, maintaining the integrity of your application's state.
* **Reduce Network Overhead:** Send a single, consolidated update request to the database instead of multiple individual writes, leading to faster operations and less bandwidth consumption.
* **Simplify Complex Data Structures:** Convert intricate nested objects and arrays into a simple, flat representation, making it easier to work with paths.

---

## 🚀 Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package DotnetLeague.JsonFlatten
```

Or via the .NET CLI:

```bash
Install-Package DotnetLeague.JsonFlatten
```

---

## 💡 Quick Start

Let's say you have a complex C# object you want to update in Firebase:

```csharp
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Address HomeAddress { get; set; }
    public List<string> Roles { get; set; }
    public bool? IsActive { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public object? Notes { get; set; } // Can be null
}

// Your C# object instance
var userData = new User
{
    Name = "Alice",
    Age = 30,
    HomeAddress = new Address
    {
        Street = "123 Main St",
        City = "Anytown",
        ZipCode = "12345",
        Notes = null // A null value
    },
    Roles = new List<string> { "admin", "editor" },
    IsActive = true
};
```

Using `FirebaseJsonFlattener`:

```csharp
using DotnetLeague.Extensions.JsonFlatten;
using System.Collections.Generic;

// Flatten the object
Dictionary<string, object?> flattenedData = FirebaseJsonFlattener.Flatten(userData);

// Example of what 'flattenedData' will contain (keys and their corresponding values):
// {
//   "/Name": "Alice",
//   "/Age": 30,
//   "/HomeAddress/Street": "123 Main St",
//   "/HomeAddress/City": "Anytown",
//   "/HomeAddress/ZipCode": "12345",
//   "/HomeAddress/Notes": null,
//   "/Roles/0": "admin",
//   "/Roles/1": "editor",
//   "/IsActive": true
// }

// Now, you can use this dictionary for an atomic update in Firebase (e.g., using FirebaseAdmin SDK):
// await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetDatabase()
//                         .GetReference("users/some_user_id")
//                         .UpdateAsync(flattenedData);
```

---

## 📖 Usage & API

The core functionality is provided by the static `FirebaseJsonFlattener` class.

```csharp
public static class FirebaseJsonFlattener
{
    /// <summary>
    /// Flattens a given object into a dictionary using default options.
    /// Null and empty string values are included, and the root prefix is "/".
    /// </summary>
    /// <param name="data">The object to flatten.</param>
    /// <returns>A dictionary representing the flattened object.</returns>
    public static Dictionary<string, object?> Flatten(object data) { ... }

    /// <summary>
    /// Flattens a given object into a dictionary, specifying a custom root prefix.
    /// Null and empty string values are included by default.
    /// </summary>
    /// <param name="data">The object to flatten.</param>
    /// <param name="root">The root prefix to prepend to all keys (e.g., "/my_collection").</param>
    /// <returns>A dictionary representing the flattened object.</returns>
    public static Dictionary<string, object?> Flatten(object data, string root) { ... }

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
    public static Dictionary<string, object?> Flatten(object data, bool includeNullAndEmptyValues) { ... }

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
    public static Dictionary<string, object?> Flatten(object data, bool includeNullAndEmptyValues, string root) { ... }
}
```

### Examples:

#### 1. Basic Flattening

```csharp
var simpleObj = new { product = "Laptop", price = 1200 };
var result = FirebaseJsonFlattener.Flatten(simpleObj);
// result: { "/product": "Laptop", "/price": 1200 }
```

#### 2. Nested Objects and Arrays

```csharp
var nestedObj = new
{
    order = new
    {
        id = "ORD123",
        items = new[]
        {
            new { name = "Keyboard", qty = 1 },
            new { name = "Mouse", qty = 2 }
        }
    }
};
var result = FirebaseJsonFlattener.Flatten(nestedObj, root: "/transactions");
// result:
// {
//   "/transactions/order/id": "ORD123",
//   "/transactions/order/items/0/name": "Keyboard",
//   "/transactions/order/items/0/qty": 1,
//   "/transactions/order/items/1/name": "Mouse",
//   "/transactions/order/items/1/qty": 2
// }
```

#### 3. Handling Null and Empty Values

```csharp
var dataWithNulls = new
{
    status = "pending",
    notes = (string?)null,
    description = "" // Empty string
};

// Include nulls and empty strings (default behavior)
var resultIncluded = FirebaseJsonFlattener.Flatten(dataWithNulls, includeNullAndEmptyValues: true);
// resultIncluded:
// {
//   "/status": "pending",
//   "/notes": null,
//   "/description": ""
// }

// Exclude nulls and empty strings
var resultExcluded = FirebaseJsonFlattener.Flatten(dataWithNulls, includeNullAndEmptyValues: false);
// resultExcluded:
// {
//   "/status": "pending"
// }
```

#### 4. Flattening a Root Array

```csharp
var rootArray = new[]
{
    new { fruit = "Apple" },
    new { fruit = "Banana" }
};
var result = FirebaseJsonFlattener.Flatten(rootArray, root: "/fruits");
// result:
// {
//   "/fruits/0/fruit": "Apple",
//   "/fruits/1/fruit": "Banana"
// }
```

---

## 🤝 Contributing

Contributions are welcome! If you have ideas for improvements, bug fixes, or new features, please open an issue or submit a pull request on our [GitHub Repository](https://github.com/dotnetleague/Extensions.JsonFlatten).

---

## 📄 License

This project is licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).

---

## 📧 Contact

For any questions or inquiries, feel free to reach out:
[dotnetdevleague@gmail.com](mailto:dotnetdevleague@gmail.com)

---

## ✨ NuGet Profile

Check out our other packages on our [NuGet Profile](https://www.nuget.org/profiles/dotnetdevleague).