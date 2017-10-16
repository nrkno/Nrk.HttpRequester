using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Nrk.HttpRequester
{
    /// <summary>
    /// Builder for User-Agent header
    /// </summary>
    public class UserAgent
    {
        private readonly List<ProductPart> _parts = new List<ProductPart>();

        /// <summary>
        /// Creates a new <code>UserAgent</code> with
        /// product, e.g.
        /// MyApplication
        /// </summary>
        /// <param name="product">e.g. MyApplication</param>
        public UserAgent(string product)
        {
            Add(product);
        }

        /// <summary>
        /// Creates a new <code>UserAgent</code> with
        /// product/version e.g.
        /// MyApplication/1.0.0
        /// </summary>
        /// <param name="product">e.g. MyApplication</param>
        /// <param name="version">e.g. 1.0.0</param>
        public UserAgent(string product, string version)
        {
            Add(product, version);
        }


        /// <summary>
        /// Creates a new <code>UserAgent</code> with
        /// product/version (comment), e.g.
        /// MyApplication/1.0.0 (This is a comment)
        /// </summary>
        /// <param name="product">e.g. MyApplication</param>
        /// <param name="version">e.g. 1.0.0</param>
        /// <param name="comment">e.g. This is a comment</param>
        public UserAgent(string product, string version, string comment)
        {
            Add(product, version, comment);
        }

        /// <summary>
        /// Adds product
        /// </summary>
        /// <param name="product">e.g. MyApplication</param>
        /// <returns>this UserAgent</returns>
        public UserAgent Add(string product)
        {
            return Add(new ProductPart
            {
                Product = product
            });
        }


        /// <summary>
        /// Adds product/version, 
        /// </summary>
        /// <param name="product">e.g. MyApplication</param>
        /// <param name="version">e.g. 1.0.0</param>
        /// <returns>this UserAgent</returns>
        public UserAgent Add(string product, string version)
        {
            return Add(new ProductPart
            {
                Product = product,
                Version = version
            });
        }


        /// <summary>
        /// Adds product/version (comment)
        /// </summary>
        /// <param name="product">e.g. MyApplication</param>
        /// <param name="version">e.g. 1.0.0</param>
        /// <param name="comment">e.g. This is a comment</param>
        /// <returns></returns>
        public UserAgent Add(string product, string version, string comment)
        {
            return Add(new ProductPart
            {
                Product = product,
                Version = version,
                Comment = comment
            });
        }

        private UserAgent Add(ProductPart part)
        {
            _parts.Add(part);
            return this;
        }

        public IEnumerable<ProductInfoHeaderValue> ToProductInfoHeaderValues()
        {
            foreach (var part in _parts)
            {
                if (string.IsNullOrWhiteSpace(part.Version))
                {
                    yield return new ProductInfoHeaderValue(new ProductHeaderValue(part.Product));
                }
                else
                {
                    yield return new ProductInfoHeaderValue(part.Product, part.Version);
                }
                var comment = part.Comment;
                if (!string.IsNullOrWhiteSpace(comment))
                {
                    if (!comment.StartsWith("("))
                    {
                        comment = $"({comment}";
                    }
                    if (!comment.EndsWith(")"))
                    {
                        comment = $"{comment})";
                    }
                    yield return new ProductInfoHeaderValue(comment);
                }
            }
        }
    }
}
