﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema
{
	/// <summary>
	/// Defines the `required` JSON Schema keyword.
	/// </summary>
	[DebuggerDisplay("Name={Name}; Count={Count}")]
	public class RequiredKeyword : List<string>, IJsonSchemaKeyword, IEquatable<RequiredKeyword>
	{
		/// <summary>
		/// Gets or sets the error message template.
		/// </summary>
		/// <remarks>
		/// Supports the following tokens:
		/// - properties
		/// </remarks>
		public static string ErrorTemplate { get; set; } = "The properties {{properties}} are required.";

		/// <summary>
		/// Gets the name of the keyword.
		/// </summary>
		public string Name => "required";
		/// <summary>
		/// Gets the versions (drafts) of JSON Schema which support this keyword.
		/// </summary>
		public JsonSchemaVersion SupportedVersions => this.Any()
			? JsonSchemaVersion.All
			: JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;
		/// <summary>
		/// Gets the a value indicating the sequence in which this keyword will be evaluated.
		/// </summary>
		public int ValidationSequence => 1;
		/// <summary>
		/// Gets the vocabulary that defines this keyword.
		/// </summary>
		public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

		/// <summary>
		/// Used for deserialization.
		/// </summary>
		[DeserializationUseOnly]
		[UsedImplicitly]
		public RequiredKeyword() { }
		/// <summary>
		/// Creates an instance of the <see cref="RequiredKeyword"/>.
		/// </summary>
		public RequiredKeyword(params string[] values)
			: base(values) { }
		/// <summary>
		/// Creates an instance of the <see cref="RequiredKeyword"/>.
		/// </summary>
		public RequiredKeyword(IEnumerable<string> values)
			: base(values) { }

		/// <summary>
		/// Provides the validation logic for this keyword.
		/// </summary>
		/// <param name="context">The context object.</param>
		/// <returns>Results object containing a final result and any errors that may have been found.</returns>
		public SchemaValidationResults Validate(SchemaValidationContext context)
		{
			var results = new SchemaValidationResults(Name, context);

			if (context.Instance.Type != JsonValueType.Object)
			{
				Log.Schema(() => "Instance not an object; not applicable");
				return results;
			}

			var missingProperties = new List<string>();
			var obj = context.Instance.Object;
			foreach (var propertyName in this)
			{
				if (!obj.ContainsKey(propertyName))
				{
					if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
					{
						results.IsValid = false;
						return results;
					}
					missingProperties.Add(propertyName);
				}
			}

			if (missingProperties.Any())
			{
				Log.Schema(() => $"Properties {missingProperties.ToJson()} required but not found");
				results.IsValid = false;
				results.AdditionalInfo["properties"] = missingProperties.ToJson();
				results.ErrorMessage = ErrorTemplate.ResolveTokens(results.AdditionalInfo);
			}

			return results;
		}
		/// <summary>
		/// Used register any subschemas during validation.  Enables look-forward compatibility with `$ref` keywords.
		/// </summary>
		/// <param name="baseUri">The current base URI</param>
		/// <param name="localRegistry"></param>
		public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry) { }
		/// <summary>
		/// Resolves any subschemas during resolution of a `$ref` during validation.
		/// </summary>
		/// <param name="pointer">A <see cref="JsonPointer"/> to the target schema.</param>
		/// <param name="baseUri">The current base URI.</param>
		/// <returns>The referenced schema, if it exists; otherwise null.</returns>
		public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
		{
			return null;
		}
		/// <summary>
		/// Builds an object from a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="json">The <see cref="JsonValue"/> representation of the object.</param>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		public void FromJson(JsonValue json, JsonSerializer serializer)
		{
			AddRange(json.Array.Select(jv => jv.String));
		}
		/// <summary>
		/// Converts an object to a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		/// <returns>The <see cref="JsonValue"/> representation of the object.</returns>
		public JsonValue ToJson(JsonSerializer serializer)
		{
			return new JsonArray(this.Select(s => new JsonValue(s))){EqualityStandard = ArrayEquality.ContentsEqual};
		}
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RequiredKeyword? other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			return this.ContentsEqual(other);
		}
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(IJsonSchemaKeyword? other)
		{
			return Equals(other as RequiredKeyword);
		}
		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object? obj)
		{
			return Equals(obj as RequiredKeyword);
		}
		/// <summary>Serves as the default hash function. </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.GetCollectionHashCode();
		}
	}
}