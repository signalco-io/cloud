namespace Signal.Api.Dtos
{
    public class LinkDto
    {
        public string Href { get; set; }

        public string Rel { get; set; }

        public string Type { get; set; }

        public LinkDto(string href, string rel, string type)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                throw new global::System.ArgumentException($"'{nameof(href)}' cannot be null or whitespace", nameof(href));
            }

            if (string.IsNullOrWhiteSpace(rel))
            {
                throw new global::System.ArgumentException($"'{nameof(rel)}' cannot be null or whitespace", nameof(rel));
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new global::System.ArgumentException($"'{nameof(type)}' cannot be null or whitespace", nameof(type));
            }

            Href = href;
            Rel = rel;
            Type = type;
        }
    }
}
