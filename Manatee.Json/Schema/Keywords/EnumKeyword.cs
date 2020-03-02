﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema
{
	/// <summary>
	/// Defines the `enum` JSON Schema keyword.
	/// </summary>
	[DebuggerDisplay("Name={Name}; Count={Count}")]
	public class EnumKeyword : List<JsonValue>, IJsonSchemaKeyword, IEquatable<EnumKeyword>
	{
		/// <summary>
		/// Gets or sets the error message template.
		/// </summary>
		/// <remarks>
		/// Supports the following tokens:
		/// - value
		/// </remarks>
		public static string ErrorTemplate { get; set; } = "{{value}} does not match any of the required values.";

		/// <summary>
		/// Gets the name of the keyword.
		/// </summary>
		public string Name => "enum";
		/// <summary>
		/// Gets the versions (drafts) of JSON Schema which support this keyword.
		/// </summary>
		public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;
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
		public EnumKeyword() { }
		/// <summary>
		/// Creates an instance of the <see cref="EnumKeyword"/>.
		/// </summary>
		public EnumKeyword(params JsonValue[] values)
			: base(values) { }
		/// <summary>
		/// Creates an instance of the <see cref="EnumKeyword"/>.
		/// </summary>
		public EnumKeyword(IEnumerable<JsonValue> values)
			: base(values) { }

		/// <summary>
		/// Provides the validation logic for this keyword.
		/// </summary>
		/// <param name="context">The context object.</param>
		/// <returns>Results object containing a final result and any errors that may have been found.</returns>
		public SchemaValidationResults Validate(SchemaValidationContext context)
		{
			Log.Schema(() => "Checking defined values for instance");
			var results = new SchemaValidationResults(Name, context)
				{
					IsValid = Contains(context.Instance)
				};

			if (!results.IsValid)
			{
				Log.Schema(() => "Instance does not match any of the defined values");
				results.AdditionalInfo["value"] = context.Instance;
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
			AddRange(json.Array);
		}
		/// <summary>
		/// Converts an object to a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		/// <returns>The <see cref="JsonValue"/> representation of the object.</returns>
		public JsonValue ToJson(JsonSerializer serializer)
		{
			return new JsonArray(this) {EqualityStandard = ArrayEquality.ContentsEqual};
		}
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(EnumKeyword? other)
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
			return Equals(other as EnumKeyword);
		}
		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object? obj)
		{
			return Equals(obj as EnumKeyword);
		}
		/// <summary>Serves as the default hash function. </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.GetCollectionHashCode();
		}
	}
}