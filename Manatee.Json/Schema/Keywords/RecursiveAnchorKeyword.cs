﻿using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema
{
	/// <summary>
	/// Defines the `$recursiveAnchor` JSON Schema keyword.
	/// </summary>
	[DebuggerDisplay("Name={Name} Value={Value}")]
	public class RecursiveAnchorKeyword : IJsonSchemaKeyword, IEquatable<RecursiveAnchorKeyword>
	{
		/// <summary>
		/// Gets the name of the keyword.
		/// </summary>
		public string Name => "$recursiveAnchor";
		/// <summary>
		/// Gets the versions (drafts) of JSON Schema which support this keyword.
		/// </summary>
		public JsonSchemaVersion SupportedVersions => JsonSchemaVersion.Draft2019_09;
		/// <summary>
		/// Gets the a value indicating the sequence in which this keyword will be evaluated.
		/// </summary>
		/// <implementationNotes>
		/// May be duplicated across different keywords.  This property comes into play when there
		/// are several keywords which must be evaluated in a specific order.
		/// </implementationNotes>
		public int ValidationSequence => int.MinValue;
		/// <summary>
		/// Gets the vocabulary that defines this keyword.
		/// </summary>
		public SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

		/// <summary>
		/// The boolean value for this keyword.
		/// </summary>
		public bool Value { get; set; }

		/// <summary>
		/// Used for deserialization.
		/// </summary>
		[DeserializationUseOnly]
		[UsedImplicitly]
		public RecursiveAnchorKeyword() { }
		/// <summary>
		/// Creates a new instance of the <see cref="RecursiveAnchorKeyword"/>.
		/// </summary>
		public RecursiveAnchorKeyword(bool value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides the validation logic for this keyword.
		/// </summary>
		/// <param name="context">The context object.</param>
		/// <returns>Results object containing a final result and any errors that may have been found.</returns>
		public SchemaValidationResults Validate(SchemaValidationContext context)
		{
			if (context.RecursiveAnchor == null)
			{
				Log.Schema(() => $"Marking recursive anchor at {context.RelativeLocation}");
				context.RecursiveAnchor = context.Local;
			}
			
			return SchemaValidationResults.Null;
		}
		/// <summary>
		/// Used register any subschemas during validation.  Enables look-forward compatibility with `$ref` keywords.
		/// </summary>
		/// <param name="baseUri">The current base URI</param>
		/// <param name="localRegistry"></param>
		/// <implementationNotes>
		/// If the keyword does not contain any schemas (e.g. `maximum`), this method is a no-op.
		/// </implementationNotes>
		public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry) { }
		/// <summary>
		/// Resolves any subschemas during resolution of a `$ref` during validation.
		/// </summary>
		/// <param name="pointer">A <see cref="JsonPointer"/> to the target schema.</param>
		/// <param name="baseUri">The current base URI.</param>
		/// <returns>The referenced schema, if it exists; otherwise null.</returns>
		/// <implementationNotes>
		/// If the keyword contains no subschemas, simply return null.
		/// If the keyword contains a subschema, simply pass the call to <see cref="JsonSchema.ResolveSubschema(JsonPointer, Uri)"/>.
		/// </implementationNotes>
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
			Value = json.Boolean;
		}
		/// <summary>
		/// Converts an object to a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		/// <returns>The <see cref="JsonValue"/> representation of the object.</returns>
		public JsonValue ToJson(JsonSerializer serializer)
		{
			return Value;
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RecursiveAnchorKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name);
		}
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(IJsonSchemaKeyword? other)
		{
			return Equals(other as RecursiveAnchorKeyword);
		}
		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object? obj)
		{
			return Equals(obj as RecursiveAnchorKeyword);
		}
		/// <summary>Serves as the default hash function. </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}
