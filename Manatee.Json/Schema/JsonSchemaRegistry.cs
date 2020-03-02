﻿using System;
using System.Collections.Concurrent;
using Manatee.Json.Internal;
using Manatee.Json.Patch;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema
{
	/// <summary>
	/// Provides a registry in which JSON schema can be saved to be referenced by the system.
	/// </summary>
	public class JsonSchemaRegistry
	{
		private static readonly ConcurrentDictionary<string, JsonSchema> _schemaLookup;
		private static readonly JsonSerializer _serializer;
		private readonly ConcurrentDictionary<string, JsonSchema> _contextLookup;

		/// <summary>
		/// Initializes the <see cref="JsonSchemaRegistry"/> class.
		/// </summary>
		static JsonSchemaRegistry()
		{
			_schemaLookup = new ConcurrentDictionary<string, JsonSchema>();
			_serializer = new JsonSerializer();
			Clear();
		}
		internal JsonSchemaRegistry()
		{
			_contextLookup = new ConcurrentDictionary<string, JsonSchema>();
		}

		/// <summary>
		/// Downloads and registers a schema at the specified URI.
		/// </summary>
		public static JsonSchema? Get(string uri)
		{
			JsonSchema? schema;
			lock (_schemaLookup)
			{
				uri = uri.TrimEnd('#');
				if (!_schemaLookup.TryGetValue(uri, out schema))
				{
					var schemaJson = JsonSchemaOptions.Download(uri);
					if (schemaJson == null) return null;
				    var schemaValue = JsonValue.Parse(schemaJson);
					schema = new JsonSchema {DocumentPath = new Uri(uri, UriKind.RelativeOrAbsolute)};
					schema.FromJson(schemaValue, _serializer);

					var structureErrors = schema.ValidateSchema();
					if (!structureErrors.IsValid)
						throw new SchemaLoadException("The given path does not contain a valid schema.", structureErrors);

					_schemaLookup[uri] = schema;
				}
			}

			return schema;
		}

		internal static JsonSchema? GetWellKnown(string uri)
		{
			lock (_schemaLookup)
			{
				uri = uri.TrimEnd('#');
				_schemaLookup.TryGetValue(uri, out var schema);

				return schema;
			}
		}

		internal JsonSchema? GetLocal(string uri)
		{
			lock (_contextLookup)
			{
				uri = uri.TrimEnd('#');
				_contextLookup.TryGetValue(uri, out var schema);

				return schema;
			}
		}

		/// <summary>
		/// Explicitly registers an existing schema.
		/// </summary>
		/// <remarks>
		/// This generally isn't required since <see cref="JsonSchema"/> will automatically register itself upon validation.
		/// </remarks>
		public static void Register(JsonSchema schema)
		{
			if (schema.DocumentPath == null) return;

			Log.Schema(() => $"Registering \"{schema.DocumentPath.OriginalString}\"");
			lock (_schemaLookup)
			{
				_schemaLookup[schema.DocumentPath.OriginalString] = schema;
			}
		}

		internal void RegisterLocal(JsonSchema schema)
		{
			if (schema.Id != null && schema.Id.IsLocalSchemaId())
			{
				Log.Schema(() => $"Registering \"{schema.Id}\"");
				lock (_contextLookup)
				{
					_contextLookup[schema.Id] = schema;
				}
			}

			var anchor = schema.Get<AnchorKeyword>();
			if (anchor != null)
			{
				var anchorUri = $"{schema.DocumentPath}#{anchor.Value}";
				Log.Schema(() => $"Registering \"{anchorUri}\"");
				lock (_contextLookup)
				{
					_contextLookup[anchorUri] = schema;
				}
			}
		}

		/// <summary>
		/// Removes a schema from the registry.
		/// </summary>
		public static void Unregister(JsonSchema schema)
		{
			if (schema.DocumentPath == null) return;
			lock (_schemaLookup)
			{
				_schemaLookup.TryRemove(schema.DocumentPath.OriginalString, out _);
			}
		}

		/// <summary>
		/// Removes a schema from the registry.
		/// </summary>
		public static void Unregister(string uri)
		{
			if (string.IsNullOrWhiteSpace(uri)) return;
			lock (_schemaLookup)
			{
				_schemaLookup.TryRemove(uri, out _);
			}
		}

		/// <summary>
		/// Clears the registry of all types.
		/// </summary>
		public static void Clear()
		{
			var draft04Uri = MetaSchemas.Draft04.Id!.Split('#')[0];
			var draft06Uri = MetaSchemas.Draft06.Id!.Split('#')[0];
			var draft07Uri = MetaSchemas.Draft07.Id!.Split('#')[0];
			var draft2019_09Uri = MetaSchemas.Draft2019_09.Id!.Split('#')[0];
			var patchUri = JsonPatch.Schema.Id!.Split('#')[0];
			lock (_schemaLookup)
			{
				_schemaLookup.Clear();
				_schemaLookup[draft04Uri] = MetaSchemas.Draft04;
				_schemaLookup[draft06Uri] = MetaSchemas.Draft06;
				_schemaLookup[draft07Uri] = MetaSchemas.Draft07;
				_schemaLookup[draft2019_09Uri] = MetaSchemas.Draft2019_09;
				_schemaLookup[MetaSchemas.Draft2019_09_Core.Id!] = MetaSchemas.Draft2019_09_Core;
				_schemaLookup[MetaSchemas.Draft2019_09_MetaData.Id!] = MetaSchemas.Draft2019_09_MetaData;
				_schemaLookup[MetaSchemas.Draft2019_09_Applicator.Id!] = MetaSchemas.Draft2019_09_Applicator;
				_schemaLookup[MetaSchemas.Draft2019_09_Validation.Id!] = MetaSchemas.Draft2019_09_Validation;
				_schemaLookup[MetaSchemas.Draft2019_09_Format.Id!] = MetaSchemas.Draft2019_09_Format;
				_schemaLookup[MetaSchemas.Draft2019_09_Content.Id!] = MetaSchemas.Draft2019_09_Content;
				_schemaLookup[patchUri] = JsonPatch.Schema;
			}
		}
	}
}
