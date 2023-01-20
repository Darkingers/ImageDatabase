namespace Database
{
    public interface IPersistent
    {
        string Save();
        void Load(string data);
    }
}
