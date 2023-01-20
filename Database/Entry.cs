using Newtonsoft.Json;
using Utils;

namespace Database
{
    public class Entry<TData, TID>
        where TData : new()
        where TID : IComparable<TID>
    {
        public ICollection<Tag<TID>> Tags { get; }
        public TData Data { get; }

        public Entry(
            TData data,
            ICollection<Tag<TID>> tags)
        {
            Tags = tags;
            Data = data;
        }

        public bool IsMatch(IEnumerable<Tag<TID>> tags)
        {
            return tags.ContainsAll(Tags);
        }
    }

    public static class EntryUtils
    {
        public static IEnumerable<Entry<TData, TID>> ParseEntries<TData, TID>(this IEnumerable<string> lines, TagProvider<TID> tagProvider)
            where TID : IComparable<TID>
            where TData : new()
        {
            return lines
                .AsParallel()
                .Select(x => x.ParseEntry<TData, TID>(tagProvider));
        }

        private static Entry<TData, TID> ParseEntry<TData, TID>(this string line, TagProvider<TID> tagProvider)
            where TID : IComparable<TID>
            where TData : new()
        {
            string[] parts = line.Split(Constants.EntrySeparator);

            List<Tag<TID>> tags = parts[0]
                .Split(Constants.TagSeparator)
                .Select(x => JsonConvert.DeserializeObject<TID>(x))
                .Select(x => tagProvider.ResolveTag(x))
                .ToList();

            var data = JsonConvert.DeserializeObject<TData>(parts[1]);

            return new Entry<TData, TID>(data, tags);
        }

        public static IEnumerable<string> WriteEntries<TData, TID>(this IEnumerable<Entry<TData, TID>> entries)
          where TID : IComparable<TID>
          where TData : new()
        {
            return entries
                .AsParallel()
                .Select(WriteEntry<TData, TID>);
        }

        public static string WriteEntry<TData, TID>(this Entry<TData, TID> entry)
            where TID : IComparable<TID>
            where TData : new()
        {
            string tags = string.Join(Constants.TagSeparator, entry.Tags.Select(x => x.ID));
            string data = JsonConvert.SerializeObject(entry.Data);

            return $"{tags}{Constants.EntrySeparator}{data}";
        }

    }
}
