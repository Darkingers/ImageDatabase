using System.Collections.Concurrent;

namespace Database
{
    public class Database<TData, TID>
        where TID : IComparable<TID>
        where TData : new()
    {
        private ConcurrentBag<Entry<TData, TID>> _entries;

        public Database()
        {
            _entries = new ConcurrentBag<Entry<TData, TID>>();
        }

        public void AddEntry(Entry<TData, TID> entry)
        {
            _entries.Add(entry);
        }

        public void RemoveAll(IEnumerable<Tag<TID>> tags)
        {
            IEnumerable<Entry<TData, TID>> removed = _entries
                .AsParallel()
                .Where(e => e.IsMatch(tags));

            foreach (var entry in removed)
            {
                _entries.TryTake(out _);
            }
        }

        public IEnumerable<Entry<TData, TID>> FindEntries(IEnumerable<Tag<TID>> tags)
        {
            return _entries
                .AsParallel()
                .Where(entry => entry.IsMatch(tags));
        }

        public void Run(Action<TData> action)
        {
            Parallel.ForEach(_entries, entry => action(entry.Data));
        }

        public void Save(string path)
        {
            var lines = _entries.WriteEntries();

            File.WriteAllLines(path, lines);
        }

        public void Load(string path, TagProvider<TID> tagProvider)
        {
            string[] lines = File.ReadAllLines(path);

            var entries = lines.ParseEntries<TData, TID>(tagProvider);

            _entries = new ConcurrentBag<Entry<TData, TID>>(entries);
        }
    }
}
