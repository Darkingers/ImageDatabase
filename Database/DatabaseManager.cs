namespace Database
{
    public class DatabaseManager<TData, TID>
        where TData : new()
        where TID : IComparable<TID>
    {
        private Database<TData, TID> _database;
        private TagProvider<TID> _tagProvider;

        private string _databasePath;
        private string _tagPath;

        public DatabaseManager(string databasePath, string tagPath)
        {
            _database = new Database<TData, TID>();
            _tagProvider = new TagProvider<TID>();

            _databasePath = databasePath;
            _tagPath = tagPath;
        }

        public IEnumerable<Entry<TData, TID>> Query(IEnumerable<TID> ids)
        {
            return _database.FindEntries(_tagProvider.ResolveAll(ids));
        }

        public void Add(TData data, IEnumerable<TID> ids)
        {
            List<Tag<TID>> tags = ids
                .Select(_tagProvider.ResolveTag)
                .ToList();

            _database.AddEntry(new Entry<TData, TID>(data, tags));

        }

        public void Remove(IEnumerable<TID> ids)
        {
            var tags = ids
                .Select(_tagProvider.ResolveTag);

            _database.RemoveAll(tags);
        }


        public void Save()
        {
            _database.Save(_databasePath);
            _tagProvider.Save(_tagPath);
        }

        public void Load()
        {
            if (File.Exists(_tagPath))
            {
                _tagProvider.Load(_tagPath);
            }

            if (File.Exists(_databasePath))
            {
                _database.Load(_databasePath, _tagProvider);
            }
        }
    }
}
