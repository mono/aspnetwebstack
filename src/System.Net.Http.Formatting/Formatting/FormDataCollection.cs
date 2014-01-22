﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Formatting.Internal;
using System.Net.Http.Formatting.Parsers;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Represent the form data.
    /// - This has 100% fidelity (including ordering, which is important for deserializing ordered array). 
    /// - using interfaces allows us to optimize the implementation. E.g., we can avoid eagerly string-splitting a 10gb file. 
    /// - This also provides a convenient place to put extension methods. 
    /// </summary>
    public class FormDataCollection : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _pairs;
        private NameValueCollection _nameValueCollection;

        /// <summary>
        /// Initialize a form collection around incoming data. 
        /// The key value enumeration should be immutable. 
        /// </summary>
        /// <param name="pairs">incoming set of key value pairs. Ordering is preserved.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is the convention for representing FormData")]
        public FormDataCollection(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            if (pairs == null)
            {
                throw Error.ArgumentNull("pairs");
            }
            _pairs = pairs;
        }

        /// <summary>
        /// Initialize a form collection from a query string. 
        /// Uri and FormURl body have the same schema. 
        /// </summary>
        public FormDataCollection(Uri uri)
            : this(ParseUri(uri))
        {
        }

        /// <summary>
        /// Initialize a form collection from a URL encoded query string.
        /// Any leading question mark (?) will be considered part of the query string and treated as any other value.
        /// </summary>        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "string is a query string, not a URI")]
        public FormDataCollection(string query)
            : this(ParseQueryString(query))
        {
        }
        
        // Helper to invoke parser around a uri
        private static IEnumerable<KeyValuePair<string, string>> ParseUri(Uri uri)
        {
            if (uri == null)
            {
                throw Error.ArgumentNull("uri");
            }
            
            string query = uri.Query;
            if (!String.IsNullOrEmpty(query) && query[0] == '?')
            {
                query = query.Substring(1);
            }
            return ParseQueryString(query);
        }

        // Helper to invoke parser around a query string
        private static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            byte[] bytes = Encoding.UTF8.GetBytes(query);

            FormUrlEncodedParser parser = new FormUrlEncodedParser(result, Int64.MaxValue);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(bytes, bytes.Length, ref bytesConsumed, isFinal: true);

            if (state != ParserState.Done)
            {
                throw Error.InvalidOperation(Properties.Resources.FormUrlEncodedParseError, bytesConsumed);
            }

            return result;
        }

        /// <summary>
        /// Get the collection as a NameValueCollection.
        /// Beware this loses some ordering. Values are ordered within a key,
        /// but keys are no longer ordered against each other.         
        /// </summary>
        public NameValueCollection ReadAsNameValueCollection()
        {
            if (_nameValueCollection == null)
            {
                // Initialize in a private collection to be thread-safe, and swap the finished object.
                // Ok to double initialize this. 
                NameValueCollection newCollection = HttpValueCollection.Create(this);
                Interlocked.Exchange(ref _nameValueCollection, newCollection);
            }
            return _nameValueCollection;
        }

        /// <summary>
        /// Get values associated with a given key. If there are multiple values, they're concatenated. 
        /// </summary>
        public string Get(string key)
        {
            return ReadAsNameValueCollection().Get(key);
        }

        /// <summary>
        /// Get a value associated with a given key. 
        /// </summary>
        public string[] GetValues(string key)
        {
            return ReadAsNameValueCollection().GetValues(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _pairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable ie = _pairs;
            return ie.GetEnumerator();
        }
    }
}
