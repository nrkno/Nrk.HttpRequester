using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Nrk.HttpRequester
{
    public class UserAgent
    {
        private readonly List<ProductPart> _parts = new List<ProductPart>();

        /// <summary>
        /// PsApi/1.0.0
        /// </summary>
        /// <param name="product"></param>
        /// <param name="version"></param>
        public UserAgent(string product, string version)
        {
            Add(product, version);
        }


        /// <summary>
        /// PsApi/1.0.0 (This is a comment)
        /// </summary>
        /// <param name="product"></param>
        /// <param name="version"></param>
        /// <param name="comment"></param>
        public UserAgent(string product, string version, string comment)
        {
            Add(product, version, comment);
        }

        /// <summary>
        /// PsApi
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public UserAgent Add(string product)
        {
            return Add(new ProductPart
            {
                Product = product
            });
        }


        /// <summary>
        /// PsApi/1.0.0
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public UserAgent Add(string productName, string version)
        {
            return Add(new ProductPart
            {
                Product = productName,
                Version = version
            });
        }


        /// <summary>
        /// PsApi/1.0.0 (This is a comment)
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="version"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public UserAgent Add(string productName, string version, string comment)
        {
            return Add(new ProductPart
            {
                Product = productName,
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
