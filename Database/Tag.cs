using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Database
{
    public class Tag<T> : IComparable<Tag<T>>
        where T : IComparable<T>
    {
        public T ID { get; internal set; }

        public Tag(T id)
        {
            ID = id;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Tag<T> tag = obj as Tag<T>;
            if (tag == null)
            {
                return false;
            }

            return ID.Equals(tag.ID);
        }

        public override string ToString()
        {
            return ID.ToString();
        }

        public int CompareTo(Tag<T> other)
        {
            //Null check
            if (other == null)
            {
                return 1;
            }

            return ID.CompareTo(other.ID);
        }
    }

    public static class TagUtils
    {
        public static void ParseTags<T>(this IEnumerable<string> lines, out ConcurrentDictionary<T, List<T>> aliasMap)
        {
            ConcurrentDictionary<T, List<T>> map = new ConcurrentDictionary<T, List<T>>();

            lines.AsParallel()
                 .ForAll(line =>
                 {
                     line.ParseTag(out T id, out var aliases);
                     map[id] = aliases;
                 });

            aliasMap = map;
        }

        public static void ParseTag<T>(this string line, out T id, out List<T> alieases)
        {
            string[] parts = line.Split(Constants.EntrySeparator);

            id = parts[0].ParseID<T>();

            alieases = parts[1]
                .Split(Constants.TagSeparator)
                .Select(ParseID<T>)
                .ToList();
        }

        public static T ParseID<T>(this string id)
        {
            return JsonConvert.DeserializeObject<T>(id);
        }
        public static IEnumerable<string> WriteTags<T>(this IEnumerable<Tag<T>> tags, TagProvider<T> provider)
             where T : IComparable<T>
        {
            return tags
                .AsParallel()
                .Select(x => x.WriteTag(provider));
        }

        public static string WriteTag<T>(this Tag<T> tag, TagProvider<T> provider)
            where T : IComparable<T>
        {
            var aliases = provider
                .GetAliases(tag)
                .Select(x => x.ID.ToString());

            return $"{tag.ID}{Constants.EntrySeparator}{string.Join(Constants.TagSeparator, aliases)}";
        }
    }
}
