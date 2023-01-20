using System.Collections.Concurrent;

namespace Database
{
    public class TagProvider<T>
        where T : IComparable<T>
    {
        private ConcurrentDictionary<T, Tag<T>> _tags;
        private ConcurrentDictionary<Tag<T>, List<Tag<T>>> _aliases;

        public TagProvider()
        {
            _tags = new ConcurrentDictionary<T, Tag<T>>();
            _aliases = new ConcurrentDictionary<Tag<T>, List<Tag<T>>>();
        }

        public Tag<T> ResolveTag(T id)
        {
            return _tags[id];
        }

        public IEnumerable<Tag<T>> ResolveAll(T id)
        {
            List<Tag<T>> list = new List<Tag<T>>();
            var tag = ResolveTag(id);

            list.Add(tag);

            if (_aliases.ContainsKey(tag))
            {
                list.AddRange(_aliases[tag]);
            }

            return list;
        }

        public IEnumerable<Tag<T>> ResolveAll(IEnumerable<T> id)
        {
            List<Tag<T>> list = new List<Tag<T>>();

            foreach (var i in id)
            {
                list.AddRange(ResolveAll(i));
            }

            return list;
        }

        public IEnumerable<Tag<T>> GetAliases(Tag<T> tag)
        {
            return _aliases[tag];
        }

        public void AddAlias(
            Tag<T> tag,
            Tag<T> alias)
        {
            if (!_aliases.ContainsKey(tag))
            {
                _aliases[tag] = new List<Tag<T>>();
            }

            _aliases[tag].Add(alias);
        }

        public void RemoveAlias(
            Tag<T> tag,
            Tag<T> alias)
        {
            if (_aliases.ContainsKey(tag))
            {
                _aliases[tag].Remove(alias);
            }
        }

        public void AddTag(Tag<T> tag)
        {
            _tags[tag.ID] = tag;
        }

        public void RemoveTag(Tag<T> tag)
        {
            _tags.TryRemove(tag.ID, out _);

            foreach (var aliases in _aliases.Values)
            {
                aliases.Remove(tag);
            }
        }

        public void Add(Tag<T> tag)
        {
            _tags[tag.ID] = tag;
        }

        public void Load(string path)
        {
            string[] lines = File.ReadAllLines(path);

            lines.ParseTags<T>(out var aliasMap);

            ResolveAliases(aliasMap);
        }

        public void Save(string path)
        {
            var lines = _tags.Values.WriteTags(provider: this);

            File.WriteAllLines(path, lines);
        }

        private void ResolveAliases(ConcurrentDictionary<T, List<T>> aliases)
        {
            _tags.Keys
                .AsParallel()
                .ForAll(x =>
                {
                    if (aliases.ContainsKey(x))
                    {
                        _aliases[_tags[x]] = aliases[x]
                            .Select(y => _tags[y])
                            .ToList();
                    }
                });
        }
    }
}
